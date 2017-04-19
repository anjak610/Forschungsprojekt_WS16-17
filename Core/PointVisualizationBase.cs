using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using static Fusee.Engine.Core.Input;
using Fusee.Tutorial.Core.Common;
using Fusee.Base.Common;
using System.Collections.Generic;
using Fusee.Tutorial.Core.Octree;
using Fusee.Tutorial.Core.PointCloud;
using Fusee.Tutorial05.Core.Common;

namespace Fusee.Tutorial.Core
{
    [FuseeApplication(Name = "Forschungsprojekt", Description = "HFU Wintersemester 16-17")]
    public class PointVisualizationBase : RenderCanvas
    {
        public enum ViewMode
        {
            PointCloud, VoxelSpace
        }

        #region Fields

        // --- constants

        private const float VOXEL_SIZE = 1;
        private const int COMPUTE_EVERY = 1; // take only every xth point into account in order to speed up calculation
        private const int UPDATE_PCL_EVERY = 1000; // every xth point the point cloud should update its meshes
        
        public float ParticleSize = 0.05f; // maybe gets changed from platform specific classes

        // connection details to receive from

        private const int UDP_PORT = 8001;

        // helper variables

        private int _pointCounter = 0;
        private ViewMode _viewMode = ViewMode.PointCloud;

        // --- mesh data / pointcloud / voxelspace

        /*
        private InstanceAttributes _iaPCL = new InstanceAttributes();
        private InstanceAttributes _iaVSP = new InstanceAttributes();

        private List<float3> _positionsPCL = new List<float3>();
        private List<float3> _positionsVSP = new List<float3>();
        //*/

        private DynamicMesh _pointCloud = new DynamicMesh();
        private DynamicMesh _voxelSpace = new DynamicMesh();

        private PointMesh _pointMesh = new PointMesh(float3.Zero);
        private Cube _cube = new Cube();

        private BoundingBox _boundingBox;
        Octree<OctreeNodeStates> _octree;

        // --- shader params

        private string vertsh_PCL, vertsh_VSP, pixsh_PCL, pixsh_VSP;
        
        private IShaderParam _yBoundsParam;
        private float2 _yBounds = float2.Zero;

        private IShaderParam _particleSizeParam;

        // --- camera positioning

        private float _rotationY = (float) System.Math.PI;
        private float _rotationX = (float) System.Math.PI / -8;

        private float3 _cameraPosition = new float3(0, 0, -5.0f);
        private float3 _cameraPivot = new float3(0, 0, 0);
        private float3 _cameraPivotOffset = new float3(0, 0, 0);

        private float _minAngleX = (float) -System.Math.PI / 4;
        private float _maxAngleX = (float) System.Math.PI / 4;

        #endregion

        /// <summary>
        /// Initializes variables (very good summary).
        /// </summary>
        public override void Init()
        {
            // --- 1. Start loading resources

            // octree
            
            _octree = new Octree<OctreeNodeStates>(float3.Zero, VOXEL_SIZE);
            _octree.OnNodeAddedCallback += OnNewNodeAdded;
            
            // bounding box
            
            _boundingBox = new BoundingBox();
            _boundingBox.UpdateCallbacks += OnBoundingBoxUpdate;

            // pointcloud and voxelspace

            // TO-DO: set shader param for voxelsize

            // callbacks for various readings and stuff

            PointCloudReader.OnNewPointCallbacks += OnNewPointAdded;
            PointCloudReader.OnAssetLoadedCallbacks += OnAssetLoaded;

            // start loading points
                        
            PointCloudReader.ReadFromAsset("PointCloud_IPM.txt");
            //PointCloudReader.ReceiveFromUDP(UDP_PORT); // for unity game or other

            // --- 2. Set RC/GL related variables

            // read shaders from files
            
            vertsh_PCL = AssetStorage.Get<string>("VertexShaderPCL.vert");
            pixsh_PCL = AssetStorage.Get<string>("PixelShaderPCL.frag");
            vertsh_VSP = AssetStorage.Get<string>("VertexShaderVSP.vert");
            pixsh_VSP = AssetStorage.Get<string>("PixelShaderVSP.frag");
            
            SetViewMode(_viewMode);

            // Set the clear color for the backbuffer
            RC.ClearColor = new float4(0.95f, 0.95f, 0.95f, 1);
        }

        #region Rendering

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            // check on key down

            if(Keyboard.IsKeyDown(KeyCodes.T))
            {
                ViewMode nextViewMode = _viewMode == ViewMode.PointCloud ? ViewMode.VoxelSpace : ViewMode.PointCloud;
                SetViewMode(nextViewMode);
            }

            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            // Clear GPU memory
            ClearDynamicMeshes();

            // Render
            RC.ModelView = MoveInScene();

            List<Mesh> meshes = _viewMode == ViewMode.PointCloud ? _pointCloud.GetMeshes() : _voxelSpace.GetMeshes();
            for(var i=0; i<meshes.Count; i++)
            {
                RC.Render(meshes[i]);
            }

            /*
            if(_viewMode == ViewMode.PointCloud)
            {
                _iaPCL.AddOffsets(_positionsPCL);
                _positionsPCL = new List<float3>();

                RC.RenderAsInstance(_pointMesh, _iaPCL);
            }
            else
            {
                _iaVSP.AddOffsets(_positionsVSP);
                _positionsVSP = new List<float3>();

                RC.SetShaderParam(_yBoundsParam, _yBounds);
                RC.RenderAsInstance(_cube, _iaVSP);
            }
            //*/

            // Swap buffers: Show the contents of the backbuffer (containing the currently rendered frame) on the front buffer.
            Present();
        }
        
        private float4x4 MoveInScene()
        {
            // set origin to camera pivot
            float4x4 xform = float4x4.CreateTranslation(-1 * ( _cameraPivot + _cameraPivotOffset ));
            
            // rotate around camera pivot
            if (Mouse.LeftButton || Touch.ActiveTouchpoints == 1)
            {
                float2 speed = Mouse.Velocity + Touch.GetVelocity(TouchPoints.Touchpoint_0);

                _rotationY -= speed.x * 0.0001f;
                _rotationX -= speed.y * 0.0001f;

                // clamp rotation x between min and max angle
                _rotationX = _rotationX < _minAngleX ? _minAngleX : (_rotationX > _maxAngleX ? _maxAngleX : _rotationX);
            }

            var rotation = float4x4.CreateRotationX(_rotationX) * float4x4.CreateRotationY(_rotationY);
            xform = rotation * xform;

            // --- move camera position
            
            if (Mouse.Wheel != 0 || Touch.TwoPoint)
            {
                float speed = Mouse.WheelVel + Touch.TwoPointDistanceVel * 0.1f;
                _cameraPosition.z += speed * 0.1f;
            }

            xform = float4x4.CreateTranslation(-1 * _cameraPosition) * xform;
            
            // --- move camera pivot
            /*
            float3 translation = float3.Zero;

            if (Mouse.RightButton || Touch.TwoPoint)
            {
                float2 speed = Mouse.Velocity + Touch.TwoPointMidPointVel;
                translation.x = speed.x * -0.005f;
                translation.y = speed.y * 0.005f;
            }
            
            if (translation.Length > 0)
            {
                rotation.Invert();
                translation = rotation * translation;
                _cameraPivotOffset += translation;
                //_xform = float4x4.CreateTranslation(translation) * _xform;
            }
            */
            // --- projection matrix

            return xform;
        }

        // Is called when the window was resized
        public override void Resize()
        {
            // Set the new rendering area to the entire new windows size
            RC.Viewport(0, 0, Width, Height);

            // Create a new projection matrix generating undistorted images on the new aspect ratio.
            float aspectRatio = Width / (float) Height;
            
            // 0.25*PI Rad -> 45° Opening angle along the vertical direction. Horizontal opening angle is calculated based on the aspect ratio
            // Front clipping happens at 1 (Objects nearer than 1 world unit get clipped)
            // Back clipping happens at 2000 (Anything further away from the camera than 2000 world units gets clipped, polygons will be cut)
            
            RC.Projection = float4x4.CreatePerspectiveFieldOfView(3.141592f * 0.25f, aspectRatio, 1, 20000);
        }

        #endregion

        #region Helper

        /// <summary>
        /// Clears the meshes and the corresponding GPU memory.
        /// </summary>
        private void ClearDynamicMeshes()
        {
            // point cloud
            List<Mesh> meshesToRemove = _pointCloud.GetMeshesToRemove();
            for (var i = 0; i < meshesToRemove.Count; i++)
            {
                RC.Remove(meshesToRemove[i]);
            }

            List<Mesh> meshesToRemove2 = _voxelSpace.GetMeshesToRemove();
            for (var i = 0; i < meshesToRemove2.Count; i++)
            {
                RC.Remove(meshesToRemove2[i]);
            }
        }

        #endregion

        #region Various Event Handler

        /// <summary>
        /// Event handler for when a new node gets added to the octree.
        /// </summary>
        /// <param name="node">The node that was added.</param>
        private void OnNewNodeAdded(OctreeNode<OctreeNodeStates> node)
        {
            if (node.Data == OctreeNodeStates.Occupied && node.SideLength == VOXEL_SIZE)
            {
                // add voxel position as new offset attribute
                //_positionsVSP.Add(node.Position);

                // create single cube with VoxelSideLength at specified position

                Cube cube = new Cube();

                for (var i = 0; i < cube.Vertices.Length; i++)
                {
                    float3 vertex = cube.Vertices[i];

                    // scale cube (vertices) according to voxel side length
                    vertex *= VOXEL_SIZE; // no dividing by 2 neccessary, because vertex is already 0.5

                    // position vertex according to voxel center position
                    vertex += node.Position;

                    // apply to mesh
                    cube.Vertices[i] = vertex;
                }

                // --- add cube mesh to final mesh
                _voxelSpace.AddMesh(cube);
                _voxelSpace.Apply();
            }
        }

        /// <summary>
        /// Whenever a new point gets loaded, this is what happens with him.
        /// </summary>
        /// <param name="point">Point data structure containing position and stuff.</param>
        private void OnNewPointAdded(PointCloud.Point point)
        {
            _pointCounter++;

            if (_pointCounter % COMPUTE_EVERY != 0 && COMPUTE_EVERY != 1)
                return;
            
            _boundingBox.Update(point.Position);

            // add point to offset as attribute
            //_positionsPCL.Add(point.Position);

            // add point to point cloud

            PointMesh pointMesh = new PointMesh(point.Position);

            if (point.Color != null)
            {
                uint color = (uint)new ColorUint((float3)point.Color, 1);
                pointMesh.Colors = new uint[] { color, color, color, color };
            }

            _pointCloud.AddMesh(pointMesh);

            //*
            if (_pointCounter % UPDATE_PCL_EVERY == 0)
                _pointCloud.Apply();
            //*/

            // add point to octree

            _octree.Add(point.Position, OctreeNodeStates.Occupied);
        }

        /// <summary>
        /// Event handler for when asset has finished loading.
        /// </summary>
        private void OnAssetLoaded()
        {
            // apply remaining vertices etc.
            _pointCloud.Bake();
            _voxelSpace.Bake();
        }
        
        // update cameraPivot, whenever bounding box of point cloud gets updated
        private void OnBoundingBoxUpdate(BoundingBox boundingBox)
        {
            _cameraPivot = boundingBox.GetCenterPoint();

            _yBounds.x = boundingBox.GetMinValues().y;
            _yBounds.y = boundingBox.GetMaxValues().y;
        }

        #endregion

        #region Public Members

        public void SetViewMode(ViewMode viewMode)
        {
            _viewMode = viewMode;

            // Initialize the shader(s)

            // read shaders from files

            var shader = _viewMode == ViewMode.PointCloud ? RC.CreateShader(vertsh_PCL, pixsh_PCL) : RC.CreateShader(vertsh_VSP, pixsh_VSP);
            RC.SetShader(shader);

            if (_viewMode == ViewMode.PointCloud)
            {
                _particleSizeParam = RC.GetShaderParam(shader, "particleSize");
                RC.SetShaderParam(_particleSizeParam, new float2(ParticleSize, ParticleSize));
            }
            else
            {
                _yBoundsParam = RC.GetShaderParam(shader, "yBounds");
                RC.SetShaderParam(_yBoundsParam, _yBounds);
            }
        }

        #endregion
    }
}