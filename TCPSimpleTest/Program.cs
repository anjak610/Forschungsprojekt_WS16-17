using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace Server
{
    class Program //for sending data
    {
        static void Main(string[] args)
        {
            //multithreaded example
            //Socket listener;
            //Socket connecter;
            //Socket acceptor;
            //
            //listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //connecter = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //
            //listener.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1994));//local machine address, port
            //listener.Listen(0);
            //
            //new Thread(() =>//thread blocks until connection is made
            //{
            //    acceptor = listener.Accept();
            //    Console.WriteLine("Connected");
            //
            //}).Start();
            //
            //connecter.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1994)); //remote machine address, port 

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            socket.Bind(new IPEndPoint(0, 1994));// connects to everything
            socket.Listen(0);

            Socket acceptor = socket.Accept();

            //receive connection message
            byte[] buffer = new byte[255];
            int receiver = acceptor.Receive(buffer, 0, buffer.Length, 0);
            Array.Resize(ref buffer, receiver);
            Console.WriteLine("Received: {0}", Encoding.Default.GetString(buffer));

            //send message to client //TODO: send point data in pieces
            Console.Write("Enter your message: ");
            string msg = Console.ReadLine();           
            byte[] sendbuffer = Encoding.Default.GetBytes(msg);
            acceptor.Send(sendbuffer, 0, sendbuffer.Length, SocketFlags.None);//send byte array
            
            socket.Close();
            acceptor.Close();

            Console.Read();//Console waits for input, so window doesn't close immediately
        }
    }
}
