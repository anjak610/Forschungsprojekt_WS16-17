using Fusee.Engine.Core;
using Fusee.Math.Core;
using static Fusee.Engine.Core.Input;
using static Fusee.Engine.Core.Time;
using System.Threading;
using Fusee.Tutorial.Core.Data_Transmission;
using Fusee.Base.Common;
using Fusee.Tutorial.Core.Data;
using Fusee.Engine.Common;
using Fusee.Tutorial.Core.Octree;
using System.Linq;

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
        
        public enum ViewModeDebugging // works only with pointcloud or wireframe
        {
            All,        // Standard behaviour, see all bounding boxes and points together
            PerLevel,   // watch points / nodes per level in octree
            PerNode,    // watch single nodes (points/boundingboxes highlighted in orange) together with nodes on the same level
            Snapshot    // Hit S in order to take a snapshot while viewing ViewModeDebugging.All
        }

        #endregion

        #region Fields

        #region Constants / Settings

        private const int UDP_PORT = 8001;

        private const int MAX_LEVEL_DEBUG = 8; // how deep in the octree can we debug?
        
        #endregion

        #region UDP Connection

        [InjectMe]
        public IUDPReceiver UDPReceiver;

        #endregion

        #region Rendering

        // current view mode: either pointcloud or voxelspace
        public ViewMode CurrentViewMode { get; set; } = ViewMode.PointCloud;

        // render entities
        private PointCloud _pointCloud;     // particle size gets changed via static methods
        private VoxelSpace _voxelSpace;
        private DronePath _dronePath;
        private Wireframe _wireFrame;       // Renders the bounding boxes for each node

        // data structures
        private BoundingBox _boundingBox;
        private Octree.Octree _octree;

        // due to multithreading
        private AutoResetEvent _signalEvent = new AutoResetEvent(true);

        #endregion

        #region Debugging

        public ViewModeDebugging CurrentViewModeDebugging { get; set; } = ViewModeDebugging.All;
        private bool _wireFrameVisible = true;

        private OctreeNode _debuggingNode;

        // 0 => root node, 1 => second largest node, etc.
        private int _level = 0;
        private bool _showAllLevels = true;

        #endregion

        #region Camera Positioning

        private float _rotationY = (float) System.Math.PI;
        private float _rotationX = (float) System.Math.PI / -8;

        private float3 _cameraPosition = new float3(0, 0, -5.0f);
        private float3 _cameraPivot = new float3(0, 0, 0);
        private float3 _cameraPivotOffset = new float3(0, 0, 0);

        private float _minAngleX = (float) -System.Math.PI / 4;
        private float _maxAngleX = (float) System.Math.PI / 4;
        
        // Begin Camera Values
        
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
            // data structures

            _boundingBox = new BoundingBox();
            _boundingBox.UpdateCallbacks += OnBoundingBoxUpdate;

            _octree = new Octree.Octree(float3.Zero, 16);

            // initialize render entities

            _pointCloud = new PointCloud(RC, _octree, _boundingBox);
            _voxelSpace = new VoxelSpace(RC, _boundingBox);
            _dronePath = new DronePath(RC);
            _wireFrame = new Wireframe(RC, _octree);

            _octree.Init(256); // 256 => initialSideLength; after render entities have been initialized due to callbacks

            //Zoom Value
            _zoom = 60;

            // stream point cloud from text file

            /*
            AssetReader.OnAssetLoadedCallbacks += () =>
            {
                //_pointCloud.HasAssetLoaded = true;
                Task task = new Task(() => 
                {
                    _octree.Traverse((OctreeNode node) => 
                    {
                        _signalEvent.WaitOne();
                        _pointCloud.OnNewOctreeNodeAdded(node);
                        _signalEvent.Set();
                    });
                });

                task.Start();
            };
            //*/

            //*
            AssetReader.OnNewPointCallbacks += OnNewPointAdded;
            AssetReader.ReadFromAsset("PointCloud_IPM.txt");
            //*/

            // stream point cloud via udp

            /*
            UDPReceiver.OnNewPointCallbacks += OnNewPointAdded;
            UDPReceiver.OnDronePositionCallbacks += OnDronePositionAdded;
            UDPReceiver.StreamFrom(UDP_PORT);
            //*/

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

            #region Keyboard Events

            // check on key down

            if(Keyboard.IsKeyDown(KeyCodes.T))
            {
                SwitchViewMode();
            }

            if (Keyboard.IsKeyDown(KeyCodes.K))
            {
                SwitchWireframe();
            }

            if (Keyboard.IsKeyDown(KeyCodes.L))
            {
                SwitchViewModeDebugging();
            }

            // Just for debugging purposes

            if(Keyboard.IsKeyDown(KeyCodes.Up))
            {
                LevelUp();
            }

            if (Keyboard.IsKeyDown(KeyCodes.Down))
            {
                LevelDown();
            }

            if (Keyboard.IsKeyDown(KeyCodes.Left))
            {
                DebugPreviousSibling();
            }

            if (Keyboard.IsKeyDown(KeyCodes.Right))
            {
                DebugNextSibling();
            }
            
            // check on particle size change

            if (CurrentViewMode == ViewMode.PointCloud && (Keyboard.ADAxis != 0 || Keyboard.WSAxis != 0) )
            {
                PointCloud.IncreaseParticleSize(Keyboard.ADAxis * PointCloud.ParticleSizeInterval / 20);
            }

            #endregion

            // Render

            _signalEvent.WaitOne(); // stop other thread from adding points until these points have been written to the gpu memory

            RC.ModelView = MoveInScene();

            if (CurrentViewMode == ViewMode.PointCloud)
            {
                int level = _showAllLevels ? -1 : _level;

                _pointCloud.Render(CurrentViewModeDebugging, level, _debuggingNode);

                if(_wireFrameVisible)
                    _wireFrame.Render(CurrentViewModeDebugging, level, _debuggingNode);
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

            _pointCloud.SetZoomValue(_zoom);
            
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

            _pointCloud.SetCameraPosition(ModelView * float3.Zero);

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

            // inform point cloud about resizing
            _pointCloud.OnResize(this);
        }

        #endregion
        
        #region Event Handlers
        
        /// <summary>
        /// Whenever a new point gets loaded, this is what happens with him.
        /// Not to mixed up with the OnNodeAdded callback.
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

        #region Debugging

        /// <summary>
        /// Switches the view mode for debugging wireframe and point cloud.
        /// </summary>
        private void SwitchViewModeDebugging()
        {
            ViewModeDebugging nextViewMode;

            switch (CurrentViewModeDebugging)
            {
                case ViewModeDebugging.All:
                    nextViewMode = ViewModeDebugging.PerLevel;
                    _showAllLevels = false;
                    _pointCloud.StartNewTraversingForVisibleNodes = true;
                    break;
                case ViewModeDebugging.PerLevel:
                    nextViewMode = ViewModeDebugging.PerNode;

                    // start debugging via traversing octree
                    _debuggingNode = _octree.GetRootNode();

                    _wireFrame.OnNewDebuggingNode(_debuggingNode);
                    _pointCloud.OnNewDebuggingNode(_debuggingNode);

                    _level = 0;
                    _showAllLevels = false;
                    break;
                case ViewModeDebugging.PerNode:
                    nextViewMode = ViewModeDebugging.Snapshot;
                    _showAllLevels = true;
                    break;
                case ViewModeDebugging.Snapshot:
                default:
                    nextViewMode = ViewModeDebugging.All;
                    _showAllLevels = true;
                    break;
            }

            CurrentViewModeDebugging = nextViewMode;
        }

        /// <summary>
        /// Switches whether the wire frame is visible or not.
        /// </summary>
        private void SwitchWireframe()
        {
            _wireFrameVisible = !_wireFrameVisible;
        }

        /// <summary>
        /// Demands the wireframe and the point cloud only to render a specific level.
        /// Level up means bigger nodes.
        /// </summary>
        public void LevelUp()
        {
            if (_level == 0)
                return;

            bool hasLevelChanged = false;

            if (CurrentViewModeDebugging == ViewModeDebugging.PerNode)
            {
                if (_debuggingNode.Parent != null)
                {
                    _debuggingNode = _debuggingNode.Parent;

                    _wireFrame.OnNewDebuggingNode(_debuggingNode);
                    _pointCloud.OnNewDebuggingNode(_debuggingNode);

                    _level--;
                    hasLevelChanged = true;
                }
            }
            else if(CurrentViewModeDebugging == ViewModeDebugging.PerLevel)
            {
                _level--;
                hasLevelChanged = true;
            }

            if (hasLevelChanged)
                _pointCloud.StartNewTraversingForVisibleNodes = true;
        }

        /// <summary>
        /// Demands the wireframe and the point cloud only to render a specific level.
        /// </summary>
        public void LevelDown()
        {
            if (_level >= MAX_LEVEL_DEBUG)
                return;

            bool hasLevelChanged = false;
                
            if (CurrentViewModeDebugging == ViewModeDebugging.PerNode)
            {
                if (_debuggingNode.hasChildren())
                {
                    _debuggingNode = _debuggingNode.Children[0];

                    _wireFrame.OnNewDebuggingNode(_debuggingNode);
                    _pointCloud.OnNewDebuggingNode(_debuggingNode);

                    _level++;
                    hasLevelChanged = true;
                } 
            }
            else if (CurrentViewModeDebugging == ViewModeDebugging.PerLevel)
            {
                _level++;
                hasLevelChanged = true;
            }

            if (hasLevelChanged)
                _pointCloud.StartNewTraversingForVisibleNodes = true;
        }

        /// <summary>
        /// Sets the debugging node to its previous sibling if existent.
        /// </summary>
        public void DebugPreviousSibling()
        {
            if (CurrentViewModeDebugging != ViewModeDebugging.PerNode)
                return;

            byte index = _debuggingNode.Path.Last();

            if (index > 0)
            {
                _debuggingNode = _debuggingNode.Parent.Children[index - 1];

                _wireFrame.OnNewDebuggingNode(_debuggingNode);
                _pointCloud.OnNewDebuggingNode(_debuggingNode);
            }
        }

        /// <summary>
        /// Sets the debugging node to its previous sibling if existent.
        /// </summary>
        public void DebugNextSibling()
        {
            if (CurrentViewModeDebugging != ViewModeDebugging.PerNode)
                return;

            byte index = _debuggingNode.Path.Last();

            if (index < _debuggingNode.Parent.Children.Length - 1)
            {
                _debuggingNode = _debuggingNode.Parent.Children[index + 1];

                _wireFrame.OnNewDebuggingNode(_debuggingNode);
                _pointCloud.OnNewDebuggingNode(_debuggingNode);
            }
        }

        #endregion
    }
}