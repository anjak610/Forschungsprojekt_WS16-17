using System.Collections.Generic;
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
        // maximum number of points
        private long _limit = 0;

        // Because 1 mesh can only take up to 65.535 indices in the triangles ushort[] array,
        // we need multiple meshes.
        private List<Mesh> _meshes;

        //those lists refer to the properties of one mesh
        private List<float3> _vertices;
        private List<float3> _normals;
        private List<ushort> _triangles;
        private List<float2> _uvs;

        private int _currentIndex; // index of the current point
        private long _totalNumberOfPoints = 0; // counts all points

        public PointCloud()
        {
            _limit = 0;

            _meshes = new List<Mesh>();
            ResetMesh();
        }

        public PointCloud(long limit)
        {
            _limit = limit; // limit needs to be set at start

            _meshes = new List<Mesh>();
            ResetMesh();
        }

        public List<Mesh> GetMeshes()
        {
            return _meshes;
        }

        public long GetNumberOfPoints()
        {
            return _totalNumberOfPoints;
        }

        // When the last point is added, this method will take the remaining vertices, normals, etc. and
        // add them to the mesh list.
        public void FlushPoints()
        {
            AddCurrentToMeshes();
        }
        
        // Instantiate a new mesh
        private void ResetMesh()
        {
            _currentIndex = 0;

            _vertices = new List<float3>();
            _normals = new List<float3>();
            _triangles = new List<ushort>();
            _uvs = new List<float2>();
        }

        // Creates a mesh by the current vertices, normals and triangles and adds it to the mesh list.
        private void AddCurrentToMeshes()
        {
            Mesh mesh = new Mesh();

            mesh.Vertices = _vertices.ToArray();
            mesh.Normals = _normals.ToArray();
            mesh.Triangles = _triangles.ToArray();
            mesh.UVs = _uvs.ToArray();
            //UVs = mc.UVs;

            _meshes.Add(mesh);
            
            ResetMesh();
        }

        public bool AddPoint(Point point)
        {
            bool newMeshCreated = false;

            if (_limit > 0 && _totalNumberOfPoints <= _limit)
                return false;

            if (3 + _currentIndex * 4 > 65000)
            {
                AddCurrentToMeshes();
                newMeshCreated = true;
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

            _uvs.Add(new float2(0, 0));
            _uvs.Add(new float2(0, 1));
            _uvs.Add(new float2(1, 0));
            _uvs.Add(new float2(1, 1));

            _currentIndex++;
            _totalNumberOfPoints++;

            return newMeshCreated;
        }

        // Takes another pointcloud and adds its meshes to the mesh array
        public void Merge(PointCloud pointCloud)
        {
            lock (_meshes)
            {
                _meshes.AddRange(pointCloud.GetMeshes());
            }
        }
    }
}