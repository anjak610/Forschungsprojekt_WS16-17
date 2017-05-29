using Fusee.Math.Core;
using Fusee.Tutorial.Core.Common;
using Fusee.Tutorial.Core.Data_Transmission;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using static Fusee.Tutorial.Core.Data_Transmission.AssetReader;
using Fusee.Tutorial.Core.PointClouds;

namespace Fusee.Tutorial.Android.HelperClasses
{
    /// <summary>
    /// Receives input and hands it over.
    /// </summary>

    public class UDPReceiver : IUDPReceiver
    {
        public OnNewPointAdded OnNewPointCallbacks { get; set; }
        public Action<float3> OnDronePositionCallbacks { get; set; }

        private int _port;

        public void StreamFrom(int port)
        {
            _port = port;

            Task task = new Task(StreamFromSub);
            task.Start();
        }

        private void StreamFromSub()
        {
            UdpClient client = new UdpClient(_port);
            IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                try
                {
                    // Bytes empfangen.
                    byte[] data = client.Receive(ref anyIP);

                    // byte array convert to float values
                    byte[] dataPosition = data.Skip(1).ToArray();
                    float[] position = ConvertByteToFloat(dataPosition);

                    if (data[0] == 255) // drone
                    {
                        float3 dronePosition = new float3(position[0], position[1], position[2]);
                        OnDronePositionCallbacks?.Invoke(dronePosition);
                    }
                    else // laser
                    {
                        Point point = new Point();
                        point.Position = new float3(position[0], position[1], position[2]);

                        OnNewPointCallbacks?.Invoke(point);
                    }
                }
                catch (Exception err)
                {
                    Console.WriteLine(err.ToString());
                }
            }
        }

        private static float[] ConvertByteToFloat(byte[] array)
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
    }
}