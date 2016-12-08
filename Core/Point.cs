using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fusee.Math.Core;

/// <summary>
/// Represents a simple point which holds all properties given by the IPM format.
/// </summary>

namespace Fusee.Tutorial.Core
{
    public class Point
    {
        public float3 Position { get; set; }
        public float3 Color { get; set; }
        public float EchoId { get; set; }
        public float ScanNr { get; set; }
    }
}
