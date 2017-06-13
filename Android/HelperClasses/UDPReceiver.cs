using Fusee.Math.Core;
using Fusee.Tutorial.Core.Data_Transmission;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using static Fusee.Tutorial.Core.Data_Transmission.AssetReader;

namespace Fusee.Tutorial.Android.HelperClasses
{
    /// <summary>
    /// Receives input and hands it over.
    /// </summary>

    public class UDPReceiver : IUDPReceiver
    {
        public OnNewPointAdded OnNewPointCallbacks { get; set; }
        public Action<float3> OnDronePositionCallbacks { get; set; }

        private UdpClient client;
        private int _port;

        public void Listen()
        {
            if (client == null)
                return;

            Task task = new Task(StreamFromSub);
            task.Start();
        }

        private void StreamFromSub()
        {
            IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                try
                {
                    // Bytes empfangen.
                    byte[] data = client.Receive(ref anyIP);
                }
                catch (Exception err)
                {
                    Console.WriteLine(err.ToString());
                }
            }
        }

        public void SetPort(int port)
        {
            client.Close();
            client = new UdpClient(port);
        }
    }
}