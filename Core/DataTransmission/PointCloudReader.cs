using System;

using System.Collections.Generic;

using System.Globalization;

using System.IO;

using System.Linq;

using System.Threading.Tasks;

using Fusee.Base.Common;

using Fusee.Base.Core;

using Fusee.Math.Core;

using Fusee.Tutorial.Core.Common;

using System.Threading;



namespace Fusee.Tutorial.Core.DataTransmission

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

        //float[] values = new float[7];



        public static PointCalculator pointCalc = new PointCalculator();



        // store connection data to receive from

        private static int _port;



        public static Action<int> StartStreamingUDPCallback;



        private static byte[] pendingfile;
        private static int overallPaketCount = 0;
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



            List<UInt32[]> pointPakets = new List<UInt32[]>();

            List<float[]> droneInfos = new List<float[]>();

            while (file.Length > 2056)//at least one paket must be there //>=2052
            {
                if (CheckHeader(file))
                {
                    try
                    {
                        float[] info = ReadDronePos(file);
                        droneInfos.Add(info);
                        UInt32[] packet = CreatePaket(file);
                        pointPakets.Add(packet);
                        overallPaketCount++;
                        Diagnostics.Log("Packet count:" + overallPaketCount);

                        for (int i = 0; i < packet.Length; i++)//interate through points in packet and render

                        {
                            //extract values with bitshift
                            int echoId = (int)packet[i] >> 30;//get first to bits
                            int intensity = (int)((packet[i] << 2) >> 26);//skip 2 bits take next 6 bits
                            float distance = (packet[i] << 8);//skip 8 bits --> 8 bit left shift
                            distance = distance / (10 * 10 * 100 * 100);
                            //Diagnostics.Log("Distance:" + distance);
                            //Diagnostics.Log("EchoId:" + echoId);
                            //Diagnostics.Log("Intensity:" + intensity);                    
                            pointCalc.GetDistance(distance); //fill list with distance values

                        }

                        float3[] values = pointCalc.CalculateNewPoint(pointCalc._dist, 90f); //calculate 500 points of paket
                        ConvertCalculatedPointsToPoints(values); //create point objects
                        file = file.Skip(2056).ToArray();//go to next paket

                    }
                    catch
                    {
                        Diagnostics.Log("Error creating hex point packets");
                    }
                }
                else
                {
                    Diagnostics.Log("Paket not vaild");
                    if (file.Length > 2056)
                    {
                        Diagnostics.Log("Looking for new pakets");
                        pendingfile = file;
                        Task t = new Task(LookforMarkers);
                        t.Start();
                    }
                    else Diagnostics.Log("File reading done");
                    break;
                }
            }
        }
        public static void LookforMarkers()
        {
            int i = 0;

            while (pendingfile[i] == 0)
            {
                i++;
            }

           pendingfile = pendingfile.Skip(i).ToArray();

            while (BitConverter.ToUInt32((SubArray(pendingfile, i, 4)), 0) != 0xFEEDBEEF)
            {
                i++;
            }

            pendingfile = pendingfile.Skip(i).ToArray();

            //start paket readout again
            ReadFromBinary(pendingfile);

        }

        public static void ConvertCalculatedPointsToPoints(float3[] _points)

        {

            foreach (var singlePoint in _points)

            {
                Point point = new Point();
                point.Position = singlePoint;
                //Diagnostics.Log("New Point: " + point.Position);
                OnNewPointCallbacks?.Invoke(point);

            }

        }


        public static bool CheckHeader(byte[] file)

        {

            UInt32 packetBeginMarker = BitConverter.ToUInt32((SubArray(file, 0, 4)), 0);

            UInt16 typeID = BitConverter.ToUInt16((SubArray(file, 4, 2)), 0);

            UInt16 version = BitConverter.ToUInt16((SubArray(file, 6, 2)), 0);

            UInt32 packetSize = BitConverter.ToUInt32((SubArray(file, 8, 4)), 0);

            //double time = BitConverter.ToDouble((SubArray(file, 12, 8)), 0);

            //TimeSpan timespan = TimeSpan.FromMilliseconds(time);



            //Print out on console for testing

            //Diagnostics.Log("Packet Begin Marker: 0x" + packetBeginMarker.ToString("X"));

            //Diagnostics.Log("TypeID: 0x" + typeID.ToString("X"));

            //Diagnostics.Log("Version: 0x" + version.ToString("X"));

            //Diagnostics.Log("Packet Size: " + packetSize);

            //Diagnostics.Log("Time milliseconds: " + time);



            return ((packetBeginMarker == 0xFEEDBEEF) && (typeID == 0x1010) && (version == 0x0001));

        }





        public static float[] ReadDronePos(byte[] file)

        {

            float[] values = new float[7];

            //Position of Scanner

            float drone_posX = BitConverter.ToSingle((SubArray(file, 20, 4)), 0);

            float drone_posY = BitConverter.ToSingle((SubArray(file, 24, 4)), 0);

            float drone_posZ = BitConverter.ToSingle((SubArray(file, 28, 4)), 0);



            //Orientation

            float quaternionW = BitConverter.ToSingle((SubArray(file, 32, 4)), 0);

            float quaternionX = BitConverter.ToSingle((SubArray(file, 36, 4)), 0);

            float quaternionY = BitConverter.ToSingle((SubArray(file, 40, 4)), 0);

            float quaternionZ = BitConverter.ToSingle((SubArray(file, 44, 4)), 0);



            values[0] = drone_posX;

            values[1] = drone_posY;

            values[2] = drone_posZ;

            values[3] = quaternionW;

            values[4] = quaternionX;

            values[5] = quaternionY;

            values[6] = quaternionZ;



            pointCalc.GetValues(drone_posX, drone_posY, drone_posZ, quaternionX, quaternionY, quaternionZ, quaternionW);

            //Debug

            //Diagnostics.Log("Pos X: " + drone_posX);

            //Diagnostics.Log("Pos Y: " + drone_posY);

            //Diagnostics.Log("Pos Z: " + drone_posZ);

            //Diagnostics.Log("Quaternion W: " + quaternionW);

            //Diagnostics.Log("Quaternion X: " + quaternionX);

            //Diagnostics.Log("Quaternion Y: " + quaternionY);

            //Diagnostics.Log("Quaternion Z: " + quaternionZ);





            //pointCalc.GetValues();

            return values;



        }



        public static T[] SubArray<T>(T[] data, int index, int length)

        {

            T[] result = new T[length];

            Array.Copy(data, index, result, 0, length);

            return result;

        }



        private static UInt32[] CreatePaket(byte[] array)

        {

            UInt32 numberOfPoints = BitConverter.ToUInt32((SubArray(array, 48, 4)), 0);

           // Diagnostics.Log("Number of points: " + numberOfPoints);

            List<UInt32> tmpList = new List<UInt32>();

            int count = 52;

            UInt32 limit = 2056; //((numberOfPoints * 4) + 52 + 4); --> last 4 bytes should be beginn marker of next paket

            while (count <= limit)

            {

                UInt32 piece = BitConverter.ToUInt32(SubArray(array, count, 4), 0);

                if (piece != 0xFEEDBEEF)

                {
                    tmpList.Add(piece);
                }
                
                count = count + 4;

            }

            tmpList.RemoveAt(500);//Remove packet begin marker and create an array



            return tmpList.ToArray();

        }





    }

}

