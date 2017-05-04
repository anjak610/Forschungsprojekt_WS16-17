using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using Fusee.Tutorial.Core.Octree;

namespace Fusee.Tutorial.Core.PointClouds
{
    /// <summary>
    /// Contains all the settings and variables needed for rendering the voxel space. Render context related programming is encapsulated in this class
    /// for better readability.
    /// </summary>
    public class VoxelSpace
    {
        #region Fields

        #region Constants

        private const float VOXEL_SIZE = 1;
        private const int COMPUTE_EVERY = 1; // take only every xth point into account in order to speed up calculation
        
        #endregion

        #region Shader Params

        public readonly string VertexShader, PixelShader;

        private IShaderParam _yBoundsParam;
        private float2 _yBounds;

        // TO-DO: set shader param for voxelsize?

        #endregion

        #region Data

        // This is the object where new vertices are stored. Also look at the description of the class(es) for more information.
        //private DynamicAttributes _positions = new DynamicAttributes(); // no need for separation of buffers
        
        // multiple cubes will be rendered on different positions
        private Cube _cube = new Cube();

        // An octree for better searchability, of when to add a new voxel.
        private Octree<OctreeNodeStates> _octree;

        #endregion

        #region Other

        private int _pointCounter = 0;

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Constructor, which loads the shader programs.
        /// </summary>
        /// <param name="boundingBox">Needs a reference to the bounding box for rendering.</param>
        public VoxelSpace(BoundingBox boundingBox)
        {
            boundingBox.UpdateCallbacks += OnBoundingBoxUpdate;

            VertexShader = AssetStorage.Get<string>("VertexShaderVSP.vert");
            PixelShader = AssetStorage.Get<string>("PixelShaderVSP.frag");

            _octree = new Octree<OctreeNodeStates>(float3.Zero, VOXEL_SIZE);
            _octree.OnNodeAddedCallback += OnNewNodeAdded;
        }

        /// <summary>
        /// Event handler for when a new node gets added to the octree.
        /// </summary>
        /// <param name="node">The node that was added.</param>
        private void OnNewNodeAdded(OctreeNode<OctreeNodeStates> node)
        {
            if (node.Data == OctreeNodeStates.Occupied && node.SideLength == VOXEL_SIZE)
            {
              //  _positions.AddAttribute(node.Position);
            }
        }

        /// <summary>
        /// Adds another point for the octree.
        /// </summary>
        /// <param name="point">The point to add.</param>
        public void AddPoint(Point point)
        {
            _pointCounter++;

            if (_pointCounter % COMPUTE_EVERY != 0 && COMPUTE_EVERY != 1)
                return;
            
            _octree.Add(point.Position, OctreeNodeStates.Occupied);
        }

        /// <summary>
        /// Gets called every frame. Takes of rendering the point cloud.
        /// </summary>
        /// <param name="rc">The render context.</param>
        public void Render(RenderContext rc)
        {
          //  rc.RenderAsInstance(_cube, _positions);
        }

        /// <summary>
        /// Sets the shader params for the point cloud.
        /// </summary>
        /// <param name="rc">The render context.</param>
        public void SetShaderParams(RenderContext rc, ShaderProgram shader)
        {
            _yBoundsParam = rc.GetShaderParam(shader, "yBounds");
            rc.SetShaderParam(_yBoundsParam, _yBounds);
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
