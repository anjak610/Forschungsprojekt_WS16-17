using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

//NOT USED SO FAR
//listener for async socket 
namespace Server
{
    class Listener
    {
        public delegate void SocketAcceptedHandler(Socket e);
        public event SocketAcceptedHandler Accepted;

        Socket mylistener;
        public int Port;

        public bool Running
        {
            get;
            private set;

        }
         //constructor
        public Listener() { Port = 0; }

        //start listening on specified port 
        public void Start(int port)
        {
            if (Running)
                return;
            mylistener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            mylistener.Bind(new IPEndPoint(0, port));
            mylistener.Listen(0);

            mylistener.BeginAccept(acceptedCallback, null);
            Running = true;

        }

        //stop listening
        public void Stop()
        {
            if (!Running)
            {
                return;
            }
            mylistener.Close();
            Running = false;
        }

        //callback: if data has been received callback closes accepting process at socket
        void acceptedCallback(IAsyncResult ar)
        {
            try
            {
                Socket s = mylistener.EndAccept(ar);

                if (Accepted != null)
                {
                    Accepted(s);
                }
            }
            catch
            {

            }
        }
    }
}
