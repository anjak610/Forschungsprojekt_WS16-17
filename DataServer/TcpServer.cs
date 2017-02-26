using System;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Collections;
using System.Threading;

//example of https://www.codeproject.com/Articles/13232/A-very-basic-TCP-server-written-in-C
//Server for sending data

namespace Fusee.Forschungsprojekt.Desktop
{
    public class TcpServer
    {
        private int port;
        private Socket listener;
        private TcpServiceProvider provider;
        private ArrayList connections;
        private int maxConnections = 100;

        private AsyncCallback ConnectionReady;
        private WaitCallback AcceptConnection;
        private AsyncCallback ReceivedDataReady;

        /// <SUMMARY>
        /// Initializes server. To start accepting
        /// connections call Start method.
        /// </SUMMARY>
        public TcpServer(TcpServiceProvider conn_provider, int conn_port)
        {
            provider = conn_provider;
            port = conn_port;
            listener = new Socket(AddressFamily.InterNetwork, //listener == socket
                            SocketType.Stream, ProtocolType.Tcp);
            connections = new ArrayList();
            ConnectionReady = new AsyncCallback(ConnectionReady_Handler);
            AcceptConnection = new WaitCallback(AcceptConnection_Handler);
            ReceivedDataReady = new AsyncCallback(ReceivedDataReady_Handler);
        }


        /// <SUMMARY>
        /// Start accepting connections.
        /// A false return value tell you that the port is not available.
        /// </SUMMARY>
        public bool Start()
        {
            try
            {
                Console.Write("Setting up server...");
                //127.0.0.1 localhost
                listener.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));//or use IPEndPoint(IPAddress.Any, port));
                listener.Listen(100);//101 pending connections
                listener.BeginAccept(ConnectionReady, null);
                return true;
            }
            catch
            {
                return false;
            }
        }


        /// <SUMMARY>
        /// Callback function: A new connection is waiting.
        /// </SUMMARY>
        private void ConnectionReady_Handler(IAsyncResult ar)
        {
            lock (this)
            {
                if (listener == null) return;
                Socket conn = listener.EndAccept(ar);
                if (connections.Count >= maxConnections)
                {
                    //Max number of connections reached.
                    string msg = "SE001: Server busy";
                    conn.Send(Encoding.UTF8.GetBytes(msg), 0,
                              msg.Length, SocketFlags.None);
                    conn.Shutdown(SocketShutdown.Both);
                    conn.Close();
                }
                else
                {
                    //Start servicing a new connection
                    ConnectionState st = new ConnectionState();
                    st.socket = conn;
                    st.server = this;
                    st.provider = (TcpServiceProvider)provider.Clone();
                    st.buffer = new byte[4];
                    connections.Add(st);
                    //Queue the rest of the job to be executed latter
                    ThreadPool.QueueUserWorkItem(AcceptConnection, st);
                }
                //Resume the listening callback loop
                listener.BeginAccept(ConnectionReady, null);
            }
        }


        /// <SUMMARY>
        /// Executes OnAcceptConnection method from the service provider.
        /// </SUMMARY>
        private void AcceptConnection_Handler(object state)
        {
            ConnectionState st = state as ConnectionState;
            try { st.provider.OnAcceptConnection(st); }
            catch
            {
                //report error in provider... Probably to the EventLog
                System.Diagnostics.Debug.WriteLine("AcceptConnection Handler: Error, Connection not accepted");
            }
            //Starts the ReceiveData callback loop
            if (st.socket.Connected)
                st.socket.BeginReceive(st.buffer, 0, 0, SocketFlags.None,
                  ReceivedDataReady, st);
        }


        /// <SUMMARY>
        /// Executes OnReceiveData method from the service provider.
        /// </SUMMARY>
        private void ReceivedDataReady_Handler(IAsyncResult ar) //received callback
        {
            ConnectionState st = ar.AsyncState as ConnectionState;
            st.socket.EndReceive(ar);
            //Im considering the following condition as a signal that the
            //remote host droped the connection.
            if (st.socket.Available == 0) DropConnection(st);
            else
            {
                try { st.provider.OnReceiveData(st); }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("Provider ReceivedDataReady Handler: Data not received");
                }
                //Resume ReceivedData callback loop
                if (st.socket.Connected)
                    st.socket.BeginReceive(st.buffer, 0, 0, SocketFlags.None,
                      ReceivedDataReady, st);
            }
        }


        /// <SUMMARY>
        /// Shutsdown the server
        /// </SUMMARY>
        public void Stop()
        {
            lock (this)
            {
                listener.Close();
                listener = null;
                //Close all active connections
                foreach (object obj in connections)
                {
                    ConnectionState st = obj as ConnectionState;
                    try { st.provider.OnDropConnection(st); }
                    catch
                    {
                        System.Diagnostics.Debug.WriteLine("Provider Stop(): OnDropConnection failed");
                    }
                    st.socket.Shutdown(SocketShutdown.Both);
                    st.socket.Close();
                }
                connections.Clear();
            }
        }


        /// <SUMMARY>
        /// Removes a connection from the list
        /// </SUMMARY>
        internal void DropConnection(ConnectionState st)
        {
            lock (this)
            {
                st.socket.Shutdown(SocketShutdown.Both);
                st.socket.Close();
                if (connections.Contains(st))
                    connections.Remove(st);
            }
        }


        public int MaxConnections
        {
            get
            {
                return maxConnections;
            }
            set
            {
                maxConnections = value;
            }
        }


        public int CurrentConnections
        {
            get
            {
                lock (this) { return connections.Count; }
            }
        }
    }
}

