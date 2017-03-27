using Fusee.Engine.Core;
using Fusee.Math.Core;

/// <summary>
/// Acts as a data structure for storing values of the IPM format. 
/// For the mesh representation of a point see <see cref="PointMesh"/>. 
/// </summary>

namespace Fusee.Tutorial.Core.PointCloud
{
    public class Point
    {
        public float3 Position;
        public float3? Color;
        public float? EchoId;
        public float? ScanNr;
    }
}
