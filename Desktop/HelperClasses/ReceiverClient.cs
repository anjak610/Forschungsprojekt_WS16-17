using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Fusee.Tutorial.Core;

namespace Fusee.Tutorial.Desktop.HelperClasses
{
    public class ReceiverClient
    {
        private Socket sender;
        private Socket receiver;
        private Socket acceptor;
        public bool receiving;
        private bool connected;
        private byte[] data;
        public string serveraddress;

   

        public ReceiverClient()
        {
            receiver = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            receiving = false;
            connected = false;

            
        }

        public void ConnecttoServer(string serveraddress, string localip)//send on port 1234
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(serveraddress), 1234);
            sender.Connect(endPoint);
            System.Diagnostics.Debug.WriteLine("Connected to Server");
            byte[] ipbuffer = Encoding.Default.GetBytes(localip);
            sender.Send(ipbuffer, 0, ipbuffer.Length, 0);
            System.Diagnostics.Debug.WriteLine("Sent local IP: " + localip);
            connected = true;

        }

        public void SetupReceiving()//Receive on port 50123
        {
            try
            {
                receiver.Bind(new IPEndPoint(0, 50123));
                receiver.Listen(0);
                acceptor = receiver.Accept();
                receiving = true;

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
                    System.Diagnostics.Debug.WriteLine("Client Socket accepting");
                    byte[] sizebuffer = new byte[4];
                    acceptor.Receive(sizebuffer, 0, sizebuffer.Length, 0);
                    //receive length of data                 
                    int size = BitConverter.ToInt32(sizebuffer, 0);
                    MemoryStream ms = new MemoryStream();//will hold the data that is received

                    while (size > 0)
                    {
                        var buffer = size < acceptor.ReceiveBufferSize ? new byte[size] : new byte[acceptor.ReceiveBufferSize];

                        int receive = acceptor.Receive(buffer, 0, buffer.Length, 0);
                        //subtract the size of the received data from the size
                        size -= receive;
                        //write the received data to the memory stream
                        ms.Write(buffer, 0, buffer.Length);
                    }

                    if (size == 0)
                    {

                        ms.Close();
                        data = ms.ToArray();
                        //TODO convert to float not to string
                        string datastring = Encoding.UTF8.GetString(data);
                        Core.PointClouds.PointCloudReader.ReadFromString(datastring);
                        ms.Dispose();

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
        receiving = false;
        receiver.Close();
        acceptor.Close();
        sender.Close();
        }
   

    }
}
