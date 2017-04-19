using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Threading;

namespace Server
{
    public class TCPServer
    {
        private Socket listener;
        private Socket sender;
        private Socket acceptor;
        private string msg = "0";

        private int count = 0;

        private int maxConnections { get; set; }

        public string FuseeIp { get; set; }


        public TCPServer()
        {
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

           
            FuseeIp = null;         
            maxConnections = 10;
        
        }

        public string SendPackage(byte[] package)
        {                   
                if (msg == "EOP" || msg == "0") {                
                    SendData(package);
                    Console.Write("Sending package");                 
                    msg = ListenFeedback();                 
                }

            if (package.Length == 0)//handle last package
            {
                string endMessage = "END";
                byte[] buffer = Encoding.Default.GetBytes(endMessage);
                SendData(buffer);
                System.Diagnostics.Debug.WriteLine("Sent END OF FILE Message");
                Console.WriteLine("All packages submitted");
            }
         
            return msg;                                      
        }
           
                

        public void SendData(byte[] file)
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
            Console.WriteLine("\n Received feedback: " + message);

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
       

        public List<byte[]> SplitandSendPackages(string filepath)
        {
            List<byte[]> packages = new List<byte[]>();
            new Thread(() => //thread for receiving data
            {               
                using (StreamReader s = new StreamReader(filepath))
                {
                    string line; //each line represents one point

                    int count = 0;
                    int packagesize = 20000;
                    int linelimit = packagesize;
                    string packagedata = "";
                    while (((line = s.ReadLine()) != null) && (count <= linelimit)) // read per line
                    {
                        packagedata += line + "\n";//adds new line character after each point
                       
                        if (count == 0)
                        {
                            Console.WriteLine("Preparing packages...");
                        }
                        count++;

                        if (count == linelimit)
                        {
                            linelimit = count + packagesize; //iterate through the next package

                            byte[] b = Encoding.UTF8.GetBytes(packagedata);
                            packages.Add(b);
                            Console.WriteLine("Package created. Linecount: " + count);
                            SendPackage(b);
                                               
                            packagedata = "";                                                     
                        }
                      

                    }
                }
                
            }).Start();
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
