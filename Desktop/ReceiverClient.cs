using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Fusee.Forschungsprojekt.Core;

namespace Fusee.Forschungsprojekt.Desktop
{
    public class ReceiverClient
    {
        private Socket sender;
        private Socket receiver;
        private Socket acceptor;
        public bool receiving;
        private bool connected;
        public bool completed;
        private byte[] data;
        public string serveraddress;

        public void SendConfirmation()
        {
            byte[] buffer = Encoding.Default.GetBytes("EOP");//End of package
            sender.Send(buffer, 0, buffer.Length, 0);
            System.Diagnostics.Debug.WriteLine("\nSend confirmation: EOP");
            receiving = true;
            Receive();
        }

        public ReceiverClient()
        {
            receiver = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            receiving = false;
            connected = false;
            completed = false;
            
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

        public void SetupReceiving()//Receive on port 1994
        {
            try
            {
                receiver.Bind(new IPEndPoint(0, 1994));
                receiver.Listen(0);
                acceptor = receiver.Accept();
                //System.Diagnostics.Debug.WriteLine("Socket accepting");
                receiving = true;
                completed = false;
                Core.PointVisualizationBase._pointCloud.OnDataDisplayedEvent += SendConfirmation;
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
                    System.Diagnostics.Debug.WriteLine("Socket accepting");
                    byte[] sizebuffer = new byte[4];
                    acceptor.Receive(sizebuffer, 0, sizebuffer.Length, 0);
                    //receive length of data                 
                    int size = BitConverter.ToInt32(sizebuffer, 0);
                    MemoryStream ms = new MemoryStream();//will hold the data that is received

                    while (size > 0)
                    {
                        byte[] buffer;
                        buffer = size < acceptor.ReceiveBufferSize ? new byte[size] : new byte[acceptor.ReceiveBufferSize];

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
                        string datastring = Encoding.UTF8.GetString(data);
                        Core.PointCloudReader.receivedData = datastring;
                        Core.PointCloudReader.DisplayReceived();
                        ms.Dispose();
                       

                        if (datastring == "END")
                        {
                            System.Diagnostics.Debug.WriteLine("Everything received");
                            Disconect();
                            System.Diagnostics.Debug.WriteLine("Disconnected");
                        }
                        else
                        {
                           
                            
                        }
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

      

        public void SendFailure()
        {
            byte[] buffer = Encoding.Default.GetBytes("FAILED");//End of package
            sender.Send(buffer, 0, buffer.Length, 0);
            System.Diagnostics.Debug.WriteLine("send confirmation: FAILED");
            receiving = true;
            Receive();

        }       

    }
}
