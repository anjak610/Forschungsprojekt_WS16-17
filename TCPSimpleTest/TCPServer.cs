using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections;
using System.Threading;

namespace Server
{
    public class TCPServer
    {
        private Socket listener;
        private Socket sender;


        private int maxConnections { get; set; }

        public string fuseeIP { get; set; }


        public TCPServer()
        {
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);         
            fuseeIP = null;         
            maxConnections = 10;
        
        }

        public void sendData(byte[] file)
        {
            Console.Write("Sending... ");
            try
            {
                sender.Send(BitConverter.GetBytes(file.Length), 0, 4, 0);
                sender.Send(file);
                Console.WriteLine("File sent successfully!");
                sender.Close();
            }
            catch
            {
                Console.WriteLine("Sending packets failed");

            }
        }

        /// <summary>
        /// Start Accepting requests
        /// </summary>
        public bool Start()
        {
            try {
                IPEndPoint endPoint = new IPEndPoint(0, 1234);
                listener.Bind(endPoint);
                listener.Listen(0);
                Console.WriteLine("Waiting for requests...");
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Receives IP address of request
        /// </summary>

        public bool acceptConnectionRequests()
        {
            //Get IP Address from client first

            try
            {
                Socket acc = listener.Accept();
                byte[] receivebuffer = new byte[1024];
                int rec = acc.Receive(receivebuffer, 0, receivebuffer.Length, SocketFlags.None);
                Array.Resize(ref receivebuffer, rec);
                fuseeIP = Encoding.Default.GetString(receivebuffer);
                listener.Close();
                acc.Close();
                Console.WriteLine("Received request from IP: " + fuseeIP);
                return true;

            }
            catch
            {
                Console.WriteLine("Could not receive IP");
                return false;

            }
        }

        /// <summary>
        /// Connects to a given ip address to send data
        /// </summary>

        public void Connect(string address)
        {
            //try to connect to remote application and send data
            try
            {
                sender.Connect(new IPEndPoint(IPAddress.Parse(address), 1994));// connects to everything on Port 1994
                Console.WriteLine("Connected to " + address);
            }
            catch
            {
                Console.WriteLine("Unable to connect");

            }
        }

        /// <summary>
        /// Converts text file to byte array 
        /// </summary>
        /// 
        public byte[] ReadfromFile(string filepath)
        {
            byte[] b = File.ReadAllBytes(filepath);
            Console.WriteLine("File converted to byte stream");
            return b;
        }

        /// <SUMMARY>
        /// Tells you the IP Address of the remote host that you are sending files to.
        /// should be identical with fuseeIP ;)
        /// </SUMMARY>
        public IPEndPoint RemoteEndPoint
        {
            get
            {
                if (sender != null && sender.Connected)
                {
                    return (IPEndPoint)sender.RemoteEndPoint;
                }
                return new IPEndPoint(IPAddress.None, 0);
            }
        }

        /// <SUMMARY>
        /// Shutsdown the server
        /// </SUMMARY>
        public void Stop()
        {
            lock (this)
            {

                sender.Close();
            }
        }

        
    }
}
