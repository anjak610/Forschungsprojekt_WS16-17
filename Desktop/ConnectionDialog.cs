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

namespace Fusee.Forschungsprojekt.Desktop
{
    public partial class ConnectionDialog : Form
    {
        public ConnectionDialog()
        {
            InitializeComponent();

        }

        private void sendButton_Click_1(object sender, EventArgs e)
        {
            try
            {
                //Simple tcp connection
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("192.168.0.30"), 1994);
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(inputBox.Text), 1994);
                socket.Connect(endPoint);
                //send connection message to server
                string msg = "Connected";
                byte[] msgBuffer = Encoding.Default.GetBytes(msg);
                socket.Send(msgBuffer, 0, msgBuffer.Length, 0);


                new Thread(() => //thread for receiving data
                {
                    //receive data
                    byte[] buffer = new byte[1014];// TODO: Send length of data // TODO: Set appropriate length for data that will be received
                    int receive = socket.Receive(buffer, 0, buffer.Length, 0);
                    //Error in receiving data 
                    if (receive <= 0)
                    {
                        throw new SocketException();
                    }

                    //resize buffer
                    Array.Resize(ref buffer, receive);
                    //write received message to debug console
                    System.Diagnostics.Debug.WriteLine("Received from Server: " + Encoding.Default.GetString(buffer));

                }).Start();

            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine("Server connection failed. Error: " + exp);
                MessageBox.Show("Connection failed");
            }
        }
    }
}
