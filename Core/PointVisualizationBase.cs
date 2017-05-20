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
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;

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

        // render entities
        private PointCloud _pointCloud;     // particle size gets changed via static methods
        private VoxelSpace _voxelSpace;
        private DronePath _dronePath;
        private Wireframe _wireframe;       // Renders the bounding boxes for each node

        // data structures
        private BoundingBox _boundingBox;

        #endregion

        #region Octree

        private int NODES_TO_BE_LOADED_PER_SCHEDULE = 100; // this should happen quickly
        private int NODES_TO_BE_REMOVED_PER_SCHEDULE = 100; // no need for hurry

        private OctreeRenderer _octreeRenderer;
        private Octree.Octree _octree;
        
        private List<OctreeNode> _nodesCurrentlyRendered = new List<OctreeNode>();

        private ConcurrentQueue<OctreeNode> _nodesToBeLoaded = new ConcurrentQueue<OctreeNode>();
        private ConcurrentQueue<OctreeNode> _nodesToBeRemoved = new ConcurrentQueue<OctreeNode>();
        
        private bool _collectingFinished = true;
        private bool _cancelTraversal = false;
        
        private Frustum _currentViewFrustum; // the view frustum to check nodes again

        // conditions of when to start and stop the traversal
        
        private float3 _cameraPosition = float3.Zero;
        
        #endregion

        #region Camera Positioning

        private float _rotationY = (float) System.Math.PI;
        private float _rotationX = (float) System.Math.PI / -8;

        //private float3 _cameraPosition = new float3(0, 0, -5.0f);
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

            Octree.Octree.OnNodeBucketChangedCallback += OnNodeBucketChanged;

            _octree = new Octree.Octree(float3.Zero, 32);
            //_octree = new Octree.Octree(float3.One, 4);

            _octreeRenderer = new OctreeRenderer(this, _octree);
            
            _octreeRenderer.OnTraversalFinishedCallbacks += OnOctreeRendererTraversalFinished;
            _octreeRenderer.OnTraversalVisibleNodeFoundCallbacks += OnOctreeRendererTraversalVisibleNodeFound;

            // initialize render entities

            ViewCtrl = new ViewController(_octree, Keyboard);

            _pointCloud = new PointCloud(RC, _boundingBox);
            _voxelSpace = new VoxelSpace(RC, _boundingBox);
            _dronePath = new DronePath(RC);
            _wireframe = new Wireframe(RC);

            _octree.Init(256); // 256 => initialSideLength; after render entities have been initialized due to callbacks
            //_octree.Init(16);

            //Zoom Value
            _zoom = 60;

            // stream point cloud from text file
            
            //*
            //AssetReader.OnAssetLoadedCallbacks += () => {};
            AssetReader.OnNewPointCallbacks += OnNewPointAdded;
            AssetReader.ReadFromAsset("PointCloud_IPM.txt");
            //AssetReader.ReadFromAsset("TestPoints.txt");
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
                _octreeRenderer.OnRenderCycle();
                
                LoadNodes();
                RemoveNodes();
                
                // check on particle size change
                if (Keyboard.ADAxis != 0 || Keyboard.WSAxis != 0)
                {
                    PointCloud.IncreaseParticleSize(Keyboard.ADAxis * PointCloud.ParticleSizeInterval / 20);
                }

                switch(ViewCtrl.GetCurrentDebugViewMode())
                {
                    case DebugViewMode.Standard:

                        if(ViewCtrl.HasTakenSnapshot())
                        {
                            _pointCloud.TakeSnapshot();
                            _wireframe.TakeSnapshot();
                        } 
                        else if(ViewCtrl.HasReleasedSnapshot())
                        {
                            _pointCloud.ReleaseSnapshot();
                            _wireframe.ReleaseSnapshot();
                        }

                        if(ViewCtrl.IsSnapshotActive())
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
                _voxelSpace.Render();
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

            float3 cameraPos = float4x4.Invert(ModelView) * float3.Zero;

            if(cameraPos != _cameraPosition)
            {
                _cameraPosition = cameraPos;
                _octreeRenderer.ScheduleNewTraversal(RC.ModelViewProjection, _cameraPosition);
            }

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
        
        /// <summary>
        /// Gets called when the bucket of a node has changed. (Means more points have been added or removed from this node.)
        /// </summary>
        /// <param name="node">The node which bucket has changed.</param>
        private void OnNodeBucketChanged(OctreeNode node)
        {
            _octreeRenderer.ScheduleNewTraversal(RC.ModelViewProjection, _cameraPosition);
        }

        #endregion

        #region Octree Rendering

        /// <summary>
        /// Each time on traversal, when the octree renderer has found a node that is visible and should be rendered,
        /// it gets a callback here.
        /// </summary>
        /// <param name="node">The node that should be rendered by the current view.</param>
        private void OnOctreeRendererTraversalVisibleNodeFound(OctreeNode node)
        {
            switch(node.RenderFlag)
            {
                case RenderFlag.Visible:
                    break;
                case RenderFlag.NonVisible:
                    node.RenderFlag = RenderFlag.VisibleButUnloaded;
                    _nodesToBeLoaded.Enqueue(node);
                    break;
                case RenderFlag.VisibleButUnloaded:
                    break;
                case RenderFlag.NonVisibleButLoaded:
                    node.RenderFlag = RenderFlag.Visible;
                    break;
            }
        }

        /// <summary>
        /// Gets called every time the octree renderer has finished a traversal.
        /// </summary>
        /// <param name="visibleNodes">A list of nodes which are now visible and should be rendered.</param>
        private void OnOctreeRendererTraversalFinished(List<OctreeNode> visibleNodes)
        {
            // remove all nodes from _nodesCurrentlyRendered that aren't in _currentVisibleNodes

            OctreeNode node;

            for(var i=0; i<_nodesCurrentlyRendered.Count; i++)
            {
                node = _nodesCurrentlyRendered[i];

                if (!visibleNodes.Contains(node)) // node needs to be removed
                {
                    switch (node.RenderFlag)
                    {
                        case RenderFlag.Visible:
                            node.RenderFlag = RenderFlag.NonVisibleButLoaded;
                            _nodesToBeRemoved.Enqueue(node);
                            break;
                        case RenderFlag.NonVisible:
                            break;
                        case RenderFlag.VisibleButUnloaded:
                            node.RenderFlag = RenderFlag.NonVisible;
                            break;
                        case RenderFlag.NonVisibleButLoaded:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// When traversal has finished, this method ensures that all the nodes are transferred into memory.
        /// Gets called in the render cycle.
        /// </summary>
        private void LoadNodes()
        {
            OctreeNode node;

            for (var i = 0; i < NODES_TO_BE_LOADED_PER_SCHEDULE; i++)
            {
                if (i > _nodesToBeLoaded.Count - 1)
                    break;
                
                _nodesToBeLoaded.TryDequeue(out node);

                if(node.RenderFlag != RenderFlag.NonVisible)
                {
                    _wireframe.AddNode(node);
                    _pointCloud.AddNode(node);

                    if (!_nodesCurrentlyRendered.Contains(node))
                    {
                        _nodesCurrentlyRendered.Add(node);
                    }

                    node.RenderFlag = RenderFlag.Visible;
                }
            }
        }

        /// <summary>
        /// Deallocates the memory taken by the nodes in the current nodesToBeRemoved-List.
        /// </summary>
        private void RemoveNodes()
        {
            OctreeNode node;

            for (var i = 0; i < NODES_TO_BE_REMOVED_PER_SCHEDULE; i++)
            {
                if (i > _nodesToBeRemoved.Count - 1)
                    break;
                
                _nodesToBeRemoved.TryDequeue(out node);

                if(node.RenderFlag != RenderFlag.Visible)
                {
                    _wireframe.RemoveNode(node);
                    _pointCloud.RemoveNode(node);

                    _nodesCurrentlyRendered.Remove(node);

                    node.RenderFlag = RenderFlag.NonVisible;
                }
            }
        }

        #endregion
    }
}