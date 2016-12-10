using System.Collections.Generic;
using System.Linq;
using Fusee.Engine.Core;
using Fusee.Math.Core;

namespace Fusee.Tutorial.Core
{
    /// <summary>
    /// PointCloud holds the meshes which will be pushed through the rendering pipeline. It acts as a data structure 
    /// where points can be dynamically loaded into.
    /// </summary>

    public class PointCloud
    {
        // Because 1 mesh can only take up to 65.535 indices in the triangles ushort[] array,
        // we need multiple meshes.
        private List<Mesh> _meshes;

        // those lists refer to the properties of one mesh
        private List<float3> _vertices;
        private List<float3> _normals;
        private List<ushort> _triangles;

        private int _currentIndex = 0; // index of the current point

        public PointCloud()
        {
            _meshes = new List<Mesh>();
            ResetMesh();
        }

        public List<Mesh> GetMeshes()
        {
            return _meshes;
        }
        
        // Instantiate a new mesh with full capacity of maximum 65000 points
        private void ResetMesh()
        {
            _currentIndex = 0;

            _vertices = new List<float3>();
            _normals = new List<float3>();
            _triangles = new List<ushort>();
        }

        // Creates a mesh by the current vertices, normals and triangles and adds it to the mesh list.
        private void AddCurrentToMeshes()
        {
            Mesh mesh = new Mesh();

            mesh.Vertices = _vertices.ToArray();
            mesh.Normals = _normals.ToArray();
            mesh.Triangles = _triangles.ToArray();

            _meshes.Add(mesh);

            ResetMesh();
        }

        public void AddPoint(Point point)
        {
            if (3 + _currentIndex * 4 > 65000)
            {
                AddCurrentToMeshes();
            }

            float3 pickedvertex = point.Position;

            _vertices.Add(pickedvertex);
            _vertices.Add(pickedvertex);
            _vertices.Add(pickedvertex);
            _vertices.Add(pickedvertex);
            
            _normals.Add(new float3(-1, -1, 0));
            _normals.Add(new float3(1, -1, 0));
            _normals.Add(new float3(-1, 1, 0));
            _normals.Add(new float3(1, 1, 0));
            
            _triangles.Add((ushort)(0 + _currentIndex * 4));
            _triangles.Add((ushort)(1 + _currentIndex * 4));
            _triangles.Add((ushort)(3 + _currentIndex * 4));
            _triangles.Add((ushort)(0 + _currentIndex * 4));
            _triangles.Add((ushort)(3 + _currentIndex * 4));
            _triangles.Add((ushort)(2 + _currentIndex * 4));

            _currentIndex++;
        }

        // Takes another pointcloud and adds its meshes to the mesh array
        public void Merge(PointCloud pointCloud)
        {
            _meshes.AddRange(pointCloud.GetMeshes());
        }
    }
}