using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using static Fusee.Engine.Core.Input;
using static Fusee.Engine.Core.Time;
using Fusee.Tutorial.Core.Common;
using Fusee.Base.Common;
using System.Collections.Generic;
using Fusee.Tutorial.Core.Octree;
using Fusee.Tutorial.Core.PointCloud;

namespace Fusee.Tutorial.Core
{

    [FuseeApplication(Name = "Forschungsprojekt", Description = "HFU Wintersemester 16-17")]
    public class PointVisualizationBase : RenderCanvas
    {
        private enum ViewMode
        {
            PointCloud, VoxelSpace
        }

        #region Fields

        // --- constants

        private const float VOXEL_SIZE = 2;
        private const int COMPUTE_EVERY = 1; // take only every xth point into account in order to speed up calculation
        private const ViewMode VIEW_MODE = ViewMode.VoxelSpace;
        
        public float ParticleSize = 0.05f; // maybe gets changed from platform specific classes

        // connection details to receive from

        private const int UDP_PORT = 8001;

        // helper variables

        private int _pointCounter = 0;
        
        // --- mesh data / pointcloud / voxelspace

        private DynamicMesh _pointCloud;
        private DynamicMesh _voxelSpace;
        private BoundingBox _boundingBox;

        Octree<OctreeNodeStates> _octree;
        
        // --- shader params
        
        private IShaderParam _yBoundsParam;
        private float2 _yBounds;

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
            // octree
            
            _octree = new Octree<OctreeNodeStates>(float3.Zero, VOXEL_SIZE);
            _octree.OnNodeAddedCallback += OnNewNodeAdded;

            _voxelSpace = new DynamicMesh();

            // point cloud + bounding box

            _pointCloud = new DynamicMesh();

            _boundingBox = new BoundingBox();
            _boundingBox.UpdateCallbacks += OnBoundingBoxUpdate;

            // callbacks for various readings and stuff

            PointCloudReader.OnNewPointCallbacks += OnNewPointAdded;
            PointCloudReader.OnAssetLoadedCallbacks += OnAssetLoaded;

            // start loading points
                        
            //PointCloudReader.ReadFromAsset("PointCloud_IPM.txt");
            PointCloudReader.ReceiveFromUDP(UDP_PORT); // for unity game or other

            // read shaders from files

            string pathVertexShader, pathPixelShader;

            if(VIEW_MODE == ViewMode.PointCloud)
            {
                pathVertexShader = "VertexShaderPCL.vert";
                pathPixelShader = "PixelShaderPCL.frag";
            }
            else
            {
                pathVertexShader = "VertexShaderVSP.vert";
                pathPixelShader = "PixelShaderVSP.frag";
            }

            var vertsh = AssetStorage.Get<string>(pathVertexShader);
            var pixsh = AssetStorage.Get<string>(pathPixelShader);
            
            // Initialize the shader(s)

            var shader = RC.CreateShader(vertsh, pixsh);
            RC.SetShader(shader);

            if (VIEW_MODE == ViewMode.PointCloud)
            {
                _particleSizeParam = RC.GetShaderParam(shader, "particleSize");
                RC.SetShaderParam(_particleSizeParam, new float2(ParticleSize, ParticleSize));

            }
            else
            {
                _yBoundsParam = RC.GetShaderParam(shader, "yBounds");
                RC.SetShaderParam(_yBoundsParam, float2.Zero);
            }

            // Set the clear color for the backbuffer
            RC.ClearColor = new float4(0.95f, 0.95f, 0.95f, 1);
        }

        #region Rendering

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            RC.ModelView = MoveInScene();
           

            if (VIEW_MODE == ViewMode.VoxelSpace)
                RC.SetShaderParam(_yBoundsParam, _yBounds);
            
            List<Mesh> meshes = VIEW_MODE == ViewMode.PointCloud ?  _pointCloud.GetMeshes() : _voxelSpace.GetMeshes();
            for (var i=0; i<meshes.Count; i++) // foreach would not work here because of multithreading
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
            if (Touch.TwoPoint)
            {
                if (!_twoTouchRepeated)
                {
                    _twoTouchRepeated = true;
                    _angleRollInit = Touch.TwoPointAngle - _angleRoll;
                    _maxPinchSpeed = 0;
                }
                _zoomVel = Touch.TwoPointDistanceVel * -0.01f;
                _angleRoll = Touch.TwoPointAngle - _angleRollInit;
                float pinchSpeed = Touch.TwoPointDistanceVel;
                if (pinchSpeed > _maxPinchSpeed) _maxPinchSpeed = pinchSpeed; // _maxPinchSpeed is used for debugging only.
            }
            else
            {
                _twoTouchRepeated = false;
                _zoomVel = Mouse.WheelVel * -0.5f;
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
            else if (Touch.GetTouchActive(TouchPoints.Touchpoint_0) && !Touch.TwoPoint)
            {
                _keys = false;
                float2 touchVel;
                touchVel = Touch.GetVelocity(TouchPoints.Touchpoint_0);
                _angleVelHorz = -RotationSpeed * touchVel.x * 0.0002f;
                _angleVelVert = -RotationSpeed * touchVel.y * 0.0002f;
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

            if (Mouse.RightButton || Touch.TwoPoint)
            {
                float2 speed = Mouse.Velocity + Touch.TwoPointMidPointVel;
                MoveX += speed.x * -0.005f;
                MoveY += speed.y * -0.005f;
            }


            // Scale Points with W and A
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

        #region Various Event Handler

        /// <summary>
        /// Event handler for when a new node gets added to the octree.
        /// </summary>
        /// <param name="node">The node that was added.</param>
        private void OnNewNodeAdded(OctreeNode<OctreeNodeStates> node)
        {
            if (node.Data == OctreeNodeStates.Occupied && node.SideLength == VOXEL_SIZE)
            {
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

            if(VIEW_MODE == ViewMode.PointCloud)
            {
                // add point to point cloud
                
                PointMesh pointMesh = new PointMesh(point.Position);

                if (point.Color != null)
                {
                    uint color = (uint)new ColorUint((float3) point.Color, 1);
                    pointMesh.Colors = new uint[] { color, color, color, color };
                }

                _pointCloud.AddMesh(pointMesh);
                //_pointCloud.Apply(); // don't hit apply, unless you wish to the memory to explode (make sure it gets baked at the end)
            }
            else
            {
                _octree.Add(point.Position, OctreeNodeStates.Occupied);
            }
        }

        /// <summary>
        /// Event handler for when asset has finished loading.
        /// </summary>
        private void OnAssetLoaded()
        {
            // apply remaining vertices etc.

            if (VIEW_MODE == ViewMode.PointCloud)
            {
                _pointCloud.Bake();
            }
            else
            {
                _voxelSpace.Bake();
            }
                
        }
        
        // update cameraPivot, whenever bounding box of point cloud gets updated
        private void OnBoundingBoxUpdate(BoundingBox boundingBox)
        {
            _cameraPivot = boundingBox.GetCenterPoint();

            if (VIEW_MODE == ViewMode.VoxelSpace)
            {
                _yBounds.x = boundingBox.GetMinValues().y;
                _yBounds.y = boundingBox.GetMaxValues().y;
            }
        }

        #endregion
    }
}