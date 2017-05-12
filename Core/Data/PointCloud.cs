using Fusee.Base.Core;
using Fusee.Engine.Core;
using Fusee.Tutorial.Core.Common;
using System.Collections.Generic;
using Fusee.Math.Core;
using Fusee.Tutorial.Core.Octree;
using System.Linq;
using System.Threading.Tasks;

namespace Fusee.Tutorial.Core.Data
{
    /// <summary>
    /// Contains all the settings and variables needed for rendering the point cloud. Render context related programming is encapsulated in this class
    /// for better readability.
    /// </summary>
    public class PointCloud : RenderEntitiy
    {
        // choose traversing between levels or single nodes
        public enum ViewMode
        {
            PerLevel, PerNode
        }

        #region Fields

        private const int UPDATE_EVERY = 100; // every xth point the point cloud should update its meshes
        private const int COMPUTE_EVERY = 10; // take only every xth point into account in order to speed up calculation

        private static float _particleSize = 0.05f; // maybe gets changed from platform specific classes
        public const float ParticleSizeInterval = 0.025f;

        public static ViewMode _viewMode = ViewMode.PerLevel;
        private static OctreeNode _debuggingNode;

        private int _pointCounter = 0;

        private float _aspectRatio = 1; // needed for particle size to be square, aspect ratio of viewport => see OnWindowResize
        private float _screenHeight = 1; // needs to be set for calculating screen-projected-size for each octree node

        private float2 _zBounds = float2.Zero;
        private float _zoom = 60f;

        // This is the object where new vertices are stored. Also look at the description of the class(es) for more information.
        private StaticMeshList _meshList = new StaticMeshList();

        private bool _wireframeVisible = true;

        #region Octree

        // Renders the bounding boxes for each node
        private Wireframe _wireFrame;

        // See Schuetz (2016): Potree 3.4 Octree Traversal and Visible Node Determination
        private Octree.Octree _octree;

        // conditions of when to stop the traversal
        private const int POINT_BUDGET = 10000; // number of points that are visible at one frame, tradeoff between performance and quality
        private const float MINSCREENSIZE = 5; // minimum screen size of the node
        private int NODESTOBELOADEDPERSCHEDULE = 65000; // X = 5, see Schuetz (2016)

        private int _visitPointCounter; // counts the points that have been visited

        private List<OctreeNode> _unloadedVisibleNodes;
        private bool _traversingFinished = true;

        private SortedDictionary<double, OctreeNode> _nodesOrdered = new SortedDictionary<double, OctreeNode>(); // nodes ordered by screen-projected-size

        public bool HasAssetLoaded = false;

        private double _fov = 3.141592f * 0.25f; // needs to be the same as in PointVisualizationBase.cs
        private double _slope;
        private float3 _cameraPosition;

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Constructor, which loads the shader programs and sets some references.
        /// </summary>
        /// <param name="rc">The render context.</param>
        /// <param name="boundingBox">Reference to the bounding box which is used throughout the program.</param>
        public PointCloud(RenderContext rc, BoundingBox boundingBox) : base(rc)
        {
            boundingBox.UpdateCallbacks += OnBoundingBoxUpdate;

            _octree = new Octree.Octree(float3.Zero);
            _wireFrame = new Wireframe(rc, _octree);

            _octree.Init(256);
            
            _slope = System.Math.Tan(_fov / 2);
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
            /*
            // SetZNearFarPlane
            var zBoundsParam = _rc.GetShaderParam(_shader, "zBounds");
            _rc.SetShaderParam(zBoundsParam, _zBounds);

            // SetZoomValue
            var zZoomParam = _rc.GetShaderParam(_shader, "zZoom");
            _rc.SetShaderParam(zZoomParam, _zoom);*/
        }

        #endregion

        #region Setter and Getter of Fields

        /// <summary>
        /// Switches between the different view modes.
        /// </summary>
        public void SwitchViewMode()
        {
            ViewMode nextViewMode = _viewMode == ViewMode.PerLevel ? ViewMode.PerNode : ViewMode.PerLevel;
            _viewMode = nextViewMode;

            if(_viewMode == ViewMode.PerNode)
            {
                // start debugging via traversing octree
                _debuggingNode = _octree.GetRootNode();
            }
        }

        /// <summary>
        /// Whether mode either traversing per level or per single node is active.
        /// </summary>
        public static ViewMode GetCurrentViewMode()
        {
            return _viewMode;
        }

        /// <summary>
        /// Returns the current debugging node.
        /// </summary>
        public static OctreeNode GetCurrentDebuggingNode()
        {
            return _debuggingNode;
        }

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
        /// Sets the current screen height.
        /// </summary>
        public void SetScreenHeight(float screenHeight)
        {
            _screenHeight = screenHeight;
        }

        /// <summary>
        /// Gets called when viewport changes. Sets the aspect ratio
        /// </summary>
        /// <param name="aspectRatio">width / height</param>
        public void SetAspectRatio(float aspectRatio)
        {
            _aspectRatio = aspectRatio;
        }

        /// <summary>
        /// Sets the position of the camera.
        /// </summary>
        public void SetCameraPosition(float3 position)
        {
            _cameraPosition = position;
        }

        /// <summary>
        /// Demands the wireframe and the point cloud only to render a specific level.
        /// </summary>
        public void LevelUp()
        {
            if(_viewMode == ViewMode.PerLevel)
            {
                _wireFrame.LevelUp();
            }
            else
            {
                if (_debuggingNode.Parent != null)
                    _debuggingNode = _debuggingNode.Parent;
            }
        }

        /// <summary>
        /// Demands the wireframe and the point cloud only to render a specific level.
        /// </summary>
        public void LevelDown()
        {
            if (_viewMode == ViewMode.PerLevel)
            {
                _wireFrame.LevelDown();
            }
            else
            {
                if (_debuggingNode.hasChildren())
                    _debuggingNode = _debuggingNode.Children[0];
            }
        }

        /// <summary>
        /// Sets the debugging node to its previous sibling if existent.
        /// </summary>
        public void DebugPreviousSibling()
        {
            byte index = (byte)_debuggingNode.Path.Last();

            if (index > 0)
            {
                _debuggingNode = _debuggingNode.Parent.Children[index - 1];
            }
        }

        /// <summary>
        /// Sets the debugging node to its previous sibling if existent.
        /// </summary>
        public void DebugNextSibling()
        {
            byte index = (byte) _debuggingNode.Path.Last();

            if (index < _debuggingNode.Parent.Children.Length - 1)
            {
                _debuggingNode = _debuggingNode.Parent.Children[index + 1];
            }
        }

        /// <summary>
        /// Adds another point to this point cloud.
        /// </summary>
        /// <param name="point">The point to add.</param>
        public void AddPoint(Common.Point point)
        {
            AddPoint(point.Position);
        }

        public void SwitchWireframe()
        {
            _wireframeVisible = !_wireframeVisible;
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
        public override void Render()
        {
            base.Render();

            // handle octree 

            if (_traversingFinished && HasAssetLoaded)
            {
                if (_unloadedVisibleNodes != null && _unloadedVisibleNodes.Count > 0)
                {
                    for (var i = 0; i < NODESTOBELOADEDPERSCHEDULE; i++)
                    {
                        if (i > _unloadedVisibleNodes.Count - 1)
                            break;

                        OctreeNode node = _unloadedVisibleNodes[i];

                        foreach (float3 point in node.Bucket)
                        {
                            PointMesh pointMesh = new PointMesh(point);
                            _meshList.AddMesh(pointMesh);

                            _pointCounter++;
                            if (_pointCounter % UPDATE_EVERY == 0)
                            {
                                _meshList.Apply();
                            }
                        }

                        node.RenderFlag = OctreeNodeStates.Visible;
                    }
                }

                RenderOctree();
            }

            // render points

            List<Mesh> meshesToRemove = _meshList.GetMeshesToRemove();
            for (var i = 0; i < meshesToRemove.Count; i++)
            {
                _rc.Remove(meshesToRemove[i]);
            }

            List<Mesh> meshes = _meshList.GetMeshes();
            for (var i = 0; i < meshes.Count; i++)
            {
                _rc.Render(meshes[i]);
            }

            // render wireframe

            if(_wireframeVisible)
                _wireFrame.Render();
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
        /// Traverses the octree and prints for each node some debug line.
        /// </summary>
        public void RenderOctree()
        {
            _traversingFinished = false;

            Task task = new Task(() =>
            {

                _nodesOrdered = new SortedDictionary<double, OctreeNode>();
                _visitPointCounter = 0;

                _unloadedVisibleNodes = new List<OctreeNode>();

                _octree.TraverseWithoutCallback(TraverseByProjectionSizeOrder);

                // schedule loading for visible but onloaded nodes => in Render()
                _traversingFinished = true;
            });

            task.Start();
        }

        /// <summary>
        /// Gets called when traversing in screen-projected-size order is running.
        /// </summary>
        /// <param name="node">The node currently visited.</param>
        private void OnNodeVisited(OctreeNode node)
        {
            // This is the function where the nodes are visited in screen-projected-size order
            //Diagnostics.Log(node.SideLength);

            if (node.RenderFlag != OctreeNodeStates.Visible && node.SideLength > 15)
            {
                _visitPointCounter += node.Bucket.Count;
                node.RenderFlag = OctreeNodeStates.VisibleButUnloaded;

                _unloadedVisibleNodes.Add(node);
            }
        }

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

        #endregion

        #endregion
    }
}
