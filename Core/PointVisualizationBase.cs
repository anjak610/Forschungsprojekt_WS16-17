using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using static Fusee.Engine.Core.Input;
using static Fusee.Engine.Core.Time;
using Fusee.Tutorial.Core.Common;
using Fusee.Base.Common;
using System.Collections.Generic;
using Android.Text.Method;
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

        //public PointVisualizationBase.ViewMode ModeProperty { get; set; }

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

        public ViewMode _ViewMode    // the Name property
        {
            get
            {
                return _viewMode;
            }
            set
            {
                _viewMode = value;
            }
        }

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

        private float4x4 _projection;

        // --- beginn camera values
        private static float _zoom, _zoomVel, _angleHorz = M.PiOver6 * 2.0f, _angleVert = -M.PiOver6 * 0.5f, _angleVelHorz, _angleVelVert, _angleRoll, _angleRollInit;
        private bool _twoTouchRepeated;
        private float _maxPinchSpeed;

        private float MoveX;
        private float MoveY;

        private float2 _offset;
        private static float2 _offsetInit;
        private const float Damping = 2f;
        private const float RotationSpeed = 4;

        private bool _scaleKey;
        private bool _keys;

        #endregion

        /// <summary>
        /// Initializes variables (very good summary).
        /// </summary>
        public override void Init()
        {
            // --- 1. Start loading resources

            // octree

            //Zoom Value
            _zoom = 60;
            
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
                        
            //PointCloudReader.ReadFromAsset("PointCloud_IPM.txt");
            PointCloudReader.ReceiveFromUDP(UDP_PORT); // for unity game or other

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

            if (_viewMode == ViewMode.PointCloud)
            {
                ChangeParticleSize(RC.CreateShader(vertsh_PCL, pixsh_PCL));
            }

            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            // Clear GPU memory
            ClearDynamicMeshes();

            //Change Particle Size
           // ChangeParticleSize();

            // Render
            RC.ModelView = MoveInScene();
           
            List<Mesh> meshes = _viewMode == ViewMode.PointCloud ? _pointCloud.GetMeshes() : _voxelSpace.GetMeshes();
            for(var i=0; i<meshes.Count; i++)
            {
                RC.Render(meshes[i]);
            }

            RC.Projection = _projection;
			
            // Swap buffers: Show the contents of the backbuffer (containing the currently rendered frame) on the front buffer.
            Present();
        }
        
        private float4x4 MoveInScene()
        {
          var curDamp = (float)System.Math.Exp(-Damping * DeltaTime);

            if (Keyboard.LeftRightAxis != 0 || Keyboard.UpDownAxis != 0)
            {
                _keys = true;
            }

            // Zoom & Roll
            if (Input.Touch.TwoPoint)
            {
                if (!_twoTouchRepeated)
                {
                    _twoTouchRepeated = true;
                    _angleRollInit = Input.Touch.TwoPointAngle - _angleRoll;
                    _maxPinchSpeed = 0;
                }
                _zoomVel = Input.Touch.TwoPointDistanceVel * -0.01f;
                _angleRoll = Input.Touch.TwoPointAngle - _angleRollInit;
                float pinchSpeed = Input.Touch.TwoPointDistanceVel;
                if (pinchSpeed > _maxPinchSpeed) _maxPinchSpeed = pinchSpeed; // _maxPinchSpeed is used for debugging only.
            }
            else
            {
                _twoTouchRepeated = false;
                _zoomVel = Mouse.WheelVel * -0.05f;
                _angleRoll *= curDamp * 0.8f;
                _offset *= curDamp * 0.8f;
            }

            // UpDown / LeftRight rotation
            if (Mouse.LeftButton)
            {
                _keys = false;
                _angleVelHorz = Mouse.XVel * 0.0002f;
                _angleVelVert = Mouse.YVel * 0.0002f;
            }
            else if (Input.Touch.GetTouchActive(TouchPoints.Touchpoint_0) && !Input.Touch.TwoPoint)
            {
                _keys = false;
                float2 touchVel;
                touchVel = Input.Touch.GetVelocity(TouchPoints.Touchpoint_0);
                _angleVelHorz = -RotationSpeed * touchVel.x * 0.00002f;
                _angleVelVert = -RotationSpeed * touchVel.y * -0.00002f;
            }
            else
            {
                if (_keys)
                {
                    _angleVelHorz = -RotationSpeed * Keyboard.LeftRightAxis * 0.002f;
                    _angleVelVert = -RotationSpeed * Keyboard.UpDownAxis * 0.002f;
                }
                else
                {
                    _angleVelHorz *= curDamp;
                    _angleVelVert *= curDamp;
                }
            }

            _zoom += _zoomVel;

            if (Mouse.RightButton || Input.Touch.TwoPoint)
            {
                float2 speed = Mouse.Velocity + Input.Touch.TwoPointMidPointVel;
                MoveX += speed.x * -0.0005f;
                MoveY += speed.y * -0.0005f;
            }

            _angleHorz += _angleVelHorz;
            // Wrap-around to keep _angleHorz between -PI and + PI
            _angleHorz = M.MinAngle(_angleHorz);

            _angleVert += _angleVelVert;
            // Limit pitch to the range between [-PI/2, + PI/2]
            _angleVert = M.Clamp(_angleVert, -M.PiOver2, M.PiOver2);

            // Wrap-around to keep _angleRoll between -PI and + PI
            _angleRoll = M.MinAngle(_angleRoll);

            // List<float3> positions = _pointCloud.GetPositions();

           // StartNumber StrPoint = new StartNumber(StrP);
            //float3 _startPoint = StrPoint(new float3(0, 0, 0));//positions[1]; //TODO: solution for Android!! Points aren't read at that Time
            var view = float4x4.CreateTranslation(-1 * _cameraPivot);

            var MtxCam = float4x4.LookAt(0, 0, _zoom, 0, 0, 0, 0, 1, 0) * float4x4.CreateTranslation(MoveX, MoveY, 0);
            var MtxRot = float4x4.CreateRotationZ(_angleRoll) * float4x4.CreateRotationX(_angleVert) * float4x4.CreateRotationY(_angleHorz);
            float4x4 ModelView = MtxCam * MtxRot * view;

            return ModelView;

        }

        public void ChangeParticleSize(ShaderProgram shader)
        {
            if (Keyboard.ADAxis != 0 || Keyboard.WSAxis != 0)
            {
                _scaleKey = true;
            }
            else
            {
                _scaleKey = false;
            }

            if (_scaleKey)
            {
                ParticleSize = ParticleSize + Keyboard.ADAxis * ParticleSize / 20;
            }

            _particleSizeParam = RC.GetShaderParam(shader, "particleSize");
            RC.SetShaderParam(_particleSizeParam, new float2(ParticleSize, ParticleSize));
            // return ParticleSize;
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

            _projection = float4x4.CreatePerspectiveFieldOfView(3.141592f * 0.25f, aspectRatio, 1, 20000);
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