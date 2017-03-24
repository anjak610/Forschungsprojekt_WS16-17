using Fusee.Engine.Core;
using Fusee.Math.Core;
using System.Collections.Generic;
using System.Linq;

namespace Fusee.Tutorial.Core.Common
{

    /// <summary>
    /// In contrast to the Fusee.Engine.Core.Mesh class, this class can store vertices, triangles and so on dynamically. 
    /// This means, you can add more vertices and triangles to it by passing in another mesh.
    /// 
    /// usage:
    /// // create
    /// DynamicMesh dynamicMesh = new DynamicMesh();
    /// 
    /// // render
    /// List<Mesh> meshes = dynamicMesh.GetMeshes(); // iterate through meshes and call RC.Render(meshes[i])
    /// 
    /// // add
    /// dynamicMesh.AddMesh(Mesh anotherMesh); // changes will not be visible
    /// dynamicMesh.Apply(); // changes will be visible, but not yet merged into one mesh
    /// dynamicMesh.Bake(); // meshes will be merged together, but this operation consumes performance; call this on the end of loading or something
    /// 
    /// </summary>
    public class DynamicMesh
    {
        // Because 1 mesh can only take up to 65.535 indices in the triangles ushort[] array,
        // we need multiple meshes.
        private List<Mesh> _meshes;

        // temporary mesh, for making changes visible
        private Mesh _temporaryMesh;

        // those lists refer to the values that are given in
        private List<float3> _vertices;
        private List<float3> _normals;
        private List<ushort> _triangles;
        private List<float2> _uvs;
        private List<uint> _colors;

        /// <summary>
        /// Initializes lists in order to prepare for getting other meshes in.
        /// </summary>
        public DynamicMesh()
        {
            _meshes = new List<Mesh>();
            _temporaryMesh = new Mesh();

            ResetLists();
        }

        /// <summary>
        /// Returns the meshes for rendering.
        /// </summary>
        /// <returns>List of meshes.</returns>
        public List<Mesh> GetMeshes()
        {
            if(_temporaryMesh.Vertices == null)
            {
                return _meshes;
            }
            else
            {
                List<Mesh> result = new List<Mesh>();

                result.AddRange(_meshes);
                result.Add(_temporaryMesh);

                return result;
            }
        }
        
        /// <summary>
        /// Here is where the magic happens. When another mesh comes in, it gets merged with already existing meshes.
        /// </summary>
        /// <param name="mesh">Another mesh to add</param>
        public void AddMesh(Mesh mesh)
        {
            if (mesh.Vertices == null)
                return;

            // triangles ushort list only stores indices of vertices, so we only need to consider the vertices list for not being larger than 65000 items,
            // since ushort can only take values up to 65000.
            if (_vertices.Count + mesh.Vertices.Length > 65000)
            {
                Bake();
            }

            ushort currentIndex = (ushort) _vertices.Count; // index of next value, not the last one (that would be -1)

            _vertices.AddRange(mesh.Vertices);

            if(mesh.Normals != null)
                _normals.AddRange(mesh.Normals);

            if(mesh.UVs != null)
                _uvs.AddRange(mesh.UVs);

            if(mesh.Colors != null)
                _colors.AddRange(mesh.Colors);

            // now comes the tricky part: the triangles

            if(mesh.Triangles != null)
            {
                foreach (ushort vertexIndex in mesh.Triangles)
                {
                    _triangles.Add((ushort)(vertexIndex + currentIndex));
                }
            }
        }

        /// <summary>
        /// Take the values stored in the lists and merge them into a real mesh => temporary mesh
        /// </summary>
        public void Apply()
        {
            Mesh mesh = new Mesh();

            mesh.Vertices = _vertices.ToArray();
            mesh.Normals = _normals.ToArray();
            mesh.Triangles = _triangles.ToArray();
            mesh.UVs = _uvs.ToArray();
            mesh.Colors = _colors.ToArray();

            _temporaryMesh = mesh;

            // do not reset lists, they will be resetted in Bake(), when the _temporaryMesh is full or Bake() gets called
        }

        /// <summary>
        /// This will apply the current values (alias _temporaryMesh), and merge the meshes together.
        /// </summary>
        public void Bake()
        {
            // --- first apply the current values
            Apply();

            // --- then merge the meshes together

            // TO-DO: look on the last mesh, if space is still available
            _meshes.Add(_temporaryMesh);

            // --- at last, reset lists
            ResetLists();
        }

        /// <summary>
        /// Resets the lists, in which values of other meshes are stored.
        /// </summary>
        private void ResetLists()
        {
            _temporaryMesh = new Mesh();

            _vertices = new List<float3>();
            _normals = new List<float3>();
            _triangles = new List<ushort>();
            _uvs = new List<float2>();
            _colors = new List<uint>();
        }
    }
}
