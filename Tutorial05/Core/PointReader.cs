using Fusee.Base.Core;
using Fusee.Tutorial.Core;
using Fusee.Math.Core;
using System.Globalization;
using System.Diagnostics;

/// <summary>
/// Provides a class that reads vertex values from .txt file and creates a point cloud object with vertex list
/// </summary>

namespace Fusee.Tutorial.Desktop
{

    public class PointReader
    {
        //member variables
        private PointCloud _cloud;

        //constructor
        public PointReader(PointCloud cloudinput)
        {
            _cloud = cloudinput;
        }

        //getter & setter
        public PointCloud Cloud
        {
            get { return _cloud; }
            set { _cloud = value; }

        }
        public void readPointList()
        {

            string readText = AssetStorage.Get<string>("small_test_points.txt");

            string[] lines = readText.Split('\n');

            //split in lines, split those in separate values for vertices
            for (var n = 0; n < lines.Length; n++)
            {
                string[] vertexValues = lines[n].Split('\t');
                for (var m = 0; m < vertexValues.Length; m++)
                {
                    float x = (float.Parse(vertexValues[0], CultureInfo.InvariantCulture.NumberFormat));
                    float y = (float.Parse(vertexValues[1], CultureInfo.InvariantCulture.NumberFormat));
                    float z = (float.Parse(vertexValues[2], CultureInfo.InvariantCulture.NumberFormat));
                    float z2 = (float.Parse(vertexValues[3], CultureInfo.InvariantCulture.NumberFormat)); //What is the fourth value? Also declared as Z
                    float3 insertvalue = new float3(x, y, z);
                    _cloud.Vertices.Add(insertvalue);
                    Debug.WriteLine(" Point Reader added point to cloud: x:" + x + " y:" + y + " z:" + z + " z2:" + z2);
                }
            }

        }

      }   
}
    

