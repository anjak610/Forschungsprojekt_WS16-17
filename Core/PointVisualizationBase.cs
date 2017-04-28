using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using static Fusee.Engine.Core.Input;
using static Fusee.Engine.Core.Time;
using Fusee.Tutorial.Core.Common;
using System.Collections.Generic;
using Fusee.Tutorial.Core.Octree;
using Fusee.Tutorial.Core.PointCloud;
using System.Threading;

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

        #region Constants

        private const float VOXEL_SIZE = 1;
        
        private const int COMPUTE_EVERY = 1; // take only every xth point into account in order to speed up calculation
        private const int UPDATE_PCL_EVERY = 1000; // every xth point the point cloud should update its meshes
        
        public float ParticleSize = 0.05f; // maybe gets changed from platform specific classes

        #endregion

        #region Connection Details
        
        private const int UDP_PORT = 8001;

        #endregion

        #region Helper Variables

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

        #endregion

        #region Mesh Data / Pointcloud / Voxelspace
        
        // These are the objects where new vertices are stored. Also look at the description of the class(es) for more information.
        private DynamicAttributes _positionsVSP = new DynamicAttributes(); // no need for separation of buffers
        private AttributesList _positionsPCL = new AttributesList(65000); // <- limit of how many vertices one buffer should hold ( may be randomly )

        // when adding each point to the instance attributes, computation gets slow.
        // so temporarily store vertices in list, and add it when it reaches some length, e.g. 1000 => see UPDATE_PCL_EVERY
        private List<float3> _tmpPositionsPCL = new List<float3>(); 
        
        // multiple cubes will be rendered on different positions
        private Cube _cube = new Cube();

        private BoundingBox _boundingBox;
        private Octree<OctreeNodeStates> _octree;

        #endregion

        #region Shader Params

        private ShaderProgram _shader;

        private string vertsh_PCL, vertsh_VSP, pixsh_PCL, pixsh_VSP;
        
        private IShaderParam _yBoundsParam;
        private float2 _yBounds = float2.Zero;

        private IShaderParam _particleSizeParam;

        // TO-DO: set shader param for voxelsize?

        #endregion

        #region Multithreading

        private AutoResetEvent _signalEvent = new AutoResetEvent(true);

        #endregion

        #region Camera Positioning

        private float _rotationY = (float) System.Math.PI;
        private float _rotationX = (float) System.Math.PI / -8;

        private float3 _cameraPosition = new float3(0, 0, -5.0f);
        private float3 _cameraPivot = new float3(0, 0, 0);
        private float3 _cameraPivotOffset = new float3(0, 0, 0);

        private float _minAngleX = (float) -System.Math.PI / 4;
        private float _maxAngleX = (float) System.Math.PI / 4;

        private float4x4 _projection;

        #endregion

        #region Begin Camera Values
        
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

        #endregion

        /// <summary>
        /// Initializes variables.
        /// </summary>
        public override void Init()
        {
            // --- 1. Start loading resources

            //Zoom Value
            _zoom = 60;

            // octree

            _octree = new Octree<OctreeNodeStates>(float3.Zero, VOXEL_SIZE);
            _octree.OnNodeAddedCallback += OnNewNodeAdded;
            
            // bounding box
            
            _boundingBox = new BoundingBox();
            _boundingBox.UpdateCallbacks += OnBoundingBoxUpdate;
            
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
        
        /// <summary>
        /// RenderAFrame is called once a frame.
        /// </summary>
        public override void RenderAFrame()
        {
            // check on key down

            if(Keyboard.IsKeyDown(KeyCodes.T))
            {
                ViewMode nextViewMode = _viewMode == ViewMode.PointCloud ? ViewMode.VoxelSpace : ViewMode.PointCloud;
                SetViewMode(nextViewMode);
            }
            else
            {
                SetShaderParams();
            }

            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);
            
            // Render

            RC.ModelView = MoveInScene();

            _signalEvent.WaitOne(); // stop other thread from adding points until these points have been written to the gpu memory
            
            if (_viewMode == ViewMode.PointCloud)
            {
                List<DynamicAttributes> ias = _positionsPCL.GetAttributesList();
                for(var i=0; i<ias.Count; i++)
                {
                    RC.RenderAsPoints(ias[i]);
                }
            }
            else
            {
                RC.RenderAsInstance(_cube, _positionsVSP);
            }

            _signalEvent.Set(); // allow other thread again to add points

            RC.Projection = _projection;
			
            // Swap buffers: Show the contents of the backbuffer (containing the currently rendered frame) on the front buffer.
            Present();
        }
        
        /// <summary>
        /// Computes the modelview matrix.
        /// </summary>
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

        /// <summary>
        /// Is called when the window was resized.
        /// </summary>
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
        
        #region Various Event Handlers

        /// <summary>
        /// Event handler for when a new node gets added to the octree.
        /// </summary>
        /// <param name="node">The node that was added.</param>
        private void OnNewNodeAdded(OctreeNode<OctreeNodeStates> node)
        {
            if (node.Data == OctreeNodeStates.Occupied && node.SideLength == VOXEL_SIZE)
            {
                _signalEvent.WaitOne();
                _positionsVSP.AddAttribute(node.Position);
                _signalEvent.Set();
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

            _tmpPositionsPCL.Add(point.Position);

            if(_pointCounter % UPDATE_PCL_EVERY == 0)
            {
                _signalEvent.WaitOne();
                _positionsPCL.AddAttributes(_tmpPositionsPCL);
                _tmpPositionsPCL = new List<float3>();
                _signalEvent.Set();
            }

            // add point to octree

            _octree.Add(point.Position, OctreeNodeStates.Occupied);
        }

        /// <summary>
        /// Event handler for when asset has finished loading.
        /// </summary>
        private void OnAssetLoaded()
        {
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

        /// <summary>
        /// Changes the current view to either pointcloud or voxelspace.
        /// </summary>
        public void SetViewMode(ViewMode viewMode)
        {
            _viewMode = viewMode;

            // Initialize the shader(s)

            // read shaders from files

            _shader = _viewMode == ViewMode.PointCloud ? RC.CreateShader(vertsh_PCL, pixsh_PCL) : RC.CreateShader(vertsh_VSP, pixsh_VSP);
            RC.SetShader(_shader);

            SetShaderParams();
        }

        #endregion

        #region Private Members

        /// <summary>
        /// Sets the shader params. Gets Called either every frame or from SetViewMode() (via Android button).
        /// </summary>
        private void SetShaderParams()
        {
            if (_viewMode == ViewMode.PointCloud)
            {
                // no point size implementation at the moment
                /*
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

                _particleSizeParam = RC.GetShaderParam(_shader, "particleSize");
                RC.SetShaderParam(_particleSizeParam, new float2(ParticleSize, ParticleSize));
                //*/
            }
            else
            {
                _yBoundsParam = RC.GetShaderParam(_shader, "yBounds");
                RC.SetShaderParam(_yBoundsParam, _yBounds);
            }
        }

        #endregion
    }
}