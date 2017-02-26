using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//example of https://www.codeproject.com/Articles/13232/A-very-basic-TCP-server-written-in-C

namespace Fusee.Forschungsprojekt.Desktop
{
    public class PointDataServiceProvider: TcpServiceProvider
    {
        private string receivedData;

        public override object Clone()
        {
            return new PointDataServiceProvider();
        }

        //connection ready
        public override void OnAcceptConnection(ConnectionState state)
        {
            receivedData = "";
            if (!state.Write(Encoding.UTF8.GetBytes("Hello World\r\n"), 0, 14))//Hello world is symbol, that connection is ready
                state.EndConnection(); //if write fails then end connection --> make sure that connection is always closed at the end
        }

        public override void OnReceiveData(ConnectionState state)
        {
            byte[] buffer = new byte[1024];
            while(state.AvailableData > 0)
            {
                int readBytes = state.Read(buffer, 0, 1024);
                if (readBytes > 0)
                {
                    receivedData += Encoding.UTF8.GetString(buffer, 0, readBytes);
                    if (receivedData.IndexOf("<EOF>") >= 0)//messages must end with the string "<EOF>" (end of file)
                    {
                        state.Write(Encoding.UTF8.GetBytes(receivedData), 0, receivedData.Length);
                        receivedData = "";
                    }
                } else state.EndConnection(); //If read fails close connection

            }
        }
        public override void OnDropConnection(ConnectionState state)
        {
            //Nothing to clean here
        }
    }
}
