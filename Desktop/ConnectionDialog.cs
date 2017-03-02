using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

//using tutorial: https://www.youtube.com/watch?v=-mYoJBT9XIg&list=PLAC179D21AF94D28F&index=6
namespace Fusee.Forschungsprojekt.Desktop
{
    public partial class ConnectionDialog : Form
    {

        Socket socket;
        Socket acceptor;
        Boolean connected = false;

        public ConnectionDialog()
        {
            InitializeComponent();

        }

        //connect button event handler
        private void connectButton_Click_1(object sender, EventArgs e)
        {
            
            new Thread(() => //thread for receiving data
           {
               //Setup socket
               socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
               IPEndPoint endPoint = new IPEndPoint(0, 1994);
               socket.Bind(endPoint);
               System.Diagnostics.Debug.WriteLine("Waiting for Server");

               Invoke((MethodInvoker)delegate
               {
                   statusText.Text += ("\nWaiting for Server...");
               });

               socket.Listen(0);
               acceptor = socket.Accept();
               System.Diagnostics.Debug.WriteLine("Connection ready");
               connected = true;
               Invoke((MethodInvoker)delegate
               {
                   statusText.Text += ("\nConnected!");
               });

               while (connected)
               {
                   byte[] sizebuffer = new byte[4];
                   acceptor.Receive(sizebuffer, 0, sizebuffer.Length, 0);
                   //receive length of data                 
                   int size = BitConverter.ToInt32(sizebuffer, 0);
                   MemoryStream ms = new MemoryStream();//will hold the data that is received
                                        
                   while (size > 0)
                   {
                       System.Diagnostics.Debug.WriteLine("Inside loop in Connection Dialog");
                       System.Diagnostics.Debug.WriteLine("Receiving..");
                       byte[] buffer;
                       if (size < acceptor.ReceiveBufferSize)
                       {
                           buffer = new byte[size];
                       }
                       else
                       {
                           buffer = new byte[acceptor.ReceiveBufferSize];
                       }

                       int receive = acceptor.Receive(buffer, 0, buffer.Length, 0);
                       //subtract the size of the received data from the size
                       size -= receive;
                       //write the received data to the memory stream
                       ms.Write(buffer, 0, buffer.Length);
                                            
                   }

                   if (size == 0)
                   {
                       connected = false;
                       ms.Close();
                       byte[] data = ms.ToArray();

                       ms.Dispose();
                       System.Diagnostics.Debug.WriteLine("Everything received");

                       Invoke((MethodInvoker)delegate
                       {
                           receivedDataText.Text = Encoding.UTF8.GetString(data);
                       });

                   }
               }
           }).Start();
        
        }

        private void disconButton_Click(object sender, EventArgs e)
        {
            Invoke((MethodInvoker)delegate
            {
                statusText.Text += ("\nConnection closed");
            });
            connected = false;
            acceptor.Close();
            socket.Close();

        }
    }
}
