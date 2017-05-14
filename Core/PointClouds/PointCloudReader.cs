using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Math.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Android.Provider;

namespace Fusee.Tutorial.Core.PointClouds
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

            //*
            Task task = new Task(StreamFromAsset);
            task.Start();
            //*/

            //StreamFromAsset();
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

        public static void ReadFromString(string file)
        {
          
            string separator = "\n";
            string[] lineList = file.Split(separator.ToCharArray());

            for (int i = 0; i < lineList.Length; i++)
            {
                try
                {
                    Point point = ConvertTextToPoint(lineList[i]);

                    if (point != null)
                        OnNewPointCallbacks?.Invoke(point);//add point to point cloud after creation 
                }
                catch (Exception e)
                {
                    //System.Diagnostics.Debug.WriteLine(e);
                    System.Diagnostics.Debug.WriteLine("Line skipped: Error converting string to float");
                }            
            }
                OnAssetLoadedCallbacks?.Invoke();
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
                if (BitConverter.IsLittleEndian)// if little endian format then reverse bit order
                {
                    Array.Reverse(array, i * 4, 4);
                }
                floatArr[i] = BitConverter.ToSingle(array, i * 4);
            }
            return floatArr;
        }

        // Called from StreamFromAsset()
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

        public static void ReadFromBinary(byte[] file)
        {
            Stream stream = new MemoryStream();
            BinaryReader reader = new BinaryReader(stream);

            if (BitConverter.IsLittleEndian)// if little endian format then reverse bit order
            {
                Array.Reverse(file);
            }

            UInt32 packetBeginMarker = (uint)reader.Read(file, 0, 4);
            UInt16 typeID = (ushort) reader.Read(file, 4, 2);
            UInt16 version = (ushort)reader.Read(file, 6, 2); ;
            UInt32 packetSize = (uint)reader.Read(file, 8, 4); ;
            double time = reader.Read(file, 12, 8);
            
            //Position of Scanner
            float drone_posX = reader.Read(file, 20, 4);
            float drone_posY = reader.Read(file, 24, 4);
            float drone_posZ = reader.Read(file, 28, 4);
           
            //Orientation
            float quaternionW = reader.Read(file, 32, 4);
            float quaternionX = reader.Read(file, 36, 4);
            float quaternionY = reader.Read(file, 40, 4);
            float quaternionZ = reader.Read(file, 44, 4);
            //number of points
            UInt32 numberOfPoints = (uint) reader.Read(file, 48, 4);

            //byte[] pointsChunk = (byte[]) file.Skip(51);

            UInt32 values = BitConverter.ToUInt32(file, 52);
            Diagnostics.Log("Pointvalues: " + values);
        }
    }
}
