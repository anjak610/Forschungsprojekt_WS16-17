using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Fusee.Forschungsprojekt.Android
{
    public class ReceiverClient
    {
        private Socket socket;
        private Socket receiver;
        private Socket acceptor;
        public Boolean receiving;
        private Boolean connected;
        public Boolean completed;
        private byte[] data;
     

        public ReceiverClient()
        {
            receiver = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            receiving = false;
            connected = false;
            completed = false;
        }

        public void ConnecttoServer(string serveraddress, string localip)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(serveraddress), 1234);
            socket.Connect(endPoint);
            System.Diagnostics.Debug.WriteLine("Connected to Server");
            byte[] ipbuffer = Encoding.Default.GetBytes(localip);
            socket.Send(ipbuffer, 0, ipbuffer.Length, 0);
            System.Diagnostics.Debug.WriteLine("Sent local IP: " + localip);
            connected = true;

        }

        public void SetupReceiving()
        {
            try
            {
                receiver.Bind(new IPEndPoint(0, 1994));
                receiver.Listen(0);
                acceptor = receiver.Accept();
                System.Diagnostics.Debug.WriteLine("Socket accepting");
                receiving = true;
                completed = false;
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Could not setup listening socket");
            }

        }

        public void Receive() {
            try
            {
                while (connected && receiving)
                {
                    byte[] sizebuffer = new byte[4];
                    acceptor.Receive(sizebuffer, 0, sizebuffer.Length, 0);
                    //receive length of data                 
                    int size = BitConverter.ToInt32(sizebuffer, 0);
                    MemoryStream ms = new MemoryStream();//will hold the data that is received

                    while (size > 0)
                    {
                        System.Diagnostics.Debug.WriteLine("Buffersize > 0");
                        System.Diagnostics.Debug.WriteLine("Receiving...");

                        byte[] buffer;
                        if (size < acceptor.ReceiveBufferSize)
                        {
                            buffer = new byte[size];
                        }
                        else
                        {
                            buffer = new byte[acceptor.ReceiveBufferSize];
                        }

                        int receive = acceptor.Receive(buffer, 0, buffer.Length, 0);
                        //subtract the size of the received data from the size
                        size -= receive;
                        //write the received data to the memory stream
                        ms.Write(buffer, 0, buffer.Length);

                    }

                    if (size == 0)
                    {
                        receiving = false;
                        ms.Close();
                        data = ms.ToArray();
                        Core.PointCloudReader.receivedData = Encoding.UTF8.GetString(data);
                        Core.PointCloudReader.DisplayReceived();
                        ms.Dispose();
                        System.Diagnostics.Debug.WriteLine("Everything received");
                        completed = true;
                    }
                }
            }
            catch (Exception exp) //catch socket exceptions
            {
                System.Diagnostics.Debug.WriteLine("Exceptions catched:\n" + exp);
            }
        }

            public void Disconect()
            {
            connected = false;
            receiver.Close();
            acceptor.Close();
            socket.Close();
            }
        
    }
}
