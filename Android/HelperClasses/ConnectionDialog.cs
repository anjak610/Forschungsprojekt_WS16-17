using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Fusee.Tutorial.Android.HelperClasses
{
    class ConnectionDialog : DialogFragment
    {
        public Action<int> OnNewPortCallbacks;

        protected EditText PortEditText;
        protected Button ConnectButton;
        
        // will hold port input
        protected int _port = 50123;

        public static ConnectionDialog NewInstance()
        {
            var dialogFragment = new ConnectionDialog();
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
                PortEditText = dialogView.FindViewById<EditText>(Resource.Id.PortInputField);
                ConnectButton = dialogView.FindViewById<Button>(Resource.Id.connectBtn);
                //ConnectButton event handler
                ConnectButton.Click += HandleConnectButtonClick;
            }

            PortEditText.Text = _port.ToString();

            builder.SetView(dialogView);
            builder.SetNegativeButton("Close", HandleNegativeButtonClick);
        
            //Create the builder 
            var dialog = builder.Create();
           
            //Now return the constructed dialog to the calling activity
            return dialog;
        }
        
        private void HandleNegativeButtonClick(object sender, DialogClickEventArgs e)
        {
            var dialog = (AlertDialog) sender;
            dialog.Dismiss();
        }

        private void HandleConnectButtonClick(object sender, EventArgs e)
        {
            int port;
            bool success = int.TryParse(PortEditText.Text, out port);

            if (success)
            {
                _port = port;

                OnNewPortCallbacks?.Invoke(port);
                Dismiss();

                string msg = "Port number was refreshed.";
                Toast.MakeText(Application.Context, msg, ToastLength.Long).Show();
            }
            else
            {
                string msg = "Please enter a valid port number.";
                Toast.MakeText(Application.Context, msg, ToastLength.Long).Show();
            }
        }
    }
}