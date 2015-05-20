using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Runtime.InteropServices;

namespace ExpertSync
{
    public class StateObject
    {
        // Size of receive buffer.
        public const int BufferSize = 256;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public Func<string> reCb;
    }

    public class DisconnectEventArgs : EventArgs
    {
        public bool requested;
    }

    public class MessageEventArgs : EventArgs
    {
        public byte modulation;
        public double vfoa;
        public double vfob;
    }

    public class ExpertSyncConnector
    {
        private static int timeout = 1000;

        class CmdEntry
        {
            public string cmd;
            public Action<string> cb;

            public CmdEntry(string cmdE, Action<string> cbE)
            {
                cmd = cmdE;
                cb = cbE;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack=1)]
        struct Message
        {
	        public byte sid;          //!< software ID
            public byte type;         //!< тип пакета
            public byte receiver;     //!< номер программного приёмника
            public double vfoa;                //!< частота приёмника A
            public double vfob;                //!< частота приёмника B
            public byte modulation;   //!< индекс модуляции eSyncModulation
        }
       
        // ManualResetEvent instances signal completion.
        private ManualResetEvent connectDone =
            new ManualResetEvent(false);
        private ManualResetEvent sendDone =
            new ManualResetEvent(false);
        private ManualResetEvent receiveDone =
            new ManualResetEvent(false);

        private IPEndPoint remoteEP;
        private volatile Socket socket;

        public event EventHandler<DisconnectEventArgs> disconnected;
        public event EventHandler<MessageEventArgs> onMessage;

        public bool connected
        {
            get
            {
                return socket != null && socket.Connected;
            }
        }

        
        public static ExpertSyncConnector create( string host, int port )
        {
            IPAddress hostIP;
            if (IPAddress.TryParse(host, out hostIP))
            {
                ExpertSyncConnector c = new ExpertSyncConnector();
                c.remoteEP = new IPEndPoint(hostIP, port);
                return c;
            }
            else
            {
                return null;
            }
        }


        public bool connect()
        {
            // Connect to a remote device.
            try
            {
                int retryCo = 0;

                while ((socket == null || !socket.Connected) && retryCo++ < 3)
                {
                    System.Diagnostics.Debug.WriteLine("Connecting...");
                    // Create a TCP/IP socket.
                    socket = new Socket(AddressFamily.InterNetwork,
                        SocketType.Stream, ProtocolType.Tcp);

                    // Connect to the remote endpoint.
                    IAsyncResult ar = socket.BeginConnect(remoteEP,
                        new AsyncCallback(connectCallback), null);
                    ar.AsyncWaitHandle.WaitOne(timeout, true);

                    if (socket != null && !socket.Connected)
                    {
                        socket.Close();
                        System.Diagnostics.Debug.WriteLine("Timeout");
                    }
                    else
                        receive();
                }
                if (socket == null || !socket.Connected)
                    System.Diagnostics.Debug.WriteLine("Retries limit reached. Connect failed");
                return (socket != null && socket.Connected);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
                return false;
            }
        }

        public void disconnect()
        {
            _disconnect(true);
        }

        private void _disconnect(bool requested)
        {
            System.Diagnostics.Debug.WriteLine("disconnect");
            if (socket != null && socket.Connected)
                socket.Close();
            if (disconnected != null)
            {
                DisconnectEventArgs e = new DisconnectEventArgs();
                e.requested = requested;
                disconnected(this, e);
            }
        }

        private void connectCallback(IAsyncResult ar)
        {
            try
            {
                if (socket != null && socket.Connected)
                {
                    socket.EndConnect(ar);

                    System.Diagnostics.Debug.WriteLine("Socket connected to " +
                        socket.RemoteEndPoint.ToString());

                    // Signal that the connection has been made.
                    connectDone.Set();
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }

        private void receive()
        {
            try
            {
                //System.Diagnostics.Debug.WriteLine("receiving");
                // Create the state object.
                StateObject state = new StateObject();

                // Begin receiving the data from the remote device.
                socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(receiveCallback), state);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }

        private void receiveCallback(IAsyncResult ar)
        {
            try
            {
                //System.Diagnostics.Debug.WriteLine("receive callback");
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;

                // Read data from the remote device.
                if (socket != null && socket.Connected)
                {

                    int bytesRead = socket.EndReceive(ar);

                    if (bytesRead > 0)
                    {
                        // There might be more data, so store the data received so far.
                        GCHandle pinnedBuffer = GCHandle.Alloc(state.buffer, GCHandleType.Pinned);
                        Message msg = (Message)Marshal.PtrToStructure(pinnedBuffer.AddrOfPinnedObject(),
                                typeof(Message));

                        processReply(msg);

                        receiveDone.Set();
                        receive();
                    } else {
                        _disconnect(false);
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }

        private void processReply(Message msg)
        {
            if (onMessage != null)
            {
                MessageEventArgs ea = new MessageEventArgs
                {
                    modulation = msg.modulation,
                    vfoa = msg.vfoa,
                    vfob = msg.vfob
                };
                onMessage(this, ea);
            }
        }


        private void replyTimeout()
        {
            System.Diagnostics.Debug.WriteLine( "Reply timeout" );
            _disconnect(false);
        }

        private void send(String data)
        {
            if (socket != null && socket.Connected)
            {
                System.Diagnostics.Debug.WriteLine("sending: " + data);
                // Convert the string data to byte data using ASCII encoding.
                byte[] byteData = Encoding.ASCII.GetBytes(data);

                // Begin sending the data to the remote device.
                socket.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(sendCallback), null);
            }
        }


        private void sendCallback(IAsyncResult ar)
        {
            try
            {

                // Complete sending the data to the remote device.
                int bytesSent = socket.EndSend(ar);
                System.Diagnostics.Debug.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.
                sendDone.Set();
                
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }

    }

  

}
