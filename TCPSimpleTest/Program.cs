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

namespace Server
{
    class Serverclient //for sending data
    {
        private static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public static IPEndPoint createEndPoint()
        {
                //if there's no socket and it is not connected then get IPEndPoint
                if (socket != null && socket.Connected)
                {
                    return (IPEndPoint)socket.RemoteEndPoint;

                }else { return new IPEndPoint(0, 1994); }
               
            }

        static void Main(string[] args)
        {

            string path = @"C:/Users/Tanja Langer/Documents/Studium/Forschungsprojekt/Forschungsprojekt_WS16-17/TCPSimpleTest\TestPoints.txt";
            //string path = @"L:/Programme/Gitkraken/Forschungsprojekt_WS16-17/TCPSimpleTest/TestPoints.txt";
            byte[] file = ReadfromFile(path);         
            List<byte[]> sendingPackages = Split(file);

            try {
                IPEndPoint end = createEndPoint();                 
                socket.Bind(end);// connects to everything
                socket.Listen(0);
            
                Socket acceptor = socket.Accept();

            //receive connection message
            byte[] buffer = new byte[1024];
            int receiver = acceptor.Receive(buffer, 0, buffer.Length, 0);
            Array.Resize(ref buffer, receiver);
            Console.WriteLine("Received: {0}", Encoding.Default.GetString(buffer));


            //send message to client //TODO: send point data in pieces
            Console.Write("Sending... ");
            foreach (byte[] item in sendingPackages)
            {
                    acceptor.Send(item, 0, item.Length, SocketFlags.None);//send byte array
             }
            
            //string msg = Console.ReadLine();
            //byte[] sendbuffer = Encoding.Default.GetBytes(msg);
            // byte[] sendbuffer = ReadfromFile("TestPoints.txt");
            //acceptor.Send(sendbuffer, 0, sendbuffer.Length, SocketFlags.None);//send byte array
            //Console.Write("Sending file...");
            socket.Close();
            acceptor.Close();
            }

            catch
            {
                Console.WriteLine("Sending failed");
            }

            Console.Read();//Console waits for input, so window doesn't close immediately
        }

        public static byte[] ReadfromFile(string path)
        {
            Console.WriteLine("Reading...");
            byte[] b = File.ReadAllBytes(path);

            return b;
        }

        public static List<byte[]> Split(byte[] filebytes)
        {
            List<byte[]> packages = new List<byte[]>();

            for  ( int i=0; i< filebytes.Length; i++)
            {
                IEnumerable<byte> b = filebytes.Cast<byte>();
                IEnumerable<byte> chunk1024 = b.Take<byte>(1024);
                IEnumerable<byte> leftovers = b.Skip<byte>(1024);
                b = leftovers;

                packages.Add(chunk1024.ToArray<byte>());
                filebytes = b.ToArray<byte>();
                Console.WriteLine(filebytes.Length);
               
            }

            return packages;
           // TODO: Handle leftovers of byte array
        }
                 
    }
}
