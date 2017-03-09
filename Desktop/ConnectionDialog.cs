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
        Socket receiver;
        Socket acceptor;
        Boolean connected = false;

        public ConnectionDialog()
        {
            InitializeComponent();
            receiver = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        }

        //connect button event handler
        private void connectButton_Click_1(object sender, EventArgs e)
        {

            new Thread(() => //thread for receiving data
           {
           //Send own IP to listening server            
           string serverip = IPinputBox.Text;

           IPAddress inputaddress;
               if (IPAddress.TryParse(serverip, out inputaddress))
               {
                   if (inputaddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                   {
                       try
                       {
                           IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(serverip), 1234);
                           socket.Connect(endPoint);
                           System.Diagnostics.Debug.WriteLine("Connected to Server");
                           string localip = getIPv4();
                           byte[] ipbuffer = Encoding.Default.GetBytes(localip);
                           socket.Send(ipbuffer, 0, ipbuffer.Length, 0);
                           System.Diagnostics.Debug.WriteLine("Sent local IP: " + localip);

                           Invoke((MethodInvoker)delegate
                           {
                               statusText.Text += ("\nSent local IP: " + localip);
                           });

                           //receiving data
                           receiver.Bind(new IPEndPoint(0, 1994));
                           receiver.Listen(0);
                           acceptor = receiver.Accept();
                           System.Diagnostics.Debug.WriteLine("Socket accepting");
                           connected = true;
                           Invoke((MethodInvoker)delegate
                           {
                               statusText.Text += ("\nConnected. Waiting for data...");
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
                                       statusText.Text += "\nData transfer complete!";
                                   });

                               }
                           }
                       }
                       catch (Exception exp) //catch socket exceptions
                       {
                           MessageBox.Show("Exceptions catched:\n" + exp);
                       }
                   }
               }//if user has not entered anything show warning
               else
               {
                   MessageBox.Show("Please enter valid IPv4 Address of Server");
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
            receiver.Close();
            acceptor.Close();
            socket.Close();

        }

        public string getIPv4()
        {
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            string ipv4 = String.Empty;

            foreach (IPAddress address in localIPs)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                    ipv4 =  address.ToString();
            }

            return ipv4;
        }

 
    }
}
