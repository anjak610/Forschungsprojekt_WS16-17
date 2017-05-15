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
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Fusee.Tutorial.Core
{
    [FuseeApplication(Name = "Forschungsprojekt", Description = "HFU Wintersemester 16-17")]
    public class PointVisualizationBase : RenderCanvas
    {
        #region Fields
        
        #region UDP Connection

        private const int UDP_PORT = 8001;

        [InjectMe]
        public IUDPReceiver UDPReceiver;

        #endregion

        #region Rendering

        public ViewController ViewCtrl;
        private bool _snapshotActive = false;

        // render entities
        private PointCloud _pointCloud;     // particle size gets changed via static methods
        private VoxelSpace _voxelSpace;
        private DronePath _dronePath;
        private Wireframe _wireframe;       // Renders the bounding boxes for each node

        // data structures
        private BoundingBox _boundingBox;

        // due to multithreading
        private AutoResetEvent _signalEvent = new AutoResetEvent(true);

        #endregion

        #region Octree

        private Octree.Octree _octree;

        private List<OctreeNode> _nodesToBeLoaded = new List<OctreeNode>();
        private List<OctreeNode> _nodesToBeRemoved = new List<OctreeNode>();

        private bool _traversingFinished = true;
        private bool _scheduleLoading = false;

        // conditions of when to stop the traversal

        private const int POINT_BUDGET = 10000; // number of points that are visible at one frame, tradeoff between performance and quality
        private const float MIN_SCREEN_PROJECTED_SIZE = 1; // minimum screen size of the node
        private int NODES_TO_BE_LOADED_PER_SCHEDULE = 1000; // X = 5, see Schuetz (2016)

        private int _visitPointCounter; // counts the points that have been scheduled to be loaded while traversing the octree

        private SortedDictionary<double, OctreeNode> _nodesOrdered = new SortedDictionary<double, OctreeNode>(); // nodes ordered by screen-projected-size

        private float _screenHeight = 1; // needs to be set for calculating screen-projected-size for each octree node

        private double _fov = 3.141592f * 0.25f; // needs to be the same as in PointVisualizationBase.cs
        private double _slope; //_slope = System.Math.Tan(_fov / 2);
        //private float3 _cameraPosition;

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

            ViewCtrl = new ViewController(_octree, Keyboard);

            _pointCloud = new PointCloud(RC, _boundingBox);
            _voxelSpace = new VoxelSpace(RC, _boundingBox);
            _dronePath = new DronePath(RC);
            _wireframe = new Wireframe(RC);

            _octree.Init(256); // 256 => initialSideLength; after render entities have been initialized due to callbacks

            //Zoom Value
            _zoom = 60;

            // stream point cloud from text file
            
            //*
            //AssetReader.OnAssetLoadedCallbacks += () => {};
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
            
            RC.ModelView = MoveInScene();

            ViewCtrl.CheckOnKeyboardEvents();
            
            if (ViewCtrl.GetCurrentViewMode() == ViewMode.PointCloud)
            {
                if (Keyboard.IsKeyDown(KeyCodes.N))
                {
                    StartCollectingNodes();
                }

                // load octree node data when traversal has finished
                if (_scheduleLoading)
                {
                    LoadNodes();
                }

                // check on particle size change
                if (Keyboard.ADAxis != 0 || Keyboard.WSAxis != 0)
                {
                    PointCloud.IncreaseParticleSize(Keyboard.ADAxis * PointCloud.ParticleSizeInterval / 20);
                }

                switch(ViewCtrl.GetCurrentDebugViewMode())
                {
                    case DebugViewMode.Standard:

                        if(Keyboard.IsKeyDown(KeyCodes.S) && !_snapshotActive)
                        {
                            _pointCloud.TakeSnapshot();
                            _wireframe.TakeSnapshot();

                            _snapshotActive = true;
                        } 
                        else if(Keyboard.IsKeyDown(KeyCodes.S) && _snapshotActive)
                        {
                            _pointCloud.ReleaseSnapshot();
                            _wireframe.ReleaseSnapshot();

                            _snapshotActive = false;
                        }

                        if(_snapshotActive)
                        {
                            _pointCloud.RenderSnapshot();

                            if (ViewCtrl.IsWireframeVisible())
                                _wireframe.RenderSnapshot();
                        }
                        else
                        {
                            _pointCloud.Render();
                            if (ViewCtrl.IsWireframeVisible())
                                _wireframe.Render();
                        }

                        break;
                    case DebugViewMode.PerLevel:

                        _pointCloud.Render(ViewCtrl.GetCurrentLevel());
                        if (ViewCtrl.IsWireframeVisible())
                            _wireframe.Render(ViewCtrl.GetCurrentLevel());

                        break;
                    case DebugViewMode.PerNode:

                        OctreeNode debugNode = ViewCtrl.GetDebugNode();

                        if (ViewCtrl.HasDebugNodeChanged())
                        {
                            _wireframe.SetNewDebuggingNode(debugNode);
                            _pointCloud.SetNewDebuggingNode(debugNode);
                        }

                        _pointCloud.Render(debugNode.GetLevel(), debugNode);
                        if (ViewCtrl.IsWireframeVisible())
                            _wireframe.Render(debugNode.GetLevel(), debugNode);

                        break;
                }
            }
            else
            {
                _signalEvent.WaitOne();
                _voxelSpace.Render();
                _signalEvent.Set();
            }
                  
            _dronePath.Render();
            
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
            _voxelSpace.AddPoint(point);
            _signalEvent.Set();

            _boundingBox.Update(point.Position);
            _octree.Add(point.Position);
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

        #region Octree Traversal

        /// <summary>
        /// When traversal has finished, this method ensures that all the nodes are transferred into memory.
        /// Gets called in the render cycle.
        /// </summary>
        private void LoadNodes()
        {
            // load data

            foreach (OctreeNode node in _nodesToBeLoaded)
            {
                _wireframe.AddNode(node);
                _pointCloud.AddNode(node);

                node.RenderFlag = OctreeNodeStates.Visible;
            }

            _nodesToBeLoaded.Clear();

            // remove data

            foreach (OctreeNode node in _nodesToBeRemoved)
            {
                _wireframe.RemoveNode(node);
                _pointCloud.RemoveNode(node);

                node.RenderFlag = OctreeNodeStates.NonVisible;
            }

            _nodesToBeRemoved.Clear();

            _scheduleLoading = false;
        }

        /// <summary>
        /// Starts a new octree traversal in order to search for nodes to be rendered.
        /// </summary>
        private void StartCollectingNodes()
        {
            if (!_traversingFinished)
                return;

            _traversingFinished = false;

            Task task = new Task(() =>
            {
                _octree.Traverse(OnNodeVisited);

                _traversingFinished = true;
                _scheduleLoading = true;
            });

            task.Start();
        }

        /// <summary>
        /// Gets called each time a node is visited by the octree traversal in StartCollectingNodes().
        /// </summary>
        /// <param name="node">The node currently visited.</param>
        private void OnNodeVisited(OctreeNode node)
        {
            if(node.RenderFlag == OctreeNodeStates.NonVisible)
            {
                _nodesToBeLoaded.Add(node);
                node.RenderFlag = OctreeNodeStates.VisibleButUnloaded;
            }   
        }

        /*
        /// <summary>
        /// Traverses the octree and searches for
        /// </summary>
        /// <param name="rootNode"></param>
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
        */

            /*
        /// <summary>
        /// Gets called when the bucket of a node has changed. (Means more points have been added or removed from this node.)
        /// </summary>
        /// <param name="node">The node which bucket has changed.</param>
        private void OnNodeBucketChanged(OctreeNode node)
        {
            int level = node.GetLevel();

            if (level > MAX_NODE_LEVEL || level != _levelToLoad)
                return;

            StartNewTraversingForVisibleNodes = true;
        }
        */

        #endregion
    }
}