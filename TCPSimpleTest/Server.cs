using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.IO;
using System.Globalization;

//192.168.0.211
//using Tutorial: https://www.youtube.com/watch?v=-mYoJBT9XIg&list=PLAC179D21AF94D28F&index=6
namespace Server
{
    class Serverclient //for sending data
    {
        private static List<byte[]> sendingPackages = new List<byte[]>();
        private static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static byte[] file;
        private static string fuseeIP = null;

        static void Main(string[] args)
        {         
            string path = @"C:/Users/Tanja Langer/Documents/Studium/Forschungsprojekt/Forschungsprojekt_WS16-17/TCPSimpleTest\TestPoints.txt";
            //string path = @"L:/Programme/Gitkraken/Forschungsprojekt_WS16-17/TCPSimpleTest/TestPoints.txt";
            file = ReadfromFile(path);           

            ConsoleKeyInfo pressed;
            listen();
            receiveIP();
       
            Console.WriteLine("Press <enter> to connect to this IP");
            pressed = Console.ReadKey();
            if (pressed.Key == ConsoleKey.Enter)
            {
                if (fuseeIP != null) {
                    connect(fuseeIP);
                }
                else { Console.WriteLine("No IP to connect to"); }
               
            }

            Console.WriteLine("Press <enter> to send data");
            pressed = Console.ReadKey();
            if (pressed.Key == ConsoleKey.Enter)
            {
                sendData();
            }            

            Console.Read();//Console waits for input, so window doesn't close immediately
        }

        public static void sendData()
        {
            Console.Write("Sending... ");
            try {
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

        public static void listen()
        {       
            IPEndPoint endPoint = new IPEndPoint(0, 1234);
            socket.Bind(endPoint);
            socket.Listen(0);
            Console.WriteLine("Waiting for requests...");
        }

        public static void receiveIP()
        {
            //Get IP Address from client first
            
            try {
                Socket acc = socket.Accept();
                byte[] receivebuffer = new byte[1024];
                int rec = acc.Receive(receivebuffer, 0, receivebuffer.Length, SocketFlags.None);
                Array.Resize(ref receivebuffer, rec);
                fuseeIP = Encoding.Default.GetString(receivebuffer);
                socket.Close();
                acc.Close();
                Console.WriteLine("Received request from IP: " + fuseeIP);
                
            }
            catch
            {
                Console.WriteLine("Could not receive IP");
                
            }
        }

        public static void connect(string address)
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

        public static byte[] ReadfromFile(string path)
        {
            
            byte[] b = File.ReadAllBytes(path);
            Console.WriteLine("File converted to byte stream");
            return b;
        }

                 
    }
}
