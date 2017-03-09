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

namespace Fusee.Forschungsprojekt.Core
{

    [FuseeApplication(Name = "Forschungsprojekt", Description = "HFU Wintersemester 16-17")]
    public class PointVisualizationBase : RenderCanvas
    {
        private PointCloud _pointCloud;

        // camera controls
        private float _rotationY = (float)System.Math.PI;
        private float _rotationX = (float)System.Math.PI / -8;
        private float3 _cameraPosition = new float3(0, 0, -20.0f);
        private float3 _cameraPivot = new float3(60, -25, 10);
        private static float _zoom, _zoomVel, _angleHorz = M.PiOver6 * 2.0f, _angleVert = -M.PiOver6 * 0.5f, _angleVelHorz, _angleVelVert, _angleRoll, _angleRollInit;
        private bool _twoTouchRepeated;
        private float _maxPinchSpeed;


        private float _alpha;
        private float _beta;
        private float MoveX;
        float MoveY;

        private float _minAngleX = (float)-System.Math.PI / 4;
        private float _maxAngleX = (float)System.Math.PI / 4;

        private float2 _offset;
        private static float2 _offsetInit;
        private const float Damping = 2f;
        private const float RotationSpeed = 4;
        private float4x4 _projection;
     

        // shader parameters
        private IShaderParam _particleSizeParam;
       // private IShaderParam _xFormParam;

        public float ParticleSize = 0.015f;// = 0.015f;
        private float4x4 _xform;
        private bool _scaleKey;
        private bool _keys;

        // Init is called on startup. 
        public override void Init()
        {
            //_pointCloud = new PointCloud();
            _pointCloud = AssetStorage.Get<PointCloud>("BasicPoints.txt");
            PointCloudReader.ReadFromAsset("PointCloud_IPM.txt", _pointCloud.Merge);

            // read shaders from files
            var vertsh = AssetStorage.Get<string>("VertexShader.vert");
            var pixsh = AssetStorage.Get<string>("PixelShader.frag");

            // Initialize the shader(s)
            var shader = RC.CreateShader(vertsh, pixsh);
            RC.SetShader(shader);

            _particleSizeParam = RC.GetShaderParam(shader, "particleSize");
            RC.SetShaderParam(_particleSizeParam, new float2(ParticleSize, ParticleSize));

          //  _xFormParam = RC.GetShaderParam(shader, "xForm");
          //  _xform = float4x4.Identity;
          //  RC.SetShaderParam(_xFormParam, _xform);

            // Set the clear color for the backbuffer
            RC.ClearColor = new float4(0.95f, 0.95f, 0.95f, 1);

            _zoom = 60;
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {

            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            MoveInSceneNew();
           // MoveInScene();

            List<Mesh> meshes = _pointCloud.GetMeshes();

            for (var i = 0; i < meshes.Count; i++)
            {
                RC.Render(meshes[i]);
            }

            BoundingBox();

            // float aspectRatio = Width / (float)Height;

            // set origin to camera pivot
            // var pivot = float4x4.CreateTranslation(-1 * _cameraPivot);
            // set camera to its position
            float aspectRatio = Width / (float)Height;
            RC.SetShaderParam(_particleSizeParam, new float2(ParticleSize, ParticleSize * aspectRatio));

            Present();
        }

       public void MoveInSceneNew()
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
                   _offsetInit = Touch.TwoPointMidPoint - _offset;
                   _maxPinchSpeed = 0;
               }
               _zoomVel = Touch.TwoPointDistanceVel * -0.01f;
               _angleRoll = Touch.TwoPointAngle - _angleRollInit;
               _offset = Touch.TwoPointMidPoint - _offsetInit;
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
               _angleVelHorz = -RotationSpeed * touchVel.x * 0.000002f;
               _angleVelVert = -RotationSpeed * touchVel.y * 0.000002f;
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
       
           var view = float4x4.CreateTranslation(-1 * _cameraPivot); //-1 * _cameraPosition) *
       
           var MtxCam = float4x4.LookAt(0, 0, _zoom, 0, 0, 0, 0, 1, 0) * float4x4.CreateTranslation(MoveX, MoveY, 0);
           var MtxRot = float4x4.CreateRotationZ(_angleRoll) * float4x4.CreateRotationX(_angleVert) * float4x4.CreateRotationY(_angleHorz);
           RC.ModelView = MtxCam * MtxRot * view;
       
           var mtxOffset = float4x4.CreateTranslation(2 * _offset.x / Width, -2 * _offset.y / Height, 0);
           RC.Projection = mtxOffset * _projection;
       }

        //public void MoveInScene()
     //  {
     //      // set origin to camera pivot
     //      _xform = float4x4.CreateTranslation(-1 * _cameraPivot);
     //
     //      // rotate around camera pivot
     //      if (Mouse.LeftButton || Touch.ActiveTouchpoints == 1)
     //      {
     //          float2 speed = Mouse.Velocity + Touch.GetVelocity(TouchPoints.Touchpoint_0);
     //
     //          _rotationY -= speed.x * 0.0001f;
     //          _rotationX -= speed.y * 0.0001f;
     //
     //          // clamp rotation x between min and max angle
     //          _rotationX = _rotationX < _minAngleX ? _minAngleX : (_rotationX > _maxAngleX ? _maxAngleX : _rotationX);
     //      }
     //
     //      var rotation = float4x4.CreateRotationX(_rotationX) * float4x4.CreateRotationY(_rotationY);
     //      _xform = rotation * _xform;
     //
     //      // set camera to its position
     //      _xform = float4x4.CreateTranslation(-1 * _cameraPosition) * _xform;
     //
     //      // --- move camera pivot
     //
     //      float3 translation = float3.Zero;
     //
     //      if (Mouse.RightButton || Touch.TwoPoint)
     //      {
     //          float2 speed = Mouse.Velocity + Touch.TwoPointMidPointVel;
     //          translation.x = speed.x * -0.005f;
     //          translation.y = speed.y * 0.005f;
     //      }
     //
     //      if (Mouse.Wheel != 0 || Touch.TwoPoint)
     //      {
     //          float speed = Mouse.WheelVel + Touch.TwoPointDistanceVel * 0.1f;
     //          translation.z = speed * 0.1f;
     //      }
     //
     //      if (translation.Length > 0)
     //      {
     //          rotation.Invert();
     //          translation = rotation * translation;
     //          _cameraPivot += translation;
     //          //_xform = float4x4.CreateTranslation(translation) * _xform;
     //      }
     //
     //      //Scale Points with W and A
     //      if (Keyboard.ADAxis != 0 || Keyboard.WSAxis != 0)
     //      {
     //          _scaleKey = true;
     //      }
     //      else
     //      {
     //          _scaleKey = false;
     //      }
     //
     //      if (_scaleKey)
     //      {
     //          ParticleSize = ParticleSize + Keyboard.ADAxis * ParticleSize / 20;
     //      }
     //
     //      // --- projection matrix
     //
     //      _xform = _projection * _xform;
     //      RC.SetShaderParam(_xFormParam, _xform);
     //  }

        // Is called when the window was resized

        public void BoundingBox()
        {
           List<float3> positions = _pointCloud.GetPositions();
         
           //var test = positionsArray[0].x;
           float _maxX = positions[0].x;
           float _minX = positions[0].x;
            float _maxY = positions[0].y;
            float _minY = positions[0].y;
            float _maxZ = positions[0].z;
            float _minZ = positions[0].z;

            for (int i = 0; i < positions.Count; i++)
           {
                // Meridian = (1 / 2) * (positions[i / 2].x + (positions[i / 2].x + 1));
              if (_minX < positions[i].x)
                {
                    _minX = positions[i].x;
                }

               if (_maxX > positions[i].x)
               {
                   _maxX = positions[i].x;
               }

                if (_minZ < positions[i].z)
                {
                    _minZ = positions[i].z;
                }

                if (_maxZ > positions[i].z)
                {
                    _maxZ = positions[i].z;
                }

                if (_minY < positions[i].y)
                {
                    _minY = positions[i].y;
                }

                if (_maxY > positions[i].y)
                {
                    _maxY = positions[i].y;
                }
            }


           //TODO compute Meridian x,y,z  and create BoundingBox --> how to transport Boudningbox to Shader???

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