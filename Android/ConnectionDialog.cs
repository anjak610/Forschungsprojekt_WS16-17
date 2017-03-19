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
    class ConnectionDialog : DialogFragment
    {
    
        protected EditText IPEditText;
        protected TextView StatusText;
        protected Button ConnectButton;
        protected Button DisconnectButton;

        private static ReceiverClient client;
       

        //string that will hold IP input
        protected string IPinput = "";

        public static ConnectionDialog NewInstance()
        {
            var dialogFragment = new ConnectionDialog();
            client = new ReceiverClient();
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
                ConnectButton = dialogView.FindViewById<Button>(Resource.Id.connectBtn);
                DisconnectButton = dialogView.FindViewById<Button>(Resource.Id.disconnectBtn);

                //ConnectButton event handler
                ConnectButton.Click += (sender, args) =>
                {       

            new Thread(() => //thread for receiving data
            {
                //Send own IP to listening server            
                string serverip = IPEditText.Text;
                string localip = getIPv4();

                IPAddress inputaddress;
                if (IPAddress.TryParse(serverip, out inputaddress))
                {
                    if (inputaddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        try
                        {
                            client.ConnecttoServer(serverip, localip);

                            Activity.RunOnUiThread(() => {
                                StatusText.Text += ("\nSent local IP: " + localip);
                            });

                            client.SetupReceiving();
                        
                            Activity.RunOnUiThread(() => {                           
                                StatusText.Text += ("\nConnected. Receiving data...");
                            });

                            client.Receive();

                            if (client.completed) {
                                System.Diagnostics.Debug.WriteLine("Data transfer complete");
                                Activity.RunOnUiThread(() => {
                                       StatusText.Text += "\nData transfer complete!";                                  
                                });
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("Data transfer not completed");
                                Activity.RunOnUiThread(() => {
                                    StatusText.Text += "\nData transfer not completed!";
                                });
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
                    client.Disconect();
                    Activity.RunOnUiThread(() => {
                        StatusText.Text += ("\nConnection closed");
                    });
           
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