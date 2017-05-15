using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Math.Core;
using System;
using System.Collections.Generic;
using System.Linq;


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
           
            UInt32 packetBeginMarker = BitConverter.ToUInt32((SubArray(file, 0, 4)),0);
            UInt16 typeID = BitConverter.ToUInt16((SubArray(file, 4, 2)), 0);
            UInt16 version = BitConverter.ToUInt16((SubArray(file, 6, 2)), 0);
            UInt32 packetSize = BitConverter.ToUInt32((SubArray(file, 8, 4)), 0);
            double time = BitConverter.ToDouble((SubArray(file, 12, 8)), 0);

            TimeSpan timespan = TimeSpan.FromMilliseconds(time);


            //Position of Scanner
            float drone_posX = BitConverter.ToSingle((SubArray(file, 20, 4)), 0);
            float drone_posY = BitConverter.ToSingle((SubArray(file, 24, 4)), 0);
            float drone_posZ = BitConverter.ToSingle((SubArray(file, 28, 4)), 0);

            //Orientation
            float quaternionW = BitConverter.ToSingle((SubArray(file, 32, 4)), 0);
            float quaternionX = BitConverter.ToSingle((SubArray(file, 36, 4)), 0);
            float quaternionY = BitConverter.ToSingle((SubArray(file, 40, 4)), 0);
            float quaternionZ = BitConverter.ToSingle((SubArray(file, 44, 4)), 0);
            //number of points
            UInt32 numberOfPoints = BitConverter.ToUInt32((SubArray(file, 48, 4)), 0);

            
           //Print out on console for testing
            Diagnostics.Log("Packet Begin Marker: 0x" + packetBeginMarker.ToString("X"));
            Diagnostics.Log("TypeID: 0x" + typeID.ToString("X"));
            Diagnostics.Log("Version: 0x" + version.ToString("X"));
            Diagnostics.Log("Packet Size: " + packetSize);
            Diagnostics.Log("Time milliseconds: " + time);
            Diagnostics.Log("Pos X: " + drone_posX);
            Diagnostics.Log("Pos Y: " + drone_posY);
            Diagnostics.Log("Pos Z: " + drone_posZ);
            Diagnostics.Log("Quaternion W: " + quaternionW);
            Diagnostics.Log("Quaternion X: " + quaternionX);
            Diagnostics.Log("Quaternion Y: " + quaternionY);
            Diagnostics.Log("Quaternion Z: " + quaternionZ);
            Diagnostics.Log("Number of points: " + numberOfPoints);

            List<UInt32> values = CreateHexList(file);

            foreach (var value in values)
            {
                if (value != 0)
                {
                    Diagnostics.Log("Pointvalue: 0x" + value.ToString("X"));
                }
            }
          
        }

        public static  T[] SubArray<T>( T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        private static List<UInt32> CreateHexList(byte[] array)
        {

           List<UInt32> pointChunks = new List<UInt32>();
            int count = 52;
            while (count < array.Length)
            {
                UInt32 chunk = BitConverter.ToUInt32(SubArray(array, count, 4), 0);
                if (chunk != 0xDEADBEEF)
                {
                    pointChunks.Add(chunk);
                }
                else Diagnostics.Log("End of Package");
               
                count = count + 4;
            }
            return pointChunks;
        }


    }
}
