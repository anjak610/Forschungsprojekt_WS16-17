using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using Fusee.Forschungsprojekt.Desktop;

//using tutorial: https://www.youtube.com/watch?v=-mYoJBT9XIg&list=PLAC179D21AF94D28F&index=6
/// <summary>
/// Input form for entering remote ServerIP and checking on data transfer status
/// </summary>
namespace Fusee.Tutorial.Desktop.HelperClasses
{
    public partial class ConnectionDialog : Form
    {

        private ReceiverClient client;
        
        public ConnectionDialog()
        {
            InitializeComponent();
            client = new ReceiverClient();


          
        }


        //connect button event handler
        private void connectButton_Click_1(object sender, EventArgs e)
        {

            new Thread(() => //thread for receiving data
           {
           //Send own IP to listening server            
           string serverip = IPinputBox.Text;
           string localip = getIPv4();

           IPAddress inputaddress;
               if (IPAddress.TryParse(serverip, out inputaddress))
               {
                   if (inputaddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                   {
                       try
                       {
                           client.serveraddress = serverip;
                           client.ConnecttoServer(serverip, localip);

                           Invoke((MethodInvoker)delegate
                           {
                               statusText.Text += ("\nSent local IP: " + localip);
                           });

                           client.SetupReceiving();
                           Invoke((MethodInvoker)delegate
                           {
                               statusText.Text += ("\nConnected.\nReceiving data...");
                           });

                           client.Receive();
                         
                           if (client.completed) {
                               System.Diagnostics.Debug.WriteLine("Data transfer complete");
                               Invoke((MethodInvoker)delegate
                               {                                 
                                   statusText.Text += "\nData transfer complete!";
                                                                 
                               });
                           }
                           else
                           {
                               System.Diagnostics.Debug.WriteLine("Data transfer not completed");
                               Invoke((MethodInvoker)delegate
                               {
                                   statusText.Text += "\nData transfer not completed!";
                                   
                               });
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
                   MessageBox.Show("Please enter a valid IPv4 Address of the Server");
               }
           }).Start(); 

        
        }

        private void disconButton_Click(object sender, EventArgs e)
        {
            client.Disconect();

            Invoke((MethodInvoker)delegate
            {
                statusText.Text += ("\nConnection closed");
            });
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

        private void statusText_TextChanged(object sender, EventArgs e)
        {
            statusText.SelectionStart = statusText.Text.Length;
            statusText.ScrollToCaret();
        }
    }
}
