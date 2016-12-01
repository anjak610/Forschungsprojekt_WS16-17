using System.Collections.Generic;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using static Fusee.Engine.Core.Input;
using static Fusee.Engine.Core.Time;
using Fusee.Tutorial.Desktop;
using System.Diagnostics;
using System.Linq;


namespace Fusee.Tutorial.Core
{

    [FuseeApplication(Name = "Forschungsprojekt", Description = "HFU Wintersemester 16-17")]
    public class PointVisualizationBase : RenderCanvas
    {
        private Mesh _mesh;

        private IShaderParam _particleSizeParam;
        private IShaderParam _xFormParam;
        private IShaderParam _screenSizeParam;

        private float4x4 _xform;
        private float2 _screenSize;

        private bool _scaleKey; 
        private bool _twoTouchRepeated;
        private bool _FirstScale;

        private List<float3> normals = new List<float3>();
        List<float3> vertices = new List<float3>();
        List<ushort> triangles = new List<ushort>();

        private float _alpha;
        private float _beta;

        public static PointCloud cloud;
        public static PointReader preader;

        private const float ParticleSize = 0.05f;
        
        // Init is called on startup. 
        public override void Init()
        {
            // read point cloud from file and assign vertices to cloud object
            cloud = new PointCloud();
            preader = new PointReader(cloud);
            preader.readPointList();

            _twoTouchRepeated = false;
            _FirstScale = false;

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

            _mesh = new Mesh();

            float3 pickedvertex;
           // List<float3> vertices = new List<float3>();
            //List<float3> normals = new List<float3>();
           // List<ushort> triangles = new List<ushort>();

            for (var i = 0; i < cloud.Vertices.Count; i++)
            {

                //vertex list times 4
                pickedvertex = cloud.Vertices[i];
                vertices.Add(pickedvertex);
                vertices.Add(pickedvertex);
                vertices.Add(pickedvertex);
                vertices.Add(pickedvertex);

                normals.Add(new float3(-1, -1, 0));
                normals.Add(new float3(1, -1, 0));
                normals.Add(new float3(-1, 1, 0));
                normals.Add(new float3(1, 1, 0));

                triangles.Add((ushort)(0 + i * 4));
                triangles.Add((ushort)(1 + i * 4));
                triangles.Add((ushort)(3 + i * 4));
                triangles.Add((ushort)(0 + i * 4));
                triangles.Add((ushort)(3 + i * 4));
                triangles.Add((ushort)(2 + i * 4));
                
            }

           /* _mesh.Vertices = vertices.ToArray();
            _mesh.Normals = normals.ToArray();
            _mesh.Triangles = triangles.ToArray();*/


            // Set the clear color for the backbuffer
            RC.ClearColor = new float4(0.95f, 0.95f, 0.95f, 1);
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            _mesh.Vertices = vertices.ToArray();
            _mesh.Normals = normals.ToArray();
            _mesh.Triangles = triangles.ToArray();

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

                     for (int i = 0; i < normals.Count; i++)
                     {
                         normals[i] = normals[i] + (normals[i] / 20);
                     }
                 }
                 else
                 {
                     _twoTouchRepeated = false;                
                 }
             }

            RC.SetShaderParam(_xFormParam, _xform);
            RC.Render(_mesh);
            
            //RC.Render(_mesh);
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