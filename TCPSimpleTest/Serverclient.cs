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
    public class Serverclient //for sending data
    {
       
     //  private static byte[] file;
     //  private static string feedback;
      private static List<byte[]> packages = new List<byte[]>();

      private static string path = @"C:/Users/Tanja Langer/Documents/Studium/Forschungsprojekt/Forschungsprojekt_WS16-17/TCPSimpleTest/PointCloud_IPM.txt";
     //  //private static string path = @"L:/Programme/Gitkraken/Forschungsprojekt_WS16-17/TCPSimpleTest/TestPoints.txt";
     //
      private static TCPServer server = new TCPServer();
     //


        
      static void Main(string[] args) //Method that starts the reading/listening
      {                   
     //      file = server.ReadfromFile(path);
     //      packages = server.Split(file);       

          //packages = server.SplitPointPackages(path);


          server.Start();
          server.acceptConnectionRequests();
     
          Console.WriteLine("Press <enter> to connect to this IP");
          var pressed = Console.ReadKey();
          if (pressed.Key == ConsoleKey.Enter)
          {
              if (server.FuseeIp != null) {
                  server.Connect(server.FuseeIp);
              }
              else { Console.WriteLine("No IP to connect to"); }
             
          }
     
          Console.WriteLine("Press <enter> to send data");
          pressed = Console.ReadKey();
          if (pressed.Key == ConsoleKey.Enter)
          {
              try
              {
                    //server.SendPackages(packages);
                    packages = server.SplitPointPackages(path);//split and send packages
                }
              catch(Exception exception)
              {
                  Console.WriteLine("Error: " + exception);
              }
          }
     
     
          Console.WriteLine("Press <enter> to stop server");
          pressed = Console.ReadKey();
          if (pressed.Key == ConsoleKey.Enter)
          {
     
              server.Stop();
              Console.WriteLine("Server stopped");
          }

            Console.Read();//Console waits for input, so window doesn't close immediately
            }
                 
    }
}
