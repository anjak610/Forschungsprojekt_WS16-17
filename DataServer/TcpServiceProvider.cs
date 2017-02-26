using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

//example of https://www.codeproject.com/Articles/13232/A-very-basic-TCP-server-written-in-C

namespace Fusee.Forschungsprojekt.Desktop
{
    /// <SUMMARY>
    /// Allows to provide the server with
    /// the actual code that is going to service
    /// incoming connections.
    /// </SUMMARY>

    public abstract class TcpServiceProvider:ICloneable
    {   
        /// Provides a new instance of the object.
        public virtual object Clone()
        {
            throw new Exception("Derived clases" +
                      " must override Clone method.");
        }

  
        /// Gets executed when the server accepts a new connection.
        public abstract void OnAcceptConnection(ConnectionState state);

        
        /// Gets executed when the server detects incoming data.
        /// This method is called only if
        /// OnAcceptConnection has already finished.
        public abstract void OnReceiveData(ConnectionState state);


        /// Gets executed when the server needs to shutdown the connection.
        public abstract void OnDropConnection(ConnectionState state);
    }
}
