using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    /// <summary>
    /// This class contains information to keep track of the client that is connected to the server and provides
    /// the objects needed for sending and receiving data.
    /// </summary>
    class ConnectionState
    {
        internal Socket connection;
        internal TCPServer server;
        internal byte[] buffer;


       


        /// <SUMMARY>
        /// Returns the number of bytes waiting to be read.
        /// </SUMMARY>
        public int AvailableData
        {
            get { return connection.Available; }
        }

        /// <SUMMARY>
        /// Tells you if the socket is connected.
        /// </SUMMARY>
        public bool Connected
        {
            get { return connection.Connected; }
        }

        /// <SUMMARY>
        /// Reads data on the socket, returns
        /// the number of bytes read.
        /// </SUMMARY>
        public int Read(byte[] buffer, int offset, int count)
        {
            try
            {
                if (connection.Available > 0)
                    return connection.Receive(buffer, offset,
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
                connection.Send(buffer, offset, count, SocketFlags.None);
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
            if (connection != null && connection.Connected)
            {
                connection.Shutdown(SocketShutdown.Both);
                connection.Close();
            }
            //server.DropConnection(this);
        }
    }

}

