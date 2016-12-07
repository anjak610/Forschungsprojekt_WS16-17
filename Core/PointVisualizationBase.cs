using System.Collections.Generic;
using System.Linq;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using static Fusee.Engine.Core.Input;

namespace Fusee.Tutorial.Core
{

    [FuseeApplication(Name = "Forschungsprojekt", Description = "HFU Wintersemester 16-17")]
    public class PointVisualizationBase : RenderCanvas
    {
        private Mesh[] _meshes;

        //Sceneviewer Parameters    
        private float _maxPinchSpeed;
        private float _minPinchSpeed;
        private bool _scaleKey;
        private bool _twoTouchRepeated;

        //End ScneneViewer      

        private IShaderParam _particleSizeParam;
        private IShaderParam _xFormParam;
        private IShaderParam _screenSizeParam;

        private float4x4 _xform;
        private float2 _screenSize;
                    
        private List<float3> normals = new List<float3>();
        List<float3> vertices = new List<float3>();
        List<ushort> triangles = new List<ushort>();

        private float _alpha;
        private float _beta;

        private const float ParticleSize = 0.05f;
        
        // Init is called on startup. 
        public override void Init()
        {
            // create mesh from pointcloud
            PointCloud pointCloud = AssetStorage.Get<PointCloud>("PointCloud_IPM2.txt");
            _meshes = pointCloud.ToMeshArray();

            //For SceneViewer
            _twoTouchRepeated = false;      
            _twoTouchRepeated = false; 
            
            //read shaders from files
            var vertsh = AssetStorage.Get<string>("VertexShader.vert");
            var pixsh = AssetStorage.Get<string>("PixelShader.frag");

            // Initialize the shader(s)
            var shader = RC.CreateShader(vertsh, pixsh);
            RC.SetShader(shader);

            _particleSizeParam = RC.GetShaderParam(shader, "particleSize");
            RC.SetShaderParam(_particleSizeParam, new float2(ParticleSize, ParticleSize));

            _xFormParam = RC.GetShaderParam(shader, "xForm");
            _xform = float4x4.Identity;

            _screenSizeParam = RC.GetShaderParam(shader, "screenSize");
            _screenSize = new float2(Width, Height); //set screen/window dimensions according to the current viewport
            //RC.SetShaderParam(_xFormParam, float4x4.CreateScale(0.5f) * float4x4.CreateTranslation(-2, -33, 34));

            // Set the clear color for the backbuffer
            RC.ClearColor = new float4(0.95f, 0.95f, 0.95f, 1);
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            List<float3> normals = _meshes[0].Normals.ToList();

            float2 speed = Mouse.Velocity + Touch.GetVelocity(TouchPoints.Touchpoint_0);
            if (Mouse.LeftButton || Touch.GetTouchActive(TouchPoints.Touchpoint_0))
            {
                _alpha -= speed.x * 0.0001f;
                _beta -= speed.y * 0.0001f;
            }

            var view = float4x4.CreateRotationY(_alpha) * float4x4.CreateRotationX(_beta);
            _xform = RC.Projection * float4x4.CreateTranslation(0, 0, 5.0f) * view;
            
            if (Keyboard.ADAxis != 0 || Keyboard.WSAxis != 0 )
            {
                _scaleKey = true;
            }
            else
            {
                _scaleKey = false;
            }

            if (_scaleKey)
            {
                for (int i = 0; i < normals.Count; i++)
                {
                    normals[i] = normals[i] + Keyboard.ADAxis*(normals[i]/200) ;
                 }
            }       

            if (Touch.TwoPoint)
            {
                if (!_twoTouchRepeated)
                {
                    _twoTouchRepeated = true;          
                    _maxPinchSpeed = 0;
                    _minPinchSpeed = 0;
                }
 
                float pinchSpeed = Touch.TwoPointDistanceVel;
                if (pinchSpeed > _maxPinchSpeed)
                {
                    _maxPinchSpeed = pinchSpeed;
                    for (int i = 0; i < normals.Count; i++)
                    {
                        normals[i] = normals[i] + (normals[i] / 10);
                    }
                }
                else if(pinchSpeed < _minPinchSpeed)
                {
                    _minPinchSpeed = pinchSpeed;
                    for (int i = 0; i < normals.Count; i++)
                    {
                        normals[i] = normals[i] - (normals[i] / 10);
                    }
                }
            }
            else
            {
                _twoTouchRepeated = false;                    
               
            }    
            
            RC.SetShaderParam(_xFormParam, _xform);

            foreach (var mesh in _meshes)
            {
                RC.Render(mesh);
            }

            // Swap buffers: Show the contents of the backbuffer (containing the currently rendered frame) on the front buffer.
            Present();
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
            var projection = float4x4.CreatePerspectiveFieldOfView(3.141592f * 0.25f, aspectRatio, 1, 20000);
            RC.Projection = projection;
        }

    }
}