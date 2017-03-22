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
        private Socket acceptor;

        private int maxConnections { get; set; }

        public string FuseeIp { get; set; }


        public TCPServer()
        {
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

           
            FuseeIp = null;         
            maxConnections = 10;
        
        }

        public void SendPackages(List<byte[]> packageList)
        {
            for(int i = 0; i < packageList.Count; i++)
            {
                string msg = " ";
                if (msg == "EOP")
                {
                    sendData(packageList[i + 1]);
                    Console.Write("Sending package " + (i+1));
                    msg = ListenFeedback();
                }
                else
                {
                    sendData(packageList[i]);
                    Console.Write("Sending package " + i + "\n");
                    msg = ListenFeedback();
                }

                if (i == (packageList.Count - 1))
                {
                    string endMessage = "END";
                    byte [] buffer = Encoding.Default.GetBytes(endMessage);
                    sender.Send(buffer, 0, buffer.Length, 0);
                    System.Diagnostics.Debug.WriteLine("Sent END OF FILE Message");
                }
                
            }
        }

        public void sendData(byte[] file)
        {
            Console.Write("Sending... ");
            try
            {
                sender.Send(BitConverter.GetBytes(file.Length), 0, 4, 0);
                sender.Send(file);
                Console.WriteLine("File sent successfully!");
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
                acceptor = listener.Accept();
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string ListenFeedback()
        {

            byte[] receivebuffer = new byte[1024];         
            int rec = acceptor.Receive(receivebuffer, 0, receivebuffer.Length, SocketFlags.None);
            Array.Resize(ref receivebuffer, rec);
            string message = Encoding.Default.GetString(receivebuffer);
            Console.WriteLine("Received feedback: " + message);

            if (message == "EOP")
            {
                Console.WriteLine("Package transfer successful");
            }

            return message;
        }


        /// <summary>
        /// Receives IP address of request
        /// </summary>

        public bool acceptConnectionRequests()
        {
            //Get IP Address from client first

            try
            {

                byte[] receivebuffer = new byte[1024];
                int rec = acceptor.Receive(receivebuffer, 0, receivebuffer.Length, SocketFlags.None);
                Array.Resize(ref receivebuffer, rec);
                FuseeIp = Encoding.Default.GetString(receivebuffer);
                Console.WriteLine("Received request from IP: " + FuseeIp);
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

        /// <summary>
        /// Splits big file into chunks of 1048576 byte
        /// </summary>

        public List<byte[]> Split(byte[] filebytes)
        {

            List<byte[]> packages = new List<byte[]>();
            Console.WriteLine("Splitting file into packages");
            for (int i = 0; i < filebytes.Length; i++)

            {

                IEnumerable<byte> b = filebytes.Cast<byte>();

                IEnumerable<byte> chunk1024 = b.Take<byte>(1048576);//one MB packages

                IEnumerable<byte> leftovers = b.Skip<byte>(1048576);

                b = leftovers;



                packages.Add(chunk1024.ToArray<byte>());

                filebytes = b.ToArray<byte>();

                Console.WriteLine(filebytes.Length);



            }

            return packages;

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
                listener.Close();
            }
        }
                
    }
}
