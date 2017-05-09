using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using Fusee.Tutorial.Core.Common;
using Fusee.Tutorial.Core.Octree;

namespace Fusee.Tutorial.Core.Data
{
    /// <summary>
    /// Contains all the settings and variables needed for rendering the voxel space. Render context related programming is encapsulated in this class
    /// for better readability.
    /// </summary>
    public class VoxelSpace : RenderEntitiy
    {
        #region Fields
        
        private const float VOXEL_SIZE = 1;
        private const int COMPUTE_EVERY = 1; // take only every xth point into account in order to speed up calculation
        
        private IShaderParam _yBoundsParam;
        private float2 _yBounds;

        // TO-DO: set shader param for voxelsize?
        
        // This is the object where new vertices are stored. Also look at the description of the class(es) for more information.
        private DynamicAttributes _positions = new DynamicAttributes(); // no need for separation of buffers
        
        // multiple cubes will be rendered on different positions
        private Cube _cube = new Cube();

        // An octree for better searchability, of when to add a new voxel.
        private Octree<OctreeNodeStates> _octree;
        
        private int _pointCounter = 0;
        
        #endregion

        #region Methods

        /// <summary>
        /// Constructor, which loads the shader programs.
        /// </summary>
        /// <param name="boundingBox">Needs a reference to the bounding box for rendering.</param>
        /// <param name="rc">The render context.</param>
        public VoxelSpace(RenderContext rc, BoundingBox boundingBox) : base(rc)
        {
            boundingBox.UpdateCallbacks += OnBoundingBoxUpdate;
            
            _octree = new Octree<OctreeNodeStates>(float3.Zero, VOXEL_SIZE);
            _octree.OnNodeAddedCallback += OnNewNodeAdded;
        }

        /// <summary>
        /// Create shader program with vertex and fragment shader for this render entity.
        /// </summary>
        protected override ShaderProgram CreateShaderProgram()
        {
            string vertsh = AssetStorage.Get<string>("VertexShaderVSP.vert");
            string pixsh = AssetStorage.Get<string>("PixelShaderVSP.frag");

            return _rc.CreateShader(vertsh, pixsh);
        }

        /// <summary>
        /// Event handler for when a new node gets added to the octree.
        /// </summary>
        /// <param name="node">The node that was added.</param>
        private void OnNewNodeAdded(OctreeNode<OctreeNodeStates> node)
        {
            if (node.Data == OctreeNodeStates.Occupied && node.SideLength == VOXEL_SIZE)
            {
                _positions.AddAttribute(node.Position);
            }
        }

        /// <summary>
        /// Adds another point for the octree.
        /// </summary>
        /// <param name="point">The point to add.</param>
        public void AddPoint(Common.Point point)
        {
            _pointCounter++;

            if (_pointCounter % COMPUTE_EVERY != 0 && COMPUTE_EVERY != 1)
                return;
            
            _octree.Add(point.Position, OctreeNodeStates.Occupied);
        }

        /// <summary>
        /// Overload method for <see cref="AddPoint(Point)"/>. 
        /// </summary>
        /// <param name="position"></param>
        public void AddPoint(float3 position)
        {
            _pointCounter++;

            if (_pointCounter % COMPUTE_EVERY != 0 && COMPUTE_EVERY != 1)
                return;

            _octree.Add(position, OctreeNodeStates.Occupied);
        }

        /// <summary>
        /// Gets called every frame. Takes care of rendering the voxel space.
        /// </summary>
        public override void Render()
        {
            base.Render();
            _rc.RenderAsInstance(_cube, _positions);
        }

        /// <summary>
        /// Sets the shader params for the point cloud.
        /// </summary>
        public override void SetShaderParams()
        {
            _yBoundsParam = _rc.GetShaderParam(_shader, "yBounds");
            _rc.SetShaderParam(_yBoundsParam, _yBounds);
        }

        /// <summary>
        /// Updates the y-Bounds for color rendering.
        /// </summary>
        /// <param name="boundingBox"></param>
        private void OnBoundingBoxUpdate(BoundingBox boundingBox)
        {
            _yBounds.x = boundingBox.GetMinValues().y;
            _yBounds.y = boundingBox.GetMaxValues().y;
        }

        #endregion
    }
}
