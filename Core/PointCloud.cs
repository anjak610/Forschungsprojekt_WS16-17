using System.Collections.Generic;
using System.Diagnostics;
using Fusee.Engine.Core;
using Fusee.Math.Core;

namespace Fusee.Tutorial.Core
{
    /// <summary>
    /// PointCloud holds basically an array of Points. In this class methods are defined which
    /// operate on the whole point cloud.
    /// </summary>

    public class PointCloud
    {
        private List<Point> _points;       
        
        public List<Point> GetPoints()
        {
            return _points;
        }

        public PointCloud()
        {
            _points = new List<Point>();
        }

        public PointCloud(List<Point> points)
        {
            _points = new List<Point>();
            AddPoints(points);
        }

        public void AddPoints(List<Point> points)
        {
            _points.AddRange(points);
        }

        // Takes another pointcloud and adds its points to the point array
        public void Merge(PointCloud pointCloud)
        {
            AddPoints(pointCloud.GetPoints());
        }

        // Converts point array into an array of meshes
        // and adds for each point four vertices in order to display a quad mesh.
        // Because 1 mesh can only take up to 65.535 indices in the triangles ushort[] array,
        // we need multiple meshes.
        public Mesh[] ToMeshArray()
        {
            int meshCount = _points.Count * 4 / 65000 + 1; // how many meshes will be generated?
            Mesh[] meshArray = new Mesh[meshCount];

            var pointIndex = 0; // pointer to the index of the current point in the point cloud

            for (var i = 0; i < meshCount; i++) // for each mesh
            {
                Mesh mesh = new Mesh();  
                                
                List<float3> vertices = new List<float3>();
                List<float3> normals = new List<float3>();
                List<ushort> triangles = new List<ushort>();
                List<float2> uvs = new List<float2>();               

                for(var j=0; 3 + (j + 1) * 4 < 65000 && pointIndex < _points.Count; j++) // for each point
                {
                    float3 pickedvertex = _points[pointIndex].Position;

                    vertices.Add(pickedvertex);
                    vertices.Add(pickedvertex);
                    vertices.Add(pickedvertex);
                    vertices.Add(pickedvertex);

                    normals.Add(new float3(-1, -1, 0));
                    normals.Add(new float3(1, -1, 0));
                    normals.Add(new float3(-1, 1, 0));
                    normals.Add(new float3(1, 1, 0));                        

                    triangles.Add((ushort)(0 + j * 4));
                    triangles.Add((ushort)(1 + j * 4));
                    triangles.Add((ushort)(3 + j * 4));
                    triangles.Add((ushort)(0 + j * 4));
                    triangles.Add((ushort)(3 + j * 4));
                    triangles.Add((ushort)(2 + j * 4));

                    uvs.Add(new float2(0, 1));
                    uvs.Add(new float2(0, 0));
                    uvs.Add(new float2(1, 0));
                    uvs.Add(new float2(1, 1));
                    uvs.Add(new float2(0, 1));                   

                    pointIndex++;
                }

                mesh.Vertices = vertices.ToArray();
                mesh.Normals = normals.ToArray();
                mesh.Triangles = triangles.ToArray();
                mesh.UVs = uvs.ToArray(); 
                //UVs = mc.UVs;

                meshArray[i] = mesh;
            }
            
            return meshArray;
        }
    }
}