using Fusee.Engine.Core;
using Fusee.Math.Core;

/// <summary>
/// Holds the geometry for rendering a single point. Acts not as a datastructure for storing values readed from the IPM format. 
/// See <see cref="Point"/> for that. 
/// </summary>

namespace Fusee.Tutorial.Core
{
    public class PointMesh : Mesh
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Point" /> class.
        /// </summary>
        public PointMesh(float3 position)
        {
            Vertices = new[]
            {
                position, position, position, position
            };

            Normals = new[]
            {
                new float3(-1, -1, 0),
                new float3(1, -1, 0),
                new float3(-1, 1, 0),
                new float3(1, 1, 0)
            };

            UVs = new[]
            {
                new float2(0, 0),
                new float2(0, 1),
                new float2(1, 0),
                new float2(1, 1)
            };

            Triangles = new ushort[]
            {
                0, 1, 3,
                0, 3, 2
            };
        }
    }
}
