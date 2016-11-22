using System.Collections.Generic;
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
        private List<float3> _vertices;
        private List<float3> _colors;
        private List<float> _echoIds;
        private List<float> _scanNrs;

        //constructor
        public PointCloud() 
      {
               _vertices = new List<float3>();
                _colors = new List<float3>();
                _echoIds = new List<float>();
                _scanNrs = new List<float>();

    }


        public List<float3> Vertices
        {
            get { return _vertices; }
            set { _vertices = value; }
        }

        public List<float3> Colors
        {
            get { return _colors; }
            set { _colors = value; }
        }

        public List<float> EchoIds
        {
            get { return _echoIds; }
            set { _echoIds = value; }
        }

        public List<float> ScanNrs
        {
            get { return _scanNrs; }
            set { _scanNrs = value; }
        }
    }
}
