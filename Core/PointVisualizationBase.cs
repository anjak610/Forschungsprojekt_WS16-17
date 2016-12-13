using Fusee.Base.Core;
using Fusee.Base.Common;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using static Fusee.Engine.Core.Input;

namespace Fusee.Tutorial.Core
{

    [FuseeApplication(Name = "Forschungsprojekt", Description = "HFU Wintersemester 16-17")]
    public class PointVisualizationBase : RenderCanvas
    {
        private PointCloud _pointCloud;

        //Sceneviewer Parameters    
        private float _maxPinchSpeed;
        private float _minPinchSpeed;
        private bool _scaleKey;       
        private bool _twoTouchRepeated;
        public static float _zoomVel, _zoom;
        public bool _mouseWheel;

        private static float2 _offset;
        private static float2 _offsetInit;
        private static float _offsetInitMouseX;
        private static float _offsetInitMouseY;
        private static float _offsetMouseX;
        private static float _offsetMouseY;
        //End ScneneViewer 

        //Get Shader Parameters
        private IShaderParam _particleSizeParam;
        private IShaderParam _xFormParam;
        private IShaderParam _screenSizeParam;

        private IShaderParam _tex;
        private ITexture _newTex;

        private float4x4 projection;

        private float4x4 _xform;
        public float2 _screenSize;

        private float _alpha;
        private float _beta;
        private float2 speed = new float2();

        public float ParticleSize;

        // Init is called on startup. 
        public override void Init()
        {
            // screenSize --> now requested from android device and windows screen
            //_screenSize = new float2(Width, Height);
            
            _pointCloud = AssetStorage.Get<PointCloud>("PointCloud_IPM2.txt");

            //For SceneViewer
            _twoTouchRepeated = false;
            // _twoTouchRepeated = false;
            _zoom = -10;
            _offsetMouseX = 0f;
            _offsetMouseY = 0f;
            _offset = float2.Zero;
            _offsetInit = float2.Zero;

            //read shaders from files
            var vertsh = AssetStorage.Get<string>("VertexShader.vert");
            var pixsh = AssetStorage.Get<string>("PixelShader.frag");
            var texture = AssetStorage.Get<ImageData>("smoked_oak.png");

            // Initialize the shader(s)
            var shader = RC.CreateShader(vertsh, pixsh);
            RC.SetShader(shader);

            _particleSizeParam = RC.GetShaderParam(shader, "particleSize");
            RC.SetShaderParam(_particleSizeParam, new float2(ParticleSize, ParticleSize));

            _xFormParam = RC.GetShaderParam(shader, "xForm");
            _xform = float4x4.Identity;

            //Load texture and save into ITexture _newTex           
            _newTex = RC.CreateTexture(texture);
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

            RC.SetShaderParam(_xFormParam, _xform);

            foreach (var mesh in _pointCloud.GetMeshes())
            {
                RC.Render(mesh);
            }

            var aspectRatio = Width / (float)Height;

            RC.SetShaderParam(_particleSizeParam, new float2(ParticleSize, ParticleSize * aspectRatio));

            var mtxCam = float4x4.LookAt(55, 0, -_zoom, 55, 0, 0, 0, 1, 0);
            var mtxOffset = float4x4.CreateTranslation(2 * _offset.x / Width, -2 * _offset.y / Height, 0);
            var mtxOffsetDesktop = float4x4.CreateTranslation(2 * _offsetMouseX / Width, -2 * _offsetMouseY / Height, 0);

            RC.Projection = projection * mtxOffsetDesktop * mtxOffset * mtxCam;

            // Swap buffers: Show the contents of the backbuffer (containing the currently rendered frame) on the front buffer.
            Present();
        }

        public void MoveInScene()
        {
            //rotate around Object
            speed = Mouse.Velocity + Touch.GetVelocity(TouchPoints.Touchpoint_0);
            if (Mouse.LeftButton || Touch.GetTouchActive(TouchPoints.Touchpoint_0))
            {
                _alpha -= speed.x * 0.0001f;
                _beta -= speed.y * 0.0001f;
            }
            var view = float4x4.CreateRotationY(_alpha) * float4x4.CreateRotationX(_beta);
            _xform = RC.Projection * float4x4.CreateTranslation(0, 0, 5.0f) * view;

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

            //Move Camer on x- and y-axis through scene by click Right MouseButton

            if (Mouse.RightButton)
            {
                _offsetMouseY += Mouse.YVel - _offsetInitMouseY; //Mouse.YVel;// 
                _offsetMouseX += Mouse.XVel - _offsetInitMouseX; //Mouse.XVel;// 
            }

            // Scale Points with Touch and move camera on x- and y-axis through scene
            if (Touch.TwoPoint) // TODO: Implement scaling with slide movements on screen 
            {
                if (!_twoTouchRepeated)
                {
                    _twoTouchRepeated = true;
                    _maxPinchSpeed = 0;
                    _minPinchSpeed = 0;
                    _offsetInit = Touch.TwoPointMidPoint - _offset;
                }

                _offset = Touch.TwoPointMidPoint - _offsetInit;
                float pinchSpeed = Touch.TwoPointDistanceVel;
                if (pinchSpeed > _maxPinchSpeed)
                {
                    _maxPinchSpeed = pinchSpeed;
                    ParticleSize = ParticleSize + ParticleSize / 2;
                }
                else if (pinchSpeed < _minPinchSpeed)
                {
                    _minPinchSpeed = pinchSpeed;
                    ParticleSize = ParticleSize - ParticleSize / 2;
                }
            }
            else
            {
                _twoTouchRepeated = false;
            }

            //Zoom with Mousewheel
            if (Mouse.Wheel != 0)
            {
                _mouseWheel = true;
            }

            if (_mouseWheel)
            {
                _zoomVel = Mouse.WheelVel * +0.008f;
            }

            _zoom += _zoomVel;
            // Limit zoom
            if (_zoom < -200)
                _zoom = -200;
            if (_zoom > 200)
                _zoom = 200;
        }

        // Is called when the window was resized
        public override void Resize()
        {
            // Set the new rendering area to the entire new windows size
            RC.Viewport(0, 0, Width, Height);

            // Create a new projection matrix generating undistorted images on the new aspect ratio.
            var aspectRatio = Width / (float)Height;
            RC.SetShaderParam(_particleSizeParam, new float2(ParticleSize, ParticleSize * aspectRatio)); //set params that can be controlled with arrow keys

            // Question: should we set particleSize depending on aspect ratio or rather define an amount of pixels, thus taking window size into computation?

            // 0.25*PI Rad -> 45° Opening angle along the vertical direction. Horizontal opening angle is calculated based on the aspect ratio
            // Front clipping happens at 1 (Objects nearer than 1 world unit get clipped)
            // Back clipping happens at 2000 (Anything further away from the camera than 2000 world units gets clipped, polygons will be cut)

            //RC.SetShaderParam(_particleSizeParam, new float2(ParticleSize, ParticleSize));

            // RC.Projection = projection * mtxOffsetDesktop * mtxOffset * mtxCam;

            projection = float4x4.CreatePerspectiveFieldOfView(3.141592f * 0.25f, aspectRatio, 1, 20000);
           // RC.Projection = projection;
        }

    }
}