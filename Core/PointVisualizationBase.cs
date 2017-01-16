using Fusee.Base.Core;
using Fusee.Base.Common;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using static Fusee.Engine.Core.Input;
using System.Collections.Generic;
using System.Diagnostics;

namespace Fusee.Tutorial.Core
{

    [FuseeApplication(Name = "Forschungsprojekt", Description = "HFU Wintersemester 16-17")]
    public class PointVisualizationBase : RenderCanvas
    {
        private PointCloud _pointCloud;
       
        // camera controls
        private float _rotationY = 0.0f;
        private float _rotationX = 0.0f;
        private float3 _cameraPosition = new float3(0, 0, -20.0f);
        private float3 _cameraPivot = new float3(0, 0, 0);

        private const float Damping = 0.8f;

        //Sceneviewer Parameters    
        private float _maxPinchSpeed;
        private float _minPinchSpeed;
        private bool _scaleKey;
        private bool _twoTouchRepeated;
        private static float _zoomVel, _zoom;
        private bool _mouseWheel;               
        
        private static float2 _offset;
        private static float2 _offsetInit;
        private static float _offsetInitMouseX;
        private static float _offsetInitMouseY;
        private static float _offsetMouseX;
        private static float _offsetMouseY;
        //End ScneneViewer 

        //Get Shader Parameters
        private IShaderParam _particleSizeParam;
       // private IShaderParam _xFormParam;

        private IShaderParam _tex;
        private ITexture _newTex;

       // private Dictionary<string, ITexture> _textueLookUp =  new Dictionary<string, ITexture>();

       // private float4x4 projection;

       // private float4x4 _xform;
        public float2 _screenSize;

        private float _alpha;
        private float _beta;
        private float2 speed = new float2();

        public float ParticleSize;

        // Init is called on startup. 
        public override void Init()
        {
            // screenSize --> now requested from android device and windows screen
            _screenSize = new float2(Width, Height);

            //Width = 1920.0f;
            //Height = 1080.0f;
            _pointCloud = AssetStorage.Get<PointCloud>("PointCloud_IPM2.txt");

            //For SceneViewer
            _twoTouchRepeated = false;
            // _twoTouchRepeated = false;
            _zoom = 0;
            _offsetMouseX = 0f;
            _offsetMouseY = 0f;
            _offset = float2.Zero;
            _offsetInit = float2.Zero;

            //read shaders from files
            var vertsh = AssetStorage.Get<string>("VertexShader.vert");
            var pixsh = AssetStorage.Get<string>("PixelShader.frag");
            var texture = AssetStorage.Get<ImageData>("BlackPoint.png");            
            
            // Initialize the shader(s)
            var shader = RC.CreateShader(vertsh, pixsh);
            RC.SetShader(shader);        

            _particleSizeParam = RC.GetShaderParam(shader, "particleSize");
            RC.SetShaderParam(_particleSizeParam, new float2(ParticleSize, ParticleSize));

           // _xFormParam = RC.GetShaderParam(shader, "xForm");
           // _xform = float4x4.Identity;

            //Load texture and save into ITexture _newTex           
            _newTex = RC.CreateTexture(texture);
           // _textueLookUp.Add("black_sphere.png", _newTex);
            _tex = RC.GetShaderParam(shader, "tex");
            RC.SetShaderParamTexture(_tex, _newTex);

            //RC.SetShaderParam(_xFormParam, float4x4.CreateScale(0.5f) * float4x4.CreateTranslation(-2, -33, 34));

            // Set the clear color for the backbuffer
            RC.ClearColor = new float4(0.95f, 0.95f, 0.95f, 1);
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {            
            
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            MoveInScene();

            var rotation = float4x4.CreateRotationX(_rotationX) * float4x4.CreateRotationY(_rotationY);
            RC.ModelView = rotation * float4x4.CreateScale(0.5f, 0.1f, 0.1f);
            // RC.SetShaderParam(_xFormParam, _xform);

            foreach (var mesh in _pointCloud.GetMeshes())
            {
                RC.Render(mesh);                
            }

            var aspectRatio = Width / (float)Height;                  

            RC.SetShaderParam(_particleSizeParam, new float2(ParticleSize, ParticleSize * aspectRatio));

            RC.SetRenderState(new RenderStateSet
            {
                AlphaBlendEnable = true,
                SourceBlend = Blend.SourceAlpha,
                DestinationBlend = Blend.InverseSourceAlpha,
                BlendOperation = BlendOperation.Add,
                // In case of particles:
                ZEnable = true,
                ZWriteEnable = false,
            });

            // Swap buffers: Show the contents of the backbuffer (containing the currently rendered frame) on the front buffer.
            Present();
        }

        public void MoveInScene()
        {
            // set origin to camera pivot
            //_xform = float4x4.CreateTranslation(-1 * _cameraPivot);

            //Scale Points with W and A
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
            // rotate around camera pivot
            if (Mouse.LeftButton || Touch.ActiveTouchpoints == 1)
            {
                float2 speed = Mouse.Velocity + Touch.GetVelocity(TouchPoints.Touchpoint_0);

                _rotationY -= speed.x * 0.0001f;
                _rotationX -= speed.y * 0.0001f;
            }

            var rotation = float4x4.CreateRotationX(_rotationX) * float4x4.CreateRotationY(_rotationY);
           // _xform = rotation * _xform;

            // set camera to its position
           // _xform = float4x4.CreateTranslation(-1 * _cameraPosition) * _xform;

            // --- move camera pivot

            float3 translation = float3.Zero;

            if (Mouse.RightButton || Touch.TwoPoint)
            {
                float2 speed = Mouse.Velocity + Touch.TwoPointMidPointVel;
                translation.x = speed.x * -0.005f;
                translation.y = speed.y * 0.005f;
            }

            if (Mouse.Wheel != 0 || Touch.TwoPoint)
            {
                float speed = Mouse.WheelVel + Touch.TwoPointDistanceVel * 0.1f;
                translation.z = speed * 0.1f;
            }

            if (translation.Length > 0)
            {
                rotation.Invert();
                translation = rotation * translation;
                _cameraPivot += translation;
                //_xform = float4x4.CreateTranslation(translation) * _xform;
            }

            // --- projection matrix

          //  _xform = RC.Projection * _xform;
            //RC.SetShaderParam(_xFormParam, _xform);
        }

        // Is called when the window was resized
        public override void Resize()
        {
            // Set the new rendering area to the entire new windows size
            RC.Viewport(0, 0, Width, Height);

            // Create a new projection matrix generating undistorted images on the new aspect ratio.
            var aspectRatio = Width / (float)Height;
            //RC.SetShaderParam(_particleSizeParam, new float2(ParticleSize, ParticleSize * aspectRatio)); //set params that can be controlled with arrow keys

            // Question: should we set particleSize depending on aspect ratio or rather define an amount of pixels, thus taking window size into computation?

            // 0.25*PI Rad -> 45° Opening angle along the vertical direction. Horizontal opening angle is calculated based on the aspect ratio
            // Front clipping happens at 1 (Objects nearer than 1 world unit get clipped)
            // Back clipping happens at 2000 (Anything further away from the camera than 2000 world units gets clipped, polygons will be cut)

            //RC.SetShaderParam(_particleSizeParam, new float2(ParticleSize, ParticleSize));

            // RC.Projection = projection * mtxOffsetDesktop * mtxOffset * mtxCam;

            RC.Projection = float4x4.CreatePerspectiveFieldOfView(3.141592f * 0.25f, aspectRatio, 1, 2000);
            // RC.Projection = projection;
        }

    }
}