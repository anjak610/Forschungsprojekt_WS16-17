using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using Fusee.Serialization;
using Fusee.Xene;
using static Fusee.Engine.Core.Input;
using Fusee.Tutorial.Core;
using System.Globalization;
using Fusee.Tutorial.Desktop;

namespace Fusee.Tutorial.Core
{

    [FuseeApplication(Name = "Forschungsprojekt", Description = "HFU Wintersemester 16-17")]
    public class PointVisualizationBase : RenderCanvas
    {
        private Mesh _mesh;
        public static PointCloud cloud;
        public static PointReader preader; 

        public static Mesh LoadMesh(string assetName)
        {
            SceneContainer sc = AssetStorage.Get<SceneContainer>(assetName);
            MeshComponent mc = sc.Children.FindComponents<MeshComponent>(c => true).First();
            return new Mesh
            {
                Vertices = mc.Vertices,
                Normals = mc.Normals,
                Triangles = mc.Triangles
            };
        }

        private const string _vertexShader = @"
     attribute vec3 fuVertex;

    void main()
    {
        gl_Position = vec4(fuVertex, 1.0);
    }";

        private const string _pixelShader = @"
    #ifdef GL_ES
        precision highp float;
    #endif

    void main()
    {
        gl_FragColor = vec4(1, 0, 1, 1);
    }";
        

        // Init is called on startup. 
        public override void Init()
        {
            // read point cloud from file and assign vertices to cloud object
            cloud = new PointCloud();
            preader = new PointReader(cloud);
            preader.readPointList();

            //read shaders from files
            //var vertsh = AssetStorage.Get<string>("VertexShader.vert");
            //var pixsh = AssetStorage.Get<string>("PixelShader.frag");

            // Initialize the shader(s)
            var shader = RC.CreateShader(_vertexShader, _pixelShader);
            RC.SetShader(shader);
    
            // Load a mesh
            _mesh = new Mesh
            {
                Vertices = new[]
            {
                new float3(-0.75f, -0.75f, 0),
                new float3(0.75f, -0.75f, 0),
                new float3(0, 0.75f, 0)

               },
                Triangles = new ushort[] { 0, 1, 2, 3 },
            };


            // Set the clear color for the backbuffer
            RC.ClearColor = new float4(1, 1, 1, 1);
        }

        static float4x4 ModelXForm(float3 pos, float3 rot, float3 pivot)
        {
            return float4x4.CreateTranslation(pos + pivot)
                   *float4x4.CreateRotationY(rot.y)
                   *float4x4.CreateRotationX(rot.x)
                   *float4x4.CreateRotationZ(rot.z)
                   *float4x4.CreateTranslation(-pivot);
        }


        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            /*float2 speed = Mouse.Velocity + Touch.GetVelocity(TouchPoints.Touchpoint_0);
            if (Mouse.LeftButton || Touch.GetTouchActive(TouchPoints.Touchpoint_0))
            {
                _alpha -= speed.x*0.0001f;
                _beta  -= speed.y*0.0001f;
            }*/

            RC.Render(_mesh);
            // Swap buffers: Show the contents of the backbuffer (containing the currently rendered frame) on the front buffer.
            Present();


        }


        // Is called when the window was resized
        public override void Resize()
        {
            // Set the new rendering area to the entire new windows size
            RC.Viewport(0, 0, Width, Height);

            // Create a new projection matrix generating undistorted images on the new aspect ratio.
            var aspectRatio = Width/(float) Height;

            // 0.25*PI Rad -> 45° Opening angle along the vertical direction. Horizontal opening angle is calculated based on the aspect ratio
            // Front clipping happens at 1 (Objects nearer than 1 world unit get clipped)
            // Back clipping happens at 2000 (Anything further away from the camera than 2000 world units gets clipped, polygons will be cut)
            var projection = float4x4.CreatePerspectiveFieldOfView(3.141592f * 0.25f, aspectRatio, 1, 20000);
            RC.Projection = projection;
        }

    }
}