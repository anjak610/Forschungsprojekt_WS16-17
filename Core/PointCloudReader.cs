using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Math.Core;

namespace Fusee.Tutorial.Core
{
    /// <summary>
    /// This class loads an asset and creates a point cloud out of it. May be used in the future to load
    /// points over a network stream.
    /// </summary>
    
    public class PointCloudReader
    {
        private PointCloud _pointCloud; // reference to the main point cloud

        public PointCloudReader(ref PointCloud pointCloud)
        {
            _pointCloud = pointCloud;
        }

        public void ReadFromAsset(string assetName, ref PointCloud pointCloud)
        {
            Task readTask = Task.Factory.StartNew(() =>
            {
                Stream storage = IO.StreamFromFile("Assets/" + assetName, FileMode.Open);
                using (var sr = new StreamReader(storage))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null) // read per line
                    {
                        string delimiter = "\t";
                        string[] textElements = line.Split(delimiter.ToCharArray());

                        if (textElements.Length == 1) // empty line
                            continue;

                        Point point = new Point();

                        // convert each string to float
                        float[] numbers = new float[textElements.Length];
                        for (var i=0; i<numbers.Length; i++)
                        {
                            numbers[i] = float.Parse(textElements[i], CultureInfo.InvariantCulture.NumberFormat);
                        }

                        point.Position = new float3(numbers[0], numbers[1], numbers[2]);

                        if (numbers.Length == 9)
                        {
                            point.Color = new float3(numbers[3], numbers[4], numbers[5]);
                            point.EchoId = numbers[6];
                            point.ScanNr = numbers[8];
                        }

                        pointCloud.AddPoint(point);
                    }
                }

                pointCloud.FlushPoints();
            });
        }
    }
}
