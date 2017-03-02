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

//using Tutorial: https://www.youtube.com/watch?v=-mYoJBT9XIg&list=PLAC179D21AF94D28F&index=6
namespace Server
{
    class Serverclient //for sending data
    {
        private static List<byte[]> sendingPackages = new List<byte[]>();
        private static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static byte[] file;

        static void Main(string[] args)
        {         
            string path = @"C:/Users/Tanja Langer/Documents/Studium/Forschungsprojekt/Forschungsprojekt_WS16-17/TCPSimpleTest\TestPoints.txt";
            //string path = @"L:/Programme/Gitkraken/Forschungsprojekt_WS16-17/TCPSimpleTest/TestPoints.txt";
            file = ReadfromFile(path);

            //ENTER IP of PC or Android Device //TODO: Receive ip from device 
            Console.WriteLine("Enter ip Adress of remote device: ");
            string ipinput = Console.ReadLine();
            Console.WriteLine("When Client is ready enter c to connect");
            string userinput = Console.ReadLine();
            if (userinput == "c")
            {
                connect(ipinput);
            }

            Console.WriteLine("Enter s to send data");
            string input = Console.ReadLine();
            if (input == "s")
            {
                sendData();
            }

            socket.Close();
            Console.Read();//Console waits for input, so window doesn't close immediately
        }

        public static void sendData()
        {
            Console.Write("Sending... ");
            try {
                socket.Send(BitConverter.GetBytes(file.Length), 0, 4, 0);
                socket.Send(file);
                Console.WriteLine("File sent successfully!");
                
            }
            catch
            {
                Console.WriteLine("Sending packets failed");
                Console.WriteLine("Enter ip Adress of remote device: ");
                string ipinput = Console.ReadLine();
                Console.WriteLine("When Client is ready enter c to connect");
                string userinput = Console.ReadLine();
                if (userinput == "c")
                {
                    connect(ipinput);
                }
                Console.WriteLine("Enter s to send data again");
                string input = Console.ReadLine();
                if (input == "s")
                {
                    sendData();
                }
              
            }

           
        }

        public static void connect(string ip)
        {
            //try to connect to remote application and send data
            try
            {
                //TODO: Send IP Address from client first
                socket.Connect(new IPEndPoint(IPAddress.Parse(ip), 1994));// connects to everything on Port 1994
                Console.WriteLine("Connected");
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
