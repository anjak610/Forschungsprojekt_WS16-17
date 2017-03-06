using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace Fusee.Forschungsprojekt.Android
{
    // TODO: Implement connection funtionality like in desktop version
    class ConnectionDialog : DialogFragment
    {
        protected EditText IPEditText;
        protected TextView StatusText;
        protected TextView ReceivedText;
        protected Button ConnectButton;
        protected Button DisconnectButton;

        private static Socket socket;
        private static Socket receiver;
        private static Socket acceptor;
        private static Boolean connected = false;

        //string that will hold IP input
        protected string IPinput = "";

        public static ConnectionDialog NewInstance()
        {
            var dialogFragment = new ConnectionDialog();
            receiver = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            return dialogFragment;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Begin building a new dialog.
            var builder = new AlertDialog.Builder(Activity);

            //Get the layout inflater
            var inflater = Activity.LayoutInflater;

            //Inflate the layout for this dialog
            var dialogView = inflater.Inflate(Resource.Layout.connection_dialog_layout, null);

            if (dialogView != null)
            {
                //Initialize the properties
                IPEditText = dialogView.FindViewById<EditText>(Resource.Id.IPinputField);
                StatusText = dialogView.FindViewById<TextView>(Resource.Id.statusTextView);
                ReceivedText = dialogView.FindViewById<TextView>(Resource.Id.receivedTextView);
                ConnectButton = dialogView.FindViewById<Button>(Resource.Id.connectBtn);
                DisconnectButton = dialogView.FindViewById<Button>(Resource.Id.disconnectBtn);

                //ConnectButton event handler
                ConnectButton.Click += (sender, args) =>
                {       

            new Thread(() => //thread for receiving data
            {
                //Send own IP to listening server            
                string serverip = IPEditText.Text;

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

                            Activity.RunOnUiThread(() => {
                                StatusText.Text += ("\nSent local IP: " + localip);
                            });

                            //receiving data
                            receiver.Bind(new IPEndPoint(0, 1994));
                            receiver.Listen(0);
                            acceptor = receiver.Accept();
                            System.Diagnostics.Debug.WriteLine("Socket accepting");
                            connected = true;
                            Activity.RunOnUiThread(() => {
                            
                                StatusText.Text += ("\nConnected. Waiting for data...");
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

                                    Activity.RunOnUiThread( () => {
                                    
                                        ReceivedText.Text = Encoding.UTF8.GetString(data);
                                        StatusText.Text += "\nData transfer complete!";
                                    });

                                }
                            }
                        }
                        catch (Exception exp) //catch socket exceptions
                        {
                            string msg = "Exceptions catched: " + exp;
                            Toast.MakeText(Application.Context, msg, ToastLength.Long).Show();
                        }
                    }
                }//if user has not entered anything show warning
                else
                {
                    string msg = "Please enter valid IPv4 Address of Server";
                    Toast.MakeText(Application.Context, msg, ToastLength.Long).Show();
                }
            }).Start();

        };

                //Disconnect 
                DisconnectButton.Click += (sender, args) =>
                {
                    Activity.RunOnUiThread(() => {
                        StatusText.Text += ("\nConnection closed");
                    });
                    connected = false;
                    receiver.Close();
                    acceptor.Close();
                    socket.Close();
                };

            }

            builder.SetView(dialogView);
            builder.SetNegativeButton("Close", HandleNegativeButtonClick);
        

            //Create the builder 
            var dialog = builder.Create();
           
            //Now return the constructed dialog to the calling activity
            return dialog;
        
        }


        public string getIPv4()
        {
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            string ipv4 = String.Empty;

            foreach (IPAddress address in localIPs)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                    ipv4 = address.ToString();
            }

            return ipv4;
        }
        private void HandleNegativeButtonClick(object sender, DialogClickEventArgs e)
        {
            var dialog = (AlertDialog)sender;
            dialog.Dismiss();
        }

    }
}