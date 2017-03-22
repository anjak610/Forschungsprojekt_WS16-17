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
using System.Collections.Generic;
using System.Linq;

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
            //_callback = callback;//TODO OnData Received Feedback when the displaying function is done or buffer for received data 
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
            List<string> pointElementLines = new List<string>();

            pointElementLines = receivedData.Split(delimiter.ToCharArray()).ToList();// received string split into lines

            //interate through every line
            for (int j = 1; j < pointElementLines.Count; j++)
            {
                try
                {
                    string tab = "\t";//split into numbers
                    List<string> pointStrings = new List<string>();
                    pointStrings = pointElementLines[j].Split(tab.ToCharArray()).ToList();// line string split to point coordinates
                    Point point = new Point();

                    // convert each string to float and add to respective attributes of point objects
                    float[] numbers = new float[pointStrings.Count];
                    for (var i = 0; i < numbers.Length; i++)//does not get greater than 4 or 9
                    {
                        numbers[i] = float.Parse(pointStrings[i], CultureInfo.InvariantCulture.NumberFormat);
                    }

                    if (numbers.Length >= 3)//make sure that there are enough values to create one point
                    {
                        point.Position = new float3(numbers[0], numbers[1], numbers[2]);

                        if (numbers.Length == 9)
                        {
                            point.Color = new float3(numbers[4], numbers[5], numbers[6]); //numbers[3] => second z coordinate
                            point.EchoId = numbers[7];
                            point.ScanNr = numbers[8];
                        }

                        bool newMeshCreated = pointCloud.AddPoint(point);

                        if (newMeshCreated)//new mesh if the limit of 65000 vertices is reached
                        {
                            Core.PointVisualizationBase._pointCloud.Merge(pointCloud);
                            pointCloud = new PointCloud();
                        }

                    }

                    //pointCloud.FlushPoints(); //Flush points if point data is less than 65000 vertices
                }
                catch
                {
                    continue; //skip line if wrong number format or whatever 
                }

            }
            Core.PointVisualizationBase._pointCloud.Merge(pointCloud);//after reading all lines merge point cloud into existing one
        }
    }
}

