using Fusee.Engine.Core;
using Fusee.Math.Core;
using static Fusee.Engine.Core.Input;
using static Fusee.Engine.Core.Time;
using System.Threading;
using Android.OS;
using Fusee.Base.Common;
using Fusee.Engine.Common;
using Fusee.Tutorial.Core.Data;
using Fusee.Tutorial.Core.Data_Transmission;
using Java.IO;
using Debug = System.Diagnostics.Debug;


namespace Fusee.Tutorial.Core
{
    [FuseeApplication(Name = "Forschungsprojekt", Description = "HFU Wintersemester 16-17")]
    public class PointVisualizationBase : RenderCanvas
    {
        #region Enums

        public enum ViewMode
        {
            PointCloud, VoxelSpace
        }

        #endregion

        #region Fields

        #region UDP Connection

        private const int UDP_PORT = 8001;//50123;//8001;
        public int udpPort { get; set; }

        [InjectMe]
        public IUDPReceiver UDPReceiver;

        #endregion

        #region Helper

        // current view mode: either pointcloud or voxelspace
        public ViewMode CurrentViewMode { get; set; } = ViewMode.PointCloud;

        // data
        private PointCloud _pointCloud; // particle size gets changed via static methods
        private VoxelSpace _voxelSpace;
        private DronePath _dronePath;

        private BoundingBox _boundingBox;
                
        // due to multithreading
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
            // bounding box
            _boundingBox = new BoundingBox();
            _boundingBox.UpdateCallbacks += OnBoundingBoxUpdate;

            // read shaders from files

            _pointCloud = new PointCloud(RC, _boundingBox);
            _voxelSpace = new VoxelSpace(RC, _boundingBox);
            _dronePath = new DronePath(RC);

            //Zoom Value
            _zoom = 60;

            // stream point cloud from text file

            ///*
            AssetReader.OnNewPointCallbacks += OnNewPointAdded;
            AssetReader.ReadFromAsset("PointCloud_IPM.txt");
            //*/

            // stream point cloud via udp

            
           //UDPReceiver.OnNewPointCallbacks += OnNewPointAdded;
           //UDPReceiver.OnDronePositionCallbacks += OnDronePositionAdded;
           //UDPReceiver.StreamFrom(UDP_PORT);
            

            // Set the clear color for the backbuffer
            RC.ClearColor = new float4(0.95f, 0.95f, 0.95f, 1);
        }

        #region Rendering
        
        /// <summary>
        /// RenderAFrame is called once a frame.
        /// </summary>
        public override void RenderAFrame()
        {
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);
            
            // check on key down

            if(Keyboard.IsKeyDown(KeyCodes.T))
            {
                SwitchViewMode();
            }

            // check on particle size change
            
            if (CurrentViewMode == ViewMode.PointCloud && (Keyboard.ADAxis != 0 || Keyboard.WSAxis != 0) )
            {
                PointCloud.IncreaseParticleSize(Keyboard.ADAxis * PointCloud.ParticleSizeInterval / 20);
            }
            
            // Render

            RC.ModelView = MoveInScene();
            
            _signalEvent.WaitOne(); // stop other thread from adding points until these points have been written to the gpu memory
            
            if (CurrentViewMode == ViewMode.PointCloud)
            {
                _pointCloud.Render();
            }
            else
            {
                _voxelSpace.Render();
            }
            
            _dronePath.Render();

            _signalEvent.Set(); // allow other thread again to add points
            
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
                _zoomVel = Mouse.WheelVel * -0.0005f;
                _angleRoll *= curDamp * 0.008f;
                _offset *= curDamp * 0.008f;
            }

            // UpDown / LeftRight rotation
            if (Mouse.LeftButton)
            {
                _keys = false;
                _angleVelHorz = -RotationSpeed* Mouse.XVel * -0.00002f;
                _angleVelVert = -RotationSpeed * Mouse.YVel * 0.00002f;
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
                    _angleVelHorz = -RotationSpeed * Keyboard.LeftRightAxis * 0.00002f;
                    _angleVelVert = -RotationSpeed * Keyboard.UpDownAxis * 0.00002f;
                }
                else
                {
                    _angleVelHorz *= curDamp;
                    _angleVelVert *= curDamp;
                }
            }

            _zoom += _zoomVel;

            // Limit zoom
            if (_zoom < 20)
                _zoom = 20;
            if (_zoom > 1000)
                _zoom = 1000;


            PointCloud.SetZoomValue(_zoom);
            //Debug.WriteLine("CurrentZoomValue" + _zoom);

            if (Mouse.RightButton || Input.Touch.TwoPoint)
            {
                float2 speed = Mouse.Velocity + Input.Touch.TwoPointMidPointVel;
                MoveX += speed.x * -0.0005f;
                MoveY += speed.y * -0.0005f;
            }

            // Debug.WriteLine("Min z" +_boundingBox.GetMinValues().z);
            //Debug.WriteLine("Max z" + _boundingBox.GetMaxValues().z);
            // SetZNearFarPlane
          // float2 _zBounds = new float2(0,0);
          // _zBounds.x = _boundingBox.GetMinValues().z;
          // _zBounds.y = _boundingBox.GetMaxValues().z;
          // _zBounds = new float2(_zBounds.x, _zBounds.y);
          // Debug.WriteLine("zBounds" + _zBounds);

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

            RC.Projection = float4x4.CreatePerspectiveFieldOfView(3.141592f * 0.25f, aspectRatio, 1, 20000);

            // particle size set to be square
            _pointCloud.SetAspectRatio(aspectRatio);
        }

        #endregion
        
        #region Event Handlers
        
        /// <summary>
        /// Whenever a new point gets loaded, this is what happens with him.
        /// </summary>
        /// <param name="point">Point data structure containing position and stuff.</param>
        private void OnNewPointAdded(Common.Point point)
        {   
            _signalEvent.WaitOne();

            _pointCloud.AddPoint(point);
            _voxelSpace.AddPoint(point);

            _signalEvent.Set();

            _boundingBox.Update(point.Position);
        }

        /// <summary>
        /// Update cameraPivot, whenever bounding box of point cloud gets updated:
        /// </summary>
        private void OnBoundingBoxUpdate(BoundingBox boundingBox)
        {
            _cameraPivot = boundingBox.GetCenterPoint();
           
        }

        /// <summary>
        /// Gets called when a new position for the drone is transmitted.
        /// </summary>
        /// <param name="position"></param>
        private void OnDronePositionAdded(float3 position)
        {
            _dronePath.AddPosition(position);
        }

        #endregion

        #region Public Members
        
        /// <summary>
        /// Switches the view mode from point cloud to voxelspace and vice versa.
        /// </summary>
        public void SwitchViewMode()
        {
            ViewMode nextViewMode = CurrentViewMode == ViewMode.PointCloud ? ViewMode.VoxelSpace : ViewMode.PointCloud;
            CurrentViewMode = nextViewMode;
        }

        #endregion
    }
}