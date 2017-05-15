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
 * - In the render cycle all the data is created. One could might do this while octree traversal.
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
        
        private const int MAX_NODE_LEVEL = 8; // the maximum level of a node for the points in it to be loaded

        // shader params

        private static float _particleSize = 1f; // maybe gets changed from platform specific classes
        public const float ParticleSizeInterval = 0.25f;

        private float2 _zBounds = float2.Zero;
        private float _zoom = 60f;

        private float _aspectRatio = 1; // needed for particle size to be square, aspect ratio of viewport => see OnWindowResize
                
        // data structure

        // This is the object where new vertices are stored. Also look at the description of the class(es) for more information.
        private Dictionary<byte[], DynamicAttributes> _pointsPerNode = new Dictionary<byte[], DynamicAttributes>();
        
        // debugging
        
        private DynamicAttributes _debugPoints;
        private List<DynamicAttributes> _snapshotPoints;
        
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
            _rc.SetShaderParam(particleSizeParam, new float2(_particleSize + 2 * ParticleSizeInterval, (_particleSize + 2 * ParticleSizeInterval) * _aspectRatio));
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
        /// Gets called when Resize() on the render canvas gets called.
        /// </summary>
        /// <param name="renderCanvas">A reference to the render canvas.</param>
        public void OnResize(RenderCanvas renderCanvas)
        {
            _aspectRatio = renderCanvas.Width / (float) renderCanvas.Height;
        }

        #endregion

        #region Render

        /// <summary>
        /// Gets called every frame. Takes care of rendering the wireframe of all octree nodes.
        /// </summary>
        public override void Render()
        {
            base.Render();

            foreach (KeyValuePair<byte[], DynamicAttributes> kvp in _pointsPerNode) // for each level
            {
                _rc.RenderAsPoints(kvp.Value);
            }
        }

        /// <summary>
        /// Renders the wireframes of the nodes of a specified level.
        /// </summary>
        /// <param name="level">The octree level to render nodes from.</param>
        public void Render(int level)
        {
            base.Render();

            foreach (KeyValuePair<byte[], DynamicAttributes> kvp in _pointsPerNode) // for each level
            {
                if (level != kvp.Key.Length - 1)
                    continue;

                _rc.RenderAsPoints(kvp.Value);
            }
        }

        /// <summary>
        /// Render method that gets called when only a specific node is desired along with a specified octree level.
        /// </summary>
        /// <param name="level">The level of which other nodes should be rendered too.</param>
        /// <param name="debugNode">The node which should be highlighted.</param>
        public void Render(int level, OctreeNode debugNode)
        {
            base.Render();

            Render(level);

            SetPointsActive(true);
            _rc.RenderAsPoints(_debugPoints);
        }

        /// <summary>
        /// Takes the current snapshot and renders it.
        /// </summary>
        public void RenderSnapshot()
        {
            base.Render();

            if (_snapshotPoints != null)
            {
                foreach (DynamicAttributes da in _snapshotPoints)
                    _rc.RenderAsPoints(da);
            }
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
        
        #region Data Model

        /// <summary>
        /// Adds a new node to render. If already existent, it gets changed.
        /// </summary>
        public void AddNode(OctreeNode node)
        {
            if (node.GetLevel() > MAX_NODE_LEVEL)
                return;

            if (!_pointsPerNode.ContainsKey(node.Path))
            {
                DynamicAttributes points = new DynamicAttributes(Octree.Octree.BucketThreshold);
                points.AddAttributes(node.Bucket);

                _pointsPerNode.Add(node.Path, points);
            }
        }

        /// <summary>
        /// Removes the specified node from the list of rendered nodes.
        /// </summary>
        public void RemoveNode(OctreeNode node)
        {
            if (_pointsPerNode.ContainsKey(node.Path))
            {
                DynamicAttributes da = _pointsPerNode[node.Path];
                _rc.Remove(da);

                _pointsPerNode.Remove(node.Path);
            }
        }

        /// <summary>
        /// Sets all the points stored in memory away.
        /// </summary>
        public void Reset()
        {
            foreach(KeyValuePair<byte[], DynamicAttributes> kvp in _pointsPerNode)
            {
                _rc.Remove(kvp.Value);
            }

            _pointsPerNode = new Dictionary<byte[], DynamicAttributes>();
        }

        /// <summary>
        /// Takes the current state and keeps it until its released.
        /// </summary>
        public void TakeSnapshot()
        {
            _snapshotPoints = new List<DynamicAttributes>();

            foreach (KeyValuePair<byte[], DynamicAttributes> kvp in _pointsPerNode)
            {
                // must have different buffer object id when removing
                DynamicAttributes da = new DynamicAttributes(Octree.Octree.BucketThreshold);
                da.AddAttributes(kvp.Value.GetOffsets());
                
                _snapshotPoints.Add(da);
            }
        }

        /// <summary>
        /// Removes the old snapshot.
        /// </summary>
        public void ReleaseSnapshot()
        {
            if (_snapshotPoints == null)
                return;

            foreach (DynamicAttributes da in _snapshotPoints)
            {
                _rc.Remove(da);
            }

            _snapshotPoints = null;
        }

        /// <summary>
        /// When a new debugging node is set, this function is called.
        /// </summary>
        /// <param name="node">The node that is currently debugged.</param>
        public void SetNewDebuggingNode(OctreeNode node)
        {
            // remove old mesh

            if (_debugPoints != null)
                _rc.Remove(_debugPoints);

            _debugPoints = new DynamicAttributes(Octree.Octree.BucketThreshold);

            // create new mesh
            _debugPoints.AddAttributes(node.Bucket);
        }

        #endregion

        #endregion
    }
}
