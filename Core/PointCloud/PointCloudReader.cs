using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Math.Core;
using System;
using Fusee.Engine.Common;

namespace Fusee.Tutorial.Core.PointCloud
{
    /// <summary>
    /// This class loads an asset and fires callbacks whenever a new point gets loaded. May be used in the future to load
    /// points over a network stream.
    /// </summary>
    
    public class PointCloudReader
    {
        // callback for when new points gets added
        public delegate void OnNewPointAdded(Point point);
        public static OnNewPointAdded OnNewPointCallbacks;

        // callback when asset has been fully loaded
        public static Action OnAssetLoadedCallbacks;

        // store asset name to load from
        private static string _assetName;

        // store connection data to receive from
        private static int _port;

        public static Action<int> StartStreamingUDPCallback;

        /// <summary>
        /// Starts loading from the specified asset.
        /// </summary>        
        /// <param name="assetName">The path to the asset to load from.</param>
        public static void ReadFromAsset(string assetName)
        {
            _assetName = assetName;

            Task task = new Task(StreamFromAsset);
            task.Start();
        }

        public static void ReceiveFromUDP(int port)
        {
            _port = port;

            Task task = new Task(StreamFromUDP);
            task.Start();
        }

        private static void StreamFromAsset()
        {
            Stream storage = IO.StreamFromFile("Assets/" + _assetName, FileMode.Open);
            using (var sr = new StreamReader(storage))
            {
                string line;
                while ((line = sr.ReadLine()) != null) // read per line
                {
                    Point point = ConvertTextToPoint(line);

                    if (point != null)
                        OnNewPointCallbacks?.Invoke(point);
                }

                OnAssetLoadedCallbacks?.Invoke();
            }
        }

        private static void StreamFromUDP()
        {
            // i have port and ip, so what now?
            // => I need a function which i can hand it over
            // => and a second function which takes the final point => ConvertToPoint
            
            StartStreamingUDPCallback?.Invoke(_port);
        }

        // called from UDPReceiver
        public static void ConvertBytesToPoint(byte[] data)
        {
            float[] points = ConvertBytesToFloat(data);

            Point point = new Point();
            point.Position = new float3(points[0], points[1], points[2]);

            OnNewPointCallbacks?.Invoke(point);
        }

        private static float[] ConvertBytesToFloat(byte[] array)
        {
            float[] floatArr = new float[array.Length / 4];
            for (int i = 0; i < floatArr.Length; i++)
            {
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(array, i * 4, 4);
                }
                floatArr[i] = BitConverter.ToSingle(array, i * 4);
            }
            return floatArr;
        }

        private static Point ConvertTextToPoint(string line)
        {
            string delimiter = "\t";
            string[] textElements = line.Split(delimiter.ToCharArray());

            if (textElements.Length == 1) // empty line
                return null;

            Point point = new Point();

            // convert each string to float

            float[] numbers = new float[textElements.Length];
            for (var i = 0; i < numbers.Length; i++)
            {
                numbers[i] = float.Parse(textElements[i], CultureInfo.InvariantCulture.NumberFormat);

                // prevent fusee from the same thing that happend to ariane 5
                // because e.g. 16 - 0.0000001f = 16, but Math.Floor(0.1E-06 / 16) = -1 and that isn't what we want
                if (numbers[i] < 0.000001f && numbers[i] > -0.000001f) 
                {
                    numbers[i] = 0;
                }
            }

            point.Position = new float3(numbers[0], numbers[2], numbers[1]);

            if (numbers.Length == 9)
            {
                point.Color = new float3(numbers[3], numbers[4], numbers[5]);

                point.EchoId = numbers[6];
                point.ScanNr = numbers[8];
            }

            return point;
        }
    }
}
