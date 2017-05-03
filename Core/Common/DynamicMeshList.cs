using Fusee.Engine.Core;
using System.Collections.Generic;

namespace Fusee.Tutorial.Core.Common
{
    /// <summary>
    /// Holds and manages a list of <see cref="DynamicMesh"/>. Whenever a <see cref="DynamicMesh"/> is full, it gets added to the meshes 
    /// list, while <see cref="GetMeshes()"/> returns the meshes list as well as the current dynamic mesh.
    /// </summary>
    public class DynamicMeshList
    {
        private List<DynamicMesh> _meshes; // where all full meshes are stored
        private DynamicMesh _currentMesh; // to this mesh everything gets added

        public DynamicMeshList()
        {
            _meshes = new List<DynamicMesh>();
            _currentMesh = new DynamicMesh();
        }

        /// <summary>
        /// Adds another mesh to the whatever this is.
        /// </summary>
        /// <param name="mesh">The mesh to add.</param>
        public void AddMesh(Mesh mesh)
        {
            if (!_currentMesh.AddMesh(mesh))
            {
                _meshes.Add(_currentMesh);
                _currentMesh = new DynamicMesh();
                _currentMesh.AddMesh(mesh);
            }
        }

        /// <summary>
        /// Returns all meshes contained by this class.
        /// </summary>
        public List<DynamicMesh> GetMeshes()
        {
            List<DynamicMesh> meshes = new List<DynamicMesh>();

            meshes.AddRange(_meshes);
            meshes.Add(_currentMesh);

            return meshes;
        }
    }
}
