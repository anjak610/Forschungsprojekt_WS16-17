using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using System.Net;
using System.Text;

namespace Fusee.Forschungsprojekt.Core
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
        public static string receivedData = "";

        public static void ReadFromAsset(string assetName, AddPointCloud callback)
        {
            _assetName = assetName;
            _callback = callback;
            Task task = new Task(StreamFromAsset);
            task.Start();
        }

        public static void DisplayReceived()
        {
            //_callback = callback;
            StreamFromNetwork();
        }

        private static void StreamFromAsset()//executed in background task
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

        public static void printReceived()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Point reader received data:" + receivedData);

            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Point reader: Problems printing data");
            }
        }

        public static void StreamFromNetwork()//background task executed every time points have been received
        {
            PointCloud pointCloud = new PointCloud();

            string delimiter = "\n";
            string[] pointElementLines = receivedData.Split(delimiter.ToCharArray());// received string split into lines

            for (int j= 1; j< pointElementLines.Length; j++)
            {
                string tab = "\t";//split into numbers
                string[] pointStrings = pointElementLines[j].Split(tab.ToCharArray());// received string

                Point point = new Point();

                // convert each string to float
                float[] numbers = new float[pointStrings.Length];
                for (var i = 0; i < numbers.Length; i++)
                {
                    if (pointStrings[i] != null)
                    {
                        numbers[i] = float.Parse(pointStrings[i], CultureInfo.InvariantCulture.NumberFormat);
                    }
                }

                point.Position = new float3(numbers[0], numbers[1], numbers[2]);

                if (numbers.Length == 9)
                {
                    point.Color = new float3(numbers[3], numbers[4], numbers[5]);
                    point.EchoId = numbers[6];
                    point.ScanNr = numbers[8];
                }

                //pointCloud.AddPoint(point);

                bool newMeshCreated = pointCloud.AddPoint(point);
          

                if (newMeshCreated)//new mesh if the limit of 65000 vertices is reached
                {
                    System.Diagnostics.Debug.WriteLine("created new Mesh");
                    Core.PointVisualizationBase._pointCloud.Merge(pointCloud);
                    pointCloud = new PointCloud();
                }                

            }


            Core.PointVisualizationBase._pointCloud = pointCloud;

        }

    }
}

