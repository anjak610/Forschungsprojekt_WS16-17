using System.Collections.Generic;
using Fusee.Base.Core;
using Fusee.Base.Common;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using System.ComponentModel;
using System.Diagnostics;
using static Fusee.Engine.Core.Time;
using static Fusee.Engine.Core.Input;

delegate float3 StartNumber(float3 StrP);

namespace Fusee.Forschungsprojekt.Core
{

    [FuseeApplication(Name = "Forschungsprojekt", Description = "HFU Wintersemester 16-17")]
    public class PointVisualizationBase : RenderCanvas
    {
        private PointCloud _pointCloud;

        // camera controls
        private float3 _cameraPivot = new float3(60, -25, 10);
        private static float _zoom, _zoomVel, _angleHorz = M.PiOver6 * 2.0f, _angleVert = -M.PiOver6 * 0.5f, _angleVelHorz, _angleVelVert, _angleRoll, _angleRollInit;
        private bool _twoTouchRepeated;
        private float _maxPinchSpeed;

        private float MoveX;
        private float MoveY;

       private float _maxX;
       private float _minX;
       private float _maxY;
       private float _minY;
       private float _maxZ;
       private float _minZ;

        private float2 _offset;
        private static float2 _offsetInit;
        private const float Damping = 2f;
        private const float RotationSpeed = 4;
        private float4x4 _projection;

        // shader parameters
        private IShaderParam _particleSizeParam;

        public float ParticleSize = 0.015f;
        private bool _scaleKey;
        private bool _keys;

        static List<float3> positions = new List<float3>();

        // Init is called on startup. 
        public override void Init()
        {

            _pointCloud = new PointCloud();
            // _pointCloud = AssetStorage.Get<PointCloud>("BasicPoints.txt");
            PointCloudReader.ReadFromAsset("PointCloud_IPM.txt", _pointCloud.Merge);

            // read shaders from files
            var vertsh = AssetStorage.Get<string>("VertexShader.vert");
            var pixsh = AssetStorage.Get<string>("PixelShader.frag");

            // Initialize the shader(s)
            var shader = RC.CreateShader(vertsh, pixsh);
            RC.SetShader(shader);

            _particleSizeParam = RC.GetShaderParam(shader, "particleSize");
            RC.SetShaderParam(_particleSizeParam, new float2(ParticleSize, ParticleSize));

            // Set the clear color for the backbuffer
            RC.ClearColor = new float4(0.95f, 0.95f, 0.95f, 1);

            _zoom = 60;
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {

            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            List<Mesh> meshes = _pointCloud.GetMeshes();

            for (var i = 0; i < meshes.Count; i++)
            {
                RC.Render(meshes[i]);
            }

            BoundingBox();
            MoveInScene();

            float aspectRatio = Width / (float)Height;
            RC.SetShaderParam(_particleSizeParam, new float2(ParticleSize, ParticleSize * aspectRatio));

            Present();
        }

        public void MoveInScene()
        {
            //set params that can be controlled with arrow keys

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

            StartNumber StrPoint = new StartNumber(StrP);
            float3 _startPoint = StrPoint(new float3(0,0,0));//positions[1]; //TODO: solution for Android!! Points aren't read at that Time
            var view = float4x4.CreateTranslation(-1 * _startPoint);

            var MtxCam = float4x4.LookAt(0, 0, _zoom, 0, 0, 0, 0, 1, 0) * float4x4.CreateTranslation(MoveX, MoveY, 0);
            var MtxRot = float4x4.CreateRotationZ(_angleRoll) * float4x4.CreateRotationX(_angleVert) * float4x4.CreateRotationY(_angleHorz);
            RC.ModelView = MtxCam * MtxRot * view;

            RC.Projection = _projection;
        
        }
        public static float3 StrP(float3 str)
        {
           // str = positions[0];
            if  (positions.Count != 0)
            {
                return positions[0];
            }
            return str;
        }

        public void BoundingBox()
        {
            positions = _pointCloud.GetPositions();

            //var test = positionsArray[0].x;
            for (int i = 0; i < positions.Count; i++)
            {
                
                if (_minX > positions[i].x)
                {
                    _minX = positions[i].x;
                }

                if (_maxX < positions[i].x)
                {
                    _maxX = positions[i].x;
                }

                if (_minZ > positions[i].z)
                {
                    _minZ = positions[i].z;
                }

                if (_maxZ < positions[i].z)
                {
                    _maxZ = positions[i].z;
                }

                if (_minY > positions[i].y)
                {
                    _minY = positions[i].y;
                }

                if (_maxY < positions[i].y)
                {
                    _maxY = positions[i].y;
                }

                // Debug.WriteLine(_maxX);
            }
            var distX = _maxX + _minX;
            var medianX = distX / 2;
          //  var medianX = System.Math.Abs(distXhalf);

            var distY = _maxY + _minY;
            var medianY = distY / 2;
           // var medianY = System.Math.Abs(distYhalf);

            var distZ = _maxZ + _minZ;
            var medianZ = distZ / 2;
           // var medianZ = System.Math.Abs(distZhalf);

            float3 midpoint = new float3(medianX, medianY, medianZ);
            float _radius = 0;

            if (System.Math.Abs(_maxX) > _radius)
            {
                _radius = _maxX;
            }
            if (System.Math.Abs(_maxY) > _radius)
            {
                _radius = _maxY;
            }
            if (System.Math.Abs(_maxZ) > _radius)
            {
                _radius = _maxZ;
            }

            //TODO: create BoundingBox --> how to transport Boudningbox to Shader???
        }

        public override void Resize()
        {
            // Set the new rendering area to the entire new windows size
            RC.Viewport(0, 0, Width, Height);

            // Create a new projection matrix generating undistorted images on the new aspect ratio.
            float aspectRatio = Width / (float)Height;

            RC.SetShaderParam(_particleSizeParam, new float2(ParticleSize, ParticleSize * aspectRatio)); //set params that can be controlled with arrow keys

            // 0.25*PI Rad -> 45° Opening angle along the vertical direction. Horizontal opening angle is calculated based on the aspect ratio
            // Front clipping happens at 1 (Objects nearer than 1 world unit get clipped)
            // Back clipping happens at 2000 (Anything further away from the camera than 2000 world units gets clipped, polygons will be cut)

            _projection = float4x4.CreatePerspectiveFieldOfView(3.141592f * 0.25f, aspectRatio, 1, 20000);
        }

    }
}