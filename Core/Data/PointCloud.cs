using Fusee.Base.Core;
using Fusee.Engine.Core;
using Fusee.Tutorial.Core.Common;
using System.Collections.Generic;
using Fusee.Math.Core;
using Fusee.Tutorial.Core.Octree;
using static Fusee.Tutorial.Core.PointVisualizationBase;
using System.Threading.Tasks;
using System.Threading;

/**
 * Three different threads or tasks are involved:
 * 1. The render thread, which accesses the _pointsPerNode, in which the points are stored which should be rendered.
 * 2. The reading thread, which either downloads points via UDP or reads them from a file and builds up the octree.
 * 3. The preparing thread, which - whenever the camera view changes - collects up the nodes from the octree that are visible and thus should be rendered.
 *      It then stores the corresponding nodes in a list, which are then scheduled to be loaded into the GPU memory by the render thread.
 *      
 * Improvements that might speed up:
 * - Instead of _pointsPerNode.ContainsKey() we could just set a render flag for each node
 */

namespace Fusee.Tutorial.Core.Data
{
    /// <summary>
    /// Contains all the settings and variables needed for rendering the point cloud. Render context related programming is encapsulated in this class
    /// for better readability.
    /// </summary>
    public class PointCloud : RenderEntitiy
    {
        #region Fields

        // constants / settings

        private const int UPDATE_EVERY = 65000; // every xth point the point cloud should update its meshes
        private const int COMPUTE_EVERY = 1; // take only every xth point into account in order to speed up calculation

        // conditions of when to stop the traversal

        private const int POINT_BUDGET = 10000; // number of points that are visible at one frame, tradeoff between performance and quality
        private const float MIN_SCREEN_PROJECTED_SIZE = 1; // minimum screen size of the node
        private int NODES_TO_BE_LOADED_PER_SCHEDULE = 1000; // X = 5, see Schuetz (2016)

        private const int MAX_NODE_LEVEL = 8; // the maximum level of a node for the points in it to be loaded

        // shader params

        private static float _particleSize = 1f; // maybe gets changed from platform specific classes
        public const float ParticleSizeInterval = 0.25f;

        private float2 _zBounds = float2.Zero;
        private float _zoom = 60f;

        private float _aspectRatio = 1; // needed for particle size to be square, aspect ratio of viewport => see OnWindowResize

        // helper

        private int _pointCounter = 0;
        private int _visitPointCounter; // counts the points that have been scheduled to be loaded while traversing the octree

        // data structure

        // This is the object where new vertices are stored. Also look at the description of the class(es) for more information.
        private Dictionary<byte[], DynamicAttributes> _pointsPerNode = new Dictionary<byte[], DynamicAttributes>();

        // octree

        private Octree.Octree _octree;
        
        private List<OctreeNode> _unloadedVisibleNodes;
        private SortedDictionary<double, OctreeNode> _nodesOrdered = new SortedDictionary<double, OctreeNode>(); // nodes ordered by screen-projected-size

        // conditions on when to start a new traversing in screen-projected-size order
        private bool _traversingFinished = true; // determines whether the octree traversal has been already finished
        private int _levelToLoad = -1;

        public bool HasAssetLoaded = false;

        public bool StartNewTraversingForVisibleNodes = false; // decides, whether a new search for visible nodes should be started
        private bool _scheduleLoading = false;

        private Dictionary<byte[], DynamicAttributes> _nodesToBeLoaded = new Dictionary<byte[], DynamicAttributes>();
        private List<byte[]> _nodesToBeRemoved = new List<byte[]>();

        // octree traversing

        private float _screenHeight = 1; // needs to be set for calculating screen-projected-size for each octree node

        private double _fov = 3.141592f * 0.25f; // needs to be the same as in PointVisualizationBase.cs
        private double _slope;
        private float3 _cameraPosition;

        // debugging
        
        private DynamicAttributes _debuggingPoints;

        // multithreading

        private AutoResetEvent _signalEvent = new AutoResetEvent(true);
        
        #endregion
        
        #region Methods

        /// <summary>
        /// Constructor, which loads the shader programs and sets some references.
        /// </summary>
        /// <param name="rc">The render context.</param>
        /// <param name="boundingBox">Reference to the bounding box which is used throughout the program.</param>
        public PointCloud(RenderContext rc, Octree.Octree octree, BoundingBox boundingBox) : base(rc)
        { 
            boundingBox.UpdateCallbacks += OnBoundingBoxUpdate;
            _slope = System.Math.Tan(_fov / 2);
            
            Octree.Octree.OnNodeBucketChangedCallback += OnNodeBucketChanged;

            _octree = octree;
        }

        #region Shader related methods

        /// <summary>
        /// Create shader program with vertex and fragment shader for this render entity.
        /// </summary>
        protected override ShaderProgram CreateShaderProgram()
        {
            string vertsh = AssetStorage.Get<string>("VertexShaderPCL.vert");
            string pixsh = AssetStorage.Get<string>("PixelShaderPCL.frag");

            return _rc.CreateShader(vertsh, pixsh);
        }

        /// <summary>
        /// Sets the shader params for the point cloud.
        /// </summary>
        public override void SetShaderParams()
        {
            var particleSizeParam = _rc.GetShaderParam(_shader, "particleSize");
            _rc.SetShaderParam(particleSizeParam, new float2(_particleSize, _particleSize * _aspectRatio));

            var colorParam = _rc.GetShaderParam(_shader, "color");
            _rc.SetShaderParam(colorParam, new float3(0, 0, 0.5f));
            /*
            // SetZNearFarPlane
            var zBoundsParam = _rc.GetShaderParam(_shader, "zBounds");
            _rc.SetShaderParam(zBoundsParam, _zBounds);

            // SetZoomValue
            var zZoomParam = _rc.GetShaderParam(_shader, "zZoom");
            _rc.SetShaderParam(zZoomParam, _zoom);*/
        }

        /// <summary>
        /// Highlights the points to be rendered next by setting a different color and point size.
        /// </summary>
        /// <param name="active"></param>
        public void SetPointsActive(bool active)
        {
            float3 color = active ? new float3(1, 0, 0) : new float3(0, 0, 0.5f);

            var colorParam = _rc.GetShaderParam(_shader, "color");
            _rc.SetShaderParam(colorParam, color);

            var particleSizeParam = _rc.GetShaderParam(_shader, "particleSize");
            _rc.SetShaderParam(particleSizeParam, new float2(_particleSize + ParticleSizeInterval, (_particleSize + ParticleSizeInterval) * _aspectRatio));
        }

        #endregion

        #region Setter and Getter of Fields
        
        /// <summary>
        /// Increases the particle size.
        /// </summary>
        /// <param name="interval">How much the particle size should increase.</param>
        public static void IncreaseParticleSize(float interval = ParticleSizeInterval)
        {
            _particleSize += interval;
        }

        /// <summary>
        /// Decreases the particle size.
        /// </summary>
        /// <param name="interval">How much the particle size should decrease.</param>
        public static void DecreaseParticleSize(float interval = ParticleSizeInterval)
        {
            _particleSize -= interval;
        }

        /// <summary>
        /// Sets directly the particle size.
        /// </summary>
        /// <param name="particleSize">The particle size to set.</param>
        public static void SetParticleSize(float particleSize)
        {
            _particleSize = particleSize;
        }

        /// <summary>
        /// Sets the zoom value.
        /// </summary>
        public void SetZoomValue(float zoom)
        {
            _zoom = zoom;
        }
        
        /// <summary>
        /// Sets the position of the camera.
        /// </summary>
        public void SetCameraPosition(float3 position)
        {
            _cameraPosition = position;
        }

        /// <summary>
        /// When a new debugging node is set, this function is called.
        /// </summary>
        /// <param name="node">The node that is currently debugged.</param>
        public void OnNewDebuggingNode(OctreeNode node)
        {
            // remove old mesh

            if(_debuggingPoints != null)
            {
                _rc.Remove(_debuggingPoints);
            }

            _debuggingPoints = new DynamicAttributes(Octree.Octree.BucketThreshold);

            // create new mesh
            _debuggingPoints.AddAttributes(node.Bucket);
        }

        /// <summary>
        /// Gets called when Resize() on the render canvas gets called.
        /// </summary>
        /// <param name="renderCanvas">A reference to the render canvas.</param>
        public void OnResize(RenderCanvas renderCanvas)
        {
            _aspectRatio = renderCanvas.Width / (float) renderCanvas.Height;
            _screenHeight = renderCanvas.Height;
        }

        /// <summary>
        /// Adds another point to this point cloud.
        /// </summary>
        /// <param name="point">The point to add.</param>
        public void AddPoint(Common.Point point)
        {
            AddPoint(point.Position);
        }

        /// <summary>
        /// Adds another point to this point cloud.
        /// </summary>
        /// <param name="position">The position to add.</param>
        public void AddPoint(float3 position)
        {
            /*
            _pointCounter++;

            if (_pointCounter % COMPUTE_EVERY != 0 && COMPUTE_EVERY != 1)
                return;

            /*
            _meshList.AddMesh(new PointMesh(position));

            //*
            if (_pointCounter % UPDATE_EVERY == 0)
            {
                _meshList.Apply();
            }
            //*/

            _octree.Add(position);
        }

        #endregion

        #region Render

        /// <summary>
        /// Gets called every frame. Takes care of rendering the point cloud.
        /// </summary>
        /// <param name="viewMode">The view mode currently set.</param>
        /// <param name="level">The level to render. -1 for all levels together.</param>
        /// <param name="debuggingNode">The node to debug for.</param>
        public void Render(ViewModeDebugging viewMode, int level, OctreeNode debuggingNode)
        {
            base.Render();
            
            if(_scheduleLoading) // if traversing the octree for new nodes to be loaded has finished
            {
                // remove nodes
                foreach (byte[] nodePath in _nodesToBeRemoved)
                {
                    DynamicAttributes da = _pointsPerNode[nodePath];
                    _rc.Remove(da);

                    _pointsPerNode.Remove(nodePath);
                }

                // add nodes
                foreach(KeyValuePair<byte[], DynamicAttributes> kvp in _nodesToBeLoaded)
                {
                    if(_pointsPerNode.ContainsKey(kvp.Key))
                    {
                        _pointsPerNode[kvp.Key] = kvp.Value;
                    }
                    else
                    {
                        _pointsPerNode.Add(kvp.Key, kvp.Value);
                    }
                }

                _nodesToBeLoaded = new Dictionary<byte[], DynamicAttributes>();
                _nodesToBeRemoved = new List<byte[]>();

                _scheduleLoading = false; // loading finished
            }

            if(StartNewTraversingForVisibleNodes && _traversingFinished && !_scheduleLoading && level != -1)
            {
                CollectNodes(level);
            }

            // Render Point Cloud

            if(viewMode == ViewModeDebugging.PerLevel || viewMode == ViewModeDebugging.All)
            {
                _signalEvent.WaitOne();

                foreach (KeyValuePair<byte[], DynamicAttributes> kvp in _pointsPerNode) // for each node
                {
                    if ((viewMode == ViewModeDebugging.PerLevel || viewMode == ViewModeDebugging.PerNode) && level != kvp.Key.Length - 1)
                        continue;

                    _rc.RenderAsPoints(kvp.Value);
                }

                _signalEvent.Set();
            }
            
            // debugging octree node

            if (viewMode == ViewModeDebugging.PerNode)
            {
                SetPointsActive(true);
                _rc.RenderAsPoints(_debuggingPoints);
            }

            #endregion
        }

        #endregion

        #region Event Handler

        /// <summary>
        /// Gets called when bounding box updates.
        /// </summary>
        private void OnBoundingBoxUpdate(BoundingBox boundingBox)
        {
            _zBounds.x = boundingBox.GetMinValues().z;
            _zBounds.y = boundingBox.GetMaxValues().z;
        }

        #endregion

        #region Octree Methods
       
        /// <summary>
        /// Starts traversing the octree and collecting for nodes which should be visible next.
        /// </summary>
        /// <param name="level">For debugging purpose, just take the nodes of a given level as visible nodes.</param>
        private void CollectNodes(int level)
        {
            if (!_traversingFinished)
                return;

            StartNewTraversingForVisibleNodes = false;
            _traversingFinished = false;
            _levelToLoad = level;

            Task task = new Task(() =>
            {
                _visitPointCounter = 0;

                _nodesToBeLoaded = new Dictionary<byte[], DynamicAttributes>();
                _nodesToBeRemoved = new List<byte[]>();

                _octree.Traverse(OnNodeVisited);

                // schedule loading for visible but onloaded nodes => in Render()
                _traversingFinished = true;
                _scheduleLoading = true;
            });

            task.Start();
        }
        
        /// <summary>
        /// Gets called when traversing the octree.
        /// </summary>
        /// <param name="node">The node currently visited.</param>
        private void OnNodeVisited(OctreeNode node)
        {
            if(node.GetLevel() == _levelToLoad)
            {
                // if not already rendered, add this node to the nodes-to-be-rendered-list
                if(!_pointsPerNode.ContainsKey(node.Path))
                {
                    _visitPointCounter += node.Bucket.Count;

                    DynamicAttributes points = new DynamicAttributes(Octree.Octree.BucketThreshold);
                    points.AddAttributes(node.Bucket);
                    
                    _nodesToBeLoaded.Add(node.Path, points);
                }
                else if(node.HasBucketChanged)
                {
                    DynamicAttributes points = _pointsPerNode[node.Path];
                    points.Reset();
                    
                    _visitPointCounter += node.Bucket.Count - points.GetCount();

                    _nodesToBeLoaded.Add(node.Path, points);
                }
            }
            else 
            {
                // if already rendered, but shouldn't be, add this node to the nodes-to-be-removed-list

                if (_pointsPerNode.ContainsKey(node.Path))
                {
                    _nodesToBeRemoved.Add(node.Path);
                }
            }
        }

        /*
        /// <summary>
        /// Traverses the octree and searches for
        /// </summary>
        /// <param name="rootNode"></param>
        private void TraverseByProjectionSizeOrder(OctreeNode rootNode)
        {
            ProcessNode(rootNode);

            while (!(_nodesOrdered.Count == 0 || _visitPointCounter > POINT_BUDGET || _unloadedVisibleNodes.Count > NODESTOBELOADEDPERSCHEDULE)) // abbruchbedingungen
            {
                // choose the nodes with the biggest screen size overall to process next

                KeyValuePair<double, OctreeNode> biggestNode = _nodesOrdered.Last();
                _nodesOrdered.Remove(biggestNode.Key);

                ProcessNode(biggestNode.Value);
            }
        }

        /// <summary>
        /// Sub function which calculates the screen-projected-size and adds it to the heap of nodesOrdered by pss.
        /// And call its callback (OnNodeVisited);
        /// </summary>
        /// <param name="node">The node to compute the pss for.</param>
        private void ProcessNode(OctreeNode node)
        {
            // process the node
            OnNodeVisited(node);

            // add child nodes to the heap of ordered nodes

            if (node.hasChildren())
            {
                foreach (OctreeNode childNode in node.Children)
                {
                    // compute screen projected size

                    //float3 nodePosition = _rc.ModelView * childNode.CenterPosition;
                    //var distance = nodePosition.Length;
                    var distance = float3.Subtract(childNode.CenterPosition, _cameraPosition).Length;

                    var projectedSize = _screenHeight / 2 * childNode.SideLength / (_slope * distance);

                    // is it below minimum or outside the view frustum => cancel

                    if (projectedSize < MINSCREENSIZE) // || liegt nicht im view frustum
                    {
                        childNode.RenderFlag = OctreeNodeStates.NonVisible;
                        continue;
                    }

                    if (!_nodesOrdered.ContainsKey(projectedSize))
                    {
                        _nodesOrdered.Add(projectedSize, childNode);
                    }
                }
            }
        }
        */
        
        /// <summary>
        /// Gets called when the bucket of a node has changed. (Means more points have been added or removed from this node.)
        /// </summary>
        /// <param name="node">The node which bucket has changed.</param>
        private void OnNodeBucketChanged(OctreeNode node)
        {
            int level = node.GetLevel();

            if (level > MAX_NODE_LEVEL || level != _levelToLoad)
                return;

            StartNewTraversingForVisibleNodes = true;
        }

        #endregion
    }
}
