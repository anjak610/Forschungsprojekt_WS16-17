using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fusee.Math.Core;

namespace Fusee.Tutorial.Core
{
    /// <summary>
    /// Provides a class for holding points in a point cloud, especially regarding the format of those from IPM.
    /// Format_A: X Y Z Z
    /// Format_B: X Y Z Red(0..1) Green(0..1) Blue(0..1) ECHO_ID Y SCAN_NR
    /// </summary>

    public class PointCloud
    {
        private float3[] _vertices;
        private float3[] _colors;
        private float[] _echoIds;
        private float[] _scanNrs;

        public float3[] Vertices
        {
            get { return _vertices; }
            set { _vertices = value; }
        }

        public float3[] Colors
        {
            get { return _colors; }
            set { _colors = value; }
        }

        public float[] EchoIds
        {
            get { return _echoIds; }
            set { _echoIds = value; }
        }

        public float[] ScanNrs
        {
            get { return _scanNrs; }
            set { _scanNrs = value; }
        }
    }
}
