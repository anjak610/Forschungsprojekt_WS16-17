using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using Fusee.Tutorial.Core.Common;
using Fusee.Tutorial.Core.Octree;
using System.Threading;

namespace Fusee.Tutorial.Core.Data
{
    /// <summary>
    /// Contains all the settings and variables needed for rendering the voxel space. Render context related programming is encapsulated in this class
    /// for better readability.
    /// </summary>
    public class VoxelSpace : RenderEntity
    {
        #region Fields
        
        private float VOXEL_SIZE_AT_LEVEL = 8;

        private IShaderParam _yBoundsParam;
        private float2 _yBounds;

        // TO-DO: set shader param for voxelsize?
        
        // This is the object where new vertices are stored. Also look at the description of the class(es) for more information.
        private DynamicAttributes _positions = new DynamicAttributes(); // no need for separation of buffers
        
        // multiple cubes will be rendered on different positions
        private Cube _cube = new Cube();
        private bool _cubeSideLengthHasInitialized = false;

        // multithreading
        private AutoResetEvent _signalEvent = new AutoResetEvent(true);

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
            
            Octree.Octree.OnNodeAddedCallback += OnNewNodeAdded;
            Octree.Octree.OnOctreeNodeLevelsChangedCallback += OnOctreeHasGrown;
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
        private void OnNewNodeAdded(OctreeNode node)
        {
            if (node.GetLevel() == VOXEL_SIZE_AT_LEVEL)
            {
                _signalEvent.WaitOne();
                _positions.AddAttribute(node.CenterPosition);
                _signalEvent.Set();
                
                // initialize cube side length

                if(!_cubeSideLengthHasInitialized)
                {
                    _cubeSideLengthHasInitialized = true;

                    for(var i=0; i<_cube.Vertices.Length; i++)
                    {
                        _cube.Vertices[i] *= node.SideLength;
                    }
                }
            }
        }

        /// <summary>
        /// Gets called when the octree grows and thus the node's level change.
        /// </summary>
        private void OnOctreeHasGrown()
        {
            VOXEL_SIZE_AT_LEVEL++;
        }

        /// <summary>
        /// Gets called every frame. Takes care of rendering the voxel space.
        /// </summary>
        public override void Render()
        {
            base.Render();

            _signalEvent.WaitOne();
            _rc.RenderAsInstance(_cube, _positions);
            _signalEvent.Set();
        }

        /// <summary>
        /// Should set member variables which store a handle to the uniform variables in the shader.
        /// </summary>
        protected override void GetShaderParams()
        {
            _yBoundsParam = _rc.GetShaderParam(_shader, "yBounds");
        }

        /// <summary>
        /// Sets the shader params for the point cloud.
        /// </summary>
        protected override void SetShaderParams()
        {
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
