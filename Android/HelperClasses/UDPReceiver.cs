using Fusee.Tutorial.Core.PointCloud;
using System;
using System.Net;
using System.Net.Sockets;

namespace Fusee.Tutorial.Android.HelperClasses
{
    /// <summary>
    /// Receives input and hands it over.
    /// </summary>

    class UDPReceiver
    {
        public UDPReceiver()
        {

        }

        public void StreamFromUDP(int port)
        {
            UdpClient client = new UdpClient(port);
            IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                try
                {
                    // Bytes empfangen.
                    byte[] data = client.Receive(ref anyIP);

                    // Bytes weiterleiten und konvertieren
                    PointCloudReader.ConvertBytesToPoint(data);
                }
                catch (Exception err)
                {
                    Console.WriteLine(err.ToString());
                }
            }
        }
    }
}