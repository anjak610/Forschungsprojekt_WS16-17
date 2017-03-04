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

        //string that will hold IP input
        protected string IPinput = "";

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
                IPEditText = dialogView.FindViewById<EditText>(Resource.Id.IPinputField);
                StatusText = dialogView.FindViewById<TextView>(Resource.Id.statusTextView);
                ReceivedText = dialogView.FindViewById<TextView>(Resource.Id.receivedTextView);
                ConnectButton = dialogView.FindViewById<Button>(Resource.Id.connectBtn);
                DisconnectButton = dialogView.FindViewById<Button>(Resource.Id.disconnectBtn); ;
              }

            builder.SetView(dialogView);
            builder.SetNegativeButton("Close", HandleNegativeButtonClick);
        

            //Create the builder 
            var dialog = builder.Create();
           
            //Now return the constructed dialog to the calling activity
            return dialog;
        
        }
        private void HandleNegativeButtonClick(object sender, DialogClickEventArgs e)
        {
            var dialog = (AlertDialog)sender;
            dialog.Dismiss();
        }

    }
}