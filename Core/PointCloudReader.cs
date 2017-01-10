using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Engine.Core;
using Fusee.Math.Core;

namespace Fusee.Tutorial.Core
{
    /// <summary>
    /// This class loads an asset and creates a point cloud out of it. May be used in the future to load
    /// points over a network stream.
    /// </summary>
    
    public class PointCloudReader
    {
        public delegate void AddPointCloud(PointCloud pointCloud); // use this as callback

        private static string _assetName;
        private static AddPointCloud _callback;
        
        public static void ReadFromAsset(string assetName, AddPointCloud callback)
        {
            _assetName = assetName;
            _callback = callback;

            Task task = new Task(StreamFromAsset);
            task.Start();
        }

        private static void StreamFromAsset()
        {
            PointCloud pointCloud = new PointCloud();

            Stream storage = IO.StreamFromFile("Assets/" + _assetName, FileMode.Open);
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
                    for (var i = 0; i < numbers.Length; i++)
                    {
                        numbers[i] = float.Parse(textElements[i], CultureInfo.InvariantCulture.NumberFormat);
                    }

                    point.Position = new float3(numbers[0], numbers[2], numbers[1]);

                    if (numbers.Length == 9)
                    {
                        point.Color = new float3(numbers[3], numbers[4], numbers[5]);
                        point.EchoId = numbers[6];
                        point.ScanNr = numbers[8];
                    }

                    bool newMeshCreated = pointCloud.AddPoint(point);

                    if (newMeshCreated)
                    {
                        _callback(pointCloud);
                        pointCloud = new PointCloud();
                    }
                }
            }
        }
    }
}
