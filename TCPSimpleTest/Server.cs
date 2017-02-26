using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;

//NOT USED SO FAR
//async server  example 
namespace Server
{
    enum Commands : int //commands for server 
    {
        String = 0,
        Image
    }
    struct ReceiveBuffer //buffer that holds data
    {
        public const int BUFFER_SIZE = 1024;
        public byte[] Buffer;
        public int ToReceive;
        public MemoryStream BufStream;//received data will write to this stream

        public ReceiveBuffer(int toRec)
        {
            Buffer = new byte[BUFFER_SIZE];
            ToReceive = toRec;//length of data we are expecting to receive
            BufStream = new MemoryStream(toRec);

        }

        public void Dispose()
        {
            Buffer = null;
            ToReceive = 0;
            Close();

            if (BufStream != null)
            {
                BufStream.Dispose();

            }

        }

        public void Close()
        {
            if (BufStream != null && BufStream.CanWrite)
            {
                BufStream.Close();
            }
        }


    }

    //one client necessary for each connection
    class Client
    {
        byte[] lenBuffer;
        ReceiveBuffer buffer;
        Socket socket;

        public IPEndPoint EndPoint
        {
            get
            {
                //if there's no socket and it is not connected then get IPEndPoint
                if (socket != null && socket.Connected)
                {
                    return (IPEndPoint)socket.RemoteEndPoint;

                }
                return new IPEndPoint(IPAddress.None, 0);
            }
        }

        //delegate --> method references // event handlers
        public delegate void DisconnectedEventHandler(Client sender);
        public event DisconnectedEventHandler Disconnected;
        public delegate void DataReceivedEventHandler(Client sender, ReceiveBuffer e);
        public event DataReceivedEventHandler DataReceived;


        //constructor creates client object with socket
        public Client(Socket s)
        {
            socket = s;
            lenBuffer = new byte[4];//byte length of integer because int will be received datatype
        }

        //close connection and dispose everything
        public void Close()
        {
            if (socket != null)
            {
                socket.Disconnect(false);
                socket.Close();

            }
            buffer.Dispose();
            socket = null;
            lenBuffer = null;
            Disconnected = null;
            DataReceived = null;

        }

        //receiving data will be happening asynchronous
        public void ReceiveAsync()
        {
            socket.BeginReceive(lenBuffer, 0, lenBuffer.Length, SocketFlags.None, receiveCallback, null);
        }

        void receiveCallback(IAsyncResult ar)
        {
            try
            {
                int rec = socket.EndReceive(ar);
                if (rec == 0)//disconnected --> no received data
                {
                    if (Disconnected != null)
                    {
                        Disconnected(this);
                        return;

                    }

                    if (rec != 4)//data type/byte array length not matching
                    {
                        throw new Exception();
                    }
                }
            }
            catch (SocketException se)
            {
                switch (se.SocketErrorCode)//socket disconnected
                {
                    case SocketError.ConnectionAborted:
                    case SocketError.ConnectionReset:
                        if (Disconnected != null)
                        {
                            Disconnected(this);
                            return;
                        }
                        break;


                }
            }
            //Exception handling
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (NullReferenceException)
            {
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            buffer = new ReceiveBuffer(BitConverter.ToInt32(lenBuffer, 0));
            socket.BeginReceive(buffer.Buffer, 0, buffer.Buffer.Length, SocketFlags.None, receivePacketCallback, null);//receive callback function

        }
        void receivePacketCallback(IAsyncResult ar)
        {
            int rec = socket.EndReceive(ar);
            if (rec <= 0) //nothing received
            {
                return;
            }
            buffer.BufStream.Write(buffer.Buffer, 0, rec);
            buffer.ToReceive -= rec;//subtract what we have from the length

            if (buffer.ToReceive > 0)//more data to be received
            {
                Array.Clear(buffer.Buffer, 0, buffer.Buffer.Length);
                socket.BeginReceive(buffer.Buffer, 0, buffer.Buffer.Length, SocketFlags.None, receiveCallback, null);
                return;

            }

            if (DataReceived != null)
            {
                buffer.BufStream.Position = 0;
                DataReceived(this, buffer);
            }

            buffer.Dispose();
            ReceiveAsync();//begin to receive data from socket

        }


    }
}

