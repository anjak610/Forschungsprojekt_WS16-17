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
using System.Globalization;
using System.Runtime.InteropServices;
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

        public void SendPackage(byte[] package)
        {                   
                        
          SendData(package);

            if (package.Length == 0)//handle last package
            {
                float endMessage = 0xFEEDBEEF;
                byte[] buffer = BitConverter.GetBytes(endMessage);
                SendData(buffer);
                System.Diagnostics.Debug.WriteLine("Sent END OF FILE Message");
                Console.WriteLine("All packages submitted");
            }


        }

        public byte[] ReadBinaryFile(string filepath)
        {
            byte[] fileData = File.ReadAllBytes(filepath);
            return fileData;
        }
                

        public void SendData(byte[] file)
        {
            Console.Write("Sending... ");
            try
            {
                byte[] buffer = BitConverter.GetBytes(file.Length);
                sender.Send(buffer, 0, buffer.Length , 0);//send length of data first              
                Console.WriteLine("Filesize sent: "+ file.Length);
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
                sender.Connect(new IPEndPoint(IPAddress.Parse(address), 50123));// connects to everything on Port 50123
                Console.WriteLine("Connected to " + address);
            }
            catch
            {
                Console.WriteLine("Unable to connect");

            }
        }
        /// <summary>
        /// Reads a text file and prepares string packages with length of 500 lines(500 points)
        /// </summary>
        /// <param name="filepath">absolute location of file</param>
        /// <returns></returns>
       
        public List<byte[]> SplitandSendPackages(string filepath)
        {
            List<byte[]> packages = new List<byte[]>();
            new Thread(() => //thread for receiving data
            {               
                using (StreamReader s = new StreamReader(filepath))
                {
                    string line; //each line represents one point

                    int count = 0;
                    int packagesize = 500;
                    int linelimit = packagesize;
                    string packagedata = "";
                    
                    while (((line = s.ReadLine()) != null) && (count <= linelimit)) // read per line
                    {
                        if (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Spacebar))
                        {
                            packagedata += line + "\n"; //adds new line character after each point

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
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Sending stopped ");
                            Console.WriteLine("Sending stopped");
                            Console.WriteLine("Press any key to exit");                        
                            Console.Read();
                            break;                           
                        }
                    }                   
                    
                }
                
            }).Start();
            return packages;
        }

        /// <summary>
        /// Reads a text file and prepares dummy packages with length of 500 lines(500 points) for testing
        /// </summary>
        /// <param name="filepath">absolute location of file</param>
        /// <returns></returns>

        public List<byte[]> CreateProtocolPackages(string filepath)
        {
            List<byte[]> packages = new List<byte[]>();
            new Thread(() => //thread for receiving data
            {
                using (StreamReader s = new StreamReader(filepath))
                {
                    string line; //each line represents one point

                    int count = 0;
                    int maxpoints = 500;
                    int linelimit = maxpoints;
                    Random rnd = new Random();

                    var mstream = new MemoryStream();
                    var writer = new BinaryWriter(mstream);

                    UInt32 packetBeginMarker = 0xFEEDBEEF;
                    UInt16 typeID = 0x1010;
                    UInt16 version = 0x001;
                    UInt32 packetSize = 2040;                 
                    DateTime utctime = DateTime.UtcNow;
                    double time = utctime.ToOADate(); //dont know if this is the right calculation!?
                    //using dummy values! 
                    //position of scanner for this packet
                    float drone_posX = 120; 
                    float drone_posY = 140;
                    float drone_posZ = 200;
                   //Orientation
                    float quaternionW = 1.0f;
                    float quaternionX = 1.5f;
                    float quaternionY = 2.5f;
                    float quaternionZ = 2.0f;
                    //number of points
                    UInt32 numberOfPoints = 500;

                    while (((line = s.ReadLine()) != null) && (count <= linelimit)) // read per line
                    {
                        if (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Spacebar))
                        {
                            //add information at the beginning of each packet
                            if (count == 0 || (count % 500 == 0))
                            {
                                Console.WriteLine("Preparing next package...");
                                writer.Write(BitConverter.GetBytes(packetBeginMarker));
                                writer.Write(BitConverter.GetBytes(typeID));
                                writer.Write(BitConverter.GetBytes(version));
                                writer.Write(BitConverter.GetBytes(packetSize));
                                writer.Write(BitConverter.GetBytes(time));
                                writer.Write(BitConverter.GetBytes(drone_posX));
                                writer.Write(BitConverter.GetBytes(drone_posY));
                                writer.Write(BitConverter.GetBytes(drone_posZ));
                                writer.Write(BitConverter.GetBytes(quaternionW));
                                writer.Write(BitConverter.GetBytes(quaternionX));
                                writer.Write(BitConverter.GetBytes(quaternionY));
                                writer.Write(BitConverter.GetBytes(quaternionZ));
                                writer.Write(BitConverter.GetBytes(numberOfPoints));
                            }

                            //create EchoId
                            int EchoId = rnd.Next(0, 3);

                            float[] pointArray = new float[3];

                            string separator = "\t";

                            string[] coordinates = line.Split(separator.ToCharArray()); //adds new line character after each point
                          
                            pointArray[0] = float.Parse(coordinates[0], CultureInfo.InvariantCulture.NumberFormat);
                            pointArray[1] = float.Parse(coordinates[1], CultureInfo.InvariantCulture.NumberFormat);
                            pointArray[2] = float.Parse(coordinates[1], CultureInfo.InvariantCulture.NumberFormat);

                            int dst = rnd.Next(10, 150);
                            
                            //write point data to memory stream                            
                             writer.Write(EchoId);
                             writer.Write(pointArray[0]);
                             writer.Write(pointArray[1]);
                             writer.Write(pointArray[2]);
                             writer.Write(dst);

                            //count up to limit
                            count++;

                            //add information at the end of every packet
                            if (count == linelimit)
                            {                              
                                linelimit = count + maxpoints; //iterate through the next package
                                writer.Write(BitConverter.GetBytes(packetBeginMarker));
                                Console.WriteLine("UAV Package created.");
                                byte[] b = mstream.ToArray();
                                SendPackage(b);
                                mstream.SetLength(0);//empty memory stream
                            }

                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Sending stopped ");
                            Console.WriteLine("Sending stopped");
                            Console.WriteLine("Press any key to exit");
                            Console.Read();
                            break;
                        }
                    }

                }

            }).Start();
            return packages;
        }

        /// <SUMMARY>
        /// Tells you the IP Address of the remote host that you are sending files to.
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
