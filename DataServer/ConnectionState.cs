using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text;

//example of https://www.codeproject.com/Articles/13232/A-very-basic-TCP-server-written-in-C

namespace Fusee.Forschungsprojekt.Desktop
{
    /// <SUMMARY>
    /// This class holds information of each client connected
    /// to the server, and provides the means
    /// for sending/receiving data to the remote
    /// host.
    /// </SUMMARY>

    public class ConnectionState
    {
        internal Socket socket;
        internal TcpServer server;
        internal TcpServiceProvider provider;
        internal byte[] buffer;



        /// <SUMMARY>
        /// Tells you the IP Address of the remote host.
        /// </SUMMARY>
        public EndPoint RemoteEndPoint
        {
            get { return socket.RemoteEndPoint; }
        }

        /// <SUMMARY>
        /// Returns the number of bytes waiting to be read.
        /// </SUMMARY>
        public int AvailableData
        {
            get { return socket.Available; }
        }

        /// <SUMMARY>
        /// Tells you if the socket is connected.
        /// </SUMMARY>
        public bool Connected
        {
            get { return socket.Connected; }
        }

        /// <SUMMARY>
        /// Reads data on the socket, returns
        /// the number of bytes read.
        /// </SUMMARY>
        public int Read(byte[] buffer, int offset, int count)
        {
            try
            {
                if (socket.Available > 0)
                    return socket.Receive(buffer, offset,
                           count, SocketFlags.None);
                else return 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <SUMMARY>
        /// Sends Data to the remote host.
        /// </SUMMARY>
        public bool Write(byte[] buffer, int offset, int count)
        {
            try
            {
                socket.Send(buffer, offset, count, SocketFlags.None);
                return true;
            }
            catch
            {
                return false;
            }
        }


        /// <SUMMARY>
        /// Ends connection with the remote host.
        /// </SUMMARY>
        public void EndConnection()
        {
            if (socket != null && socket.Connected)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            server.DropConnection(this);
        }
    }
}
