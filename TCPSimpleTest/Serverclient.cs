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
using Microsoft.Win32;

//using Tutorial: https://www.youtube.com/watch?v=-mYoJBT9XIg&list=PLAC179D21AF94D28F&index=6

namespace Server
{
    public class Serverclient //for sending data
    {

        

        private static List<byte[]> packages = new List<byte[]>();

        private static string path =
            @"C:/Users/Tanja Langer/Documents/Studium/Forschungsprojekt/Forschungsprojekt_WS16-17/TCPSimpleTest/PointCloud_IPM.txt";


       // private static string binarydatapath =
       //    @"C:\Users\Tanja Langer\Documents\Studium\Forschungsprojekt\Forschungsprojekt_WS16-17\TCPSimpleTest\TestPacket.uav_live_stream";

        private static string binarydatapath =
         @"L:\Programme\Gitkraken\Forschungsprojekt_WS16-17\Core\Assets\TestPacket.uav_live_stream";


        //private static string path = @"L:/Programme/Gitkraken/Forschungsprojekt_WS16-17/TCPSimpleTest/TestPoints.txt";
        //
        private static TCPServer server = new TCPServer();




        static void Main(string[] args) //Method that starts the reading/listening
        {

            server.Start();
            server.acceptConnectionRequests();

            Console.WriteLine("Press <enter> to connect to this IP");
            var pressed = Console.ReadKey();
            if (pressed.Key == ConsoleKey.Enter)
            {
                if (server.FuseeIp != null)
                {
                    server.Connect(server.FuseeIp);
                }
                else
                {
                    Console.WriteLine("No IP to connect to");
                }

            }

            Console.WriteLine("Press <enter> to send data");
            pressed = Console.ReadKey();

            if (pressed.Key == ConsoleKey.Enter)
            {
                try
                {
                     byte[] binaryfile = server.ReadBinaryFile(binarydatapath);
                     server.SendData(binaryfile); 

                     //packages = server.SplitandSendPackages(path); //split and send packages, store them  

                }
                catch (Exception exception)
                {
                    Console.WriteLine("Error: " + exception);
                }

            }
            Console.Read();

            //Console.WriteLine("Press <enter> to send data");
            //Console.WriteLine("Press <space> to abort sending");
            //pressed = Console.ReadKey();
            //if (pressed.Key == ConsoleKey.Enter)
            //{
            //    try
            //    {
            //        packages = server.SplitandSendPackages(path); //split and send packages, store them  

            //    }
            //    catch (Exception exception)
            //    {
            //        Console.WriteLine("Error: " + exception);
            //    }

            //}



        }
    }
}
