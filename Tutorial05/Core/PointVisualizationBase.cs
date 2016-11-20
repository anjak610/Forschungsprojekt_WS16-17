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
        IShaderParam _particleSizeParam;
        private float _alpha;
        private float _beta;

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
        attribute vec3 fuNormal;
        uniform vec2 particleSize;
        uniform mat4 xForm;
        varying vec3 modelpos;
        
    void main()
    {
        modelpos = fuVertex;
        vec4 vScreen = xForm*vec4(fuVertex, 1.0);       
        gl_Position = vScreen + vec4(fuNormal.xy*particleSize, 0, 0);
    }";

        //pixel shader called after vertex shader 
        private const string _pixelShader = @"
    #ifdef GL_ES
        precision highp float;     
    #endif
        varying vec3 modelpos;

    void main()
    {
        gl_FragColor = vec4(1, 0.5f, modelpos.z*0.01+ 0.8, 1);
    }";
        private IShaderParam _xFormParam;


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
            _particleSizeParam = RC.GetShaderParam(shader, "particleSize");
            RC.SetShaderParam(_particleSizeParam, new float2(0.01f, 0.01f));

            _xFormParam = RC.GetShaderParam(shader, "xForm");
            RC.SetShaderParam(_xFormParam, float4x4.CreateScale(0.5f)*float4x4.CreateTranslation(-2,-33, 34));

            _mesh = new Mesh();
          
            float3 pickedvertex;
            List<float3> vertices = new List<float3>();
            List<float3> normals = new List<float3>();
            List<ushort> triangles = new List<ushort>();

            for (var i = 0; i< cloud.Vertices.Count; i++)
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

                triangles.Add((ushort)(0 + i*4));
                triangles.Add((ushort)(1 + i*4));
                triangles.Add((ushort)(3 + i*4));
                triangles.Add((ushort)(0 + i*4));
                triangles.Add((ushort)(3 + i*4));
                triangles.Add((ushort)(2 + i*4));

           
                
            }

            _mesh.Vertices = vertices.ToArray();
            _mesh.Normals = normals.ToArray();
            _mesh.Triangles = triangles.ToArray();


            // Set the clear color for the backbuffer
            RC.ClearColor = new float4(0.50f, 0.80f, 0.65f, 1);
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

            float2 speed = Mouse.Velocity + Touch.GetVelocity(TouchPoints.Touchpoint_0);
            if (Mouse.LeftButton || Touch.GetTouchActive(TouchPoints.Touchpoint_0))
            {
                _alpha -= speed.x*0.0001f;
                _beta  -= speed.y*0.0001f;
            }
            
       
         
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
            var aspectRatio = Width/(float) Height;
            RC.SetShaderParam(_particleSizeParam, new float2(0.01f,0.01f*aspectRatio));//set params that can be controlled with arrow keys

            // 0.25*PI Rad -> 45° Opening angle along the vertical direction. Horizontal opening angle is calculated based on the aspect ratio
            // Front clipping happens at 1 (Objects nearer than 1 world unit get clipped)
            // Back clipping happens at 2000 (Anything further away from the camera than 2000 world units gets clipped, polygons will be cut)
            var projection = float4x4.CreatePerspectiveFieldOfView(3.141592f * 0.25f, aspectRatio, 1, 20000);
            RC.Projection = projection;
        }

    }
}