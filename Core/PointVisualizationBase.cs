using System.Collections.Generic;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using static Fusee.Engine.Core.Input;
using Fusee.Tutorial.Desktop;

namespace Fusee.Tutorial.Core
{

    [FuseeApplication(Name = "Forschungsprojekt", Description = "HFU Wintersemester 16-17")]
    public class PointVisualizationBase : RenderCanvas
    {
        private Mesh[] _meshes;

        private IShaderParam _particleSizeParam;
        private IShaderParam _xFormParam;

        private float4x4 _xform;
        
        private float _alpha;
        private float _beta;

        private const float ParticleSize = 0.05f;
        
        // Init is called on startup. 
        public override void Init()
        {
            // read point cloud from file and assign vertices to cloud object
            //PointCloud cloud = AssetStorage.Get<PointCloud>("PointCloud_IPM2.txt");
            PointCloud cloud = AssetStorage.Get<PointCloud>("TestPoints.txt");
            PointCloud basicPoints = AssetStorage.Get<PointCloud>("BasicPoints.txt");

            //cloud = PointCloud.Merge(basicPoints, cloud);
            cloud = PointCloud.Merge(cloud, basicPoints); // does not work?

            //cloud = new PointCloud();
            //PointReader preader = new PointReader(cloud);
            //PointReader preader.readPointList();

            // TO-DO:
            // 1. Warum wird (cloud,basicPoints) nicht angezeigt?
            // Aus irgendeinem Grund kann OpenGL nur 4096 Punkte (x4) darstellen, d.h. man bräuchte dann ne Art Vertex Buffer
            // -> mehrere Meshes aus einer PCL -> macht kein Sinn -> Fusee Source Code ändern
            // 
            // 2. Verdrehte Achsen
            // 3. PointCloud lesen auch auf Android

            _meshes = ConvertPointCloudToMeshes(cloud);

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

            //RC.SetShaderParam(_xFormParam, float4x4.CreateScale(0.5f) * float4x4.CreateTranslation(-2, -33, 34));

            // Set the clear color for the backbuffer
            RC.ClearColor = new float4(0.95f, 0.95f, 0.95f, 1);
        }

        // Converts a point cloud into meshes.
        private Mesh[] ConvertPointCloudToMeshes(PointCloud pointCloud)
        {
            // Because 1 mesh can only take up to 65.535 indices in the triangles ushort[] array,
            // we need multiple meshes.

            List<Mesh> meshesList = new List<Mesh>();
            Mesh currentMesh = new Mesh();

            float3 pickedvertex;
            List<float3> vertices = new List<float3>();
            List<float3> normals = new List<float3>();
            List<ushort> triangles = new List<ushort>();

            for(var i=0; i<pointCloud.Vertices.Count; i++)
            {
                //vertex list times 4
                pickedvertex = pointCloud.Vertices[i];
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

                // regarding that triangles can only take indices up to 65535, we need to place more vertices in another mesh

                var nextLoopMaxIndex = 3 + (vertices.Count + 1) * 4;
                if (nextLoopMaxIndex > 65535 || i == pointCloud.Vertices.Count - 1)
                {
                    currentMesh.Vertices = vertices.ToArray();
                    currentMesh.Normals = normals.ToArray();
                    currentMesh.Triangles = triangles.ToArray();

                    vertices = new List<float3>();
                    normals = new List<float3>();
                    triangles = new List<ushort>();

                    meshesList.Add(currentMesh);
                    currentMesh = new Mesh();
                }
            }

            return meshesList.ToArray();
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            float2 speed = Mouse.Velocity + Touch.GetVelocity(TouchPoints.Touchpoint_0);
            if (Mouse.LeftButton || Touch.GetTouchActive(TouchPoints.Touchpoint_0))
            {
                _alpha -= speed.x * 0.0001f;
                _beta -= speed.y * 0.0001f;
            }

            var view = float4x4.CreateRotationY(_alpha) * float4x4.CreateRotationX(_beta);
            _xform = RC.Projection * float4x4.CreateTranslation(0, 0, 5.0f) * view;

            RC.SetShaderParam(_xFormParam, _xform);

            foreach (var mesh in _meshes)
            {
                RC.Render(mesh);
            }
            
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