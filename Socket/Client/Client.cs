using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System;


namespace Client
{
    public sealed class Client
    {
        private static Client instance = null;
        private static readonly object padlock = new object();

        public static int dataBufferSize = 4096; //4mb

        public string ip = "127.0.0.1"; //localhost
        public int port = 26951; //the same server

        public int myId = 0;
        public TCP tcp;
        public UDP udp;


        private delegate void PacketHandler(Packet _packet);
        private static Dictionary<int, PacketHandler> packetHandlers;


        //singleton, so that we have only one instance and not multiple connection on server
        Client()
        {
           
        }

        public static Client Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new Client();                      
                    }
                    
                    return instance;
                }
            }
        }


        public void ConnectToServer()
        {
            tcp = new TCP();
            udp = new UDP();

            InitializeClientData();
            tcp.Connect();
            udp.Connect(port);
        }

        //TCP Protocol
        public class TCP
        {
            public TcpClient socket;

            private NetworkStream stream;
            private Packet receivedData; //received data from the server
            private byte[] receiveBuffer;

            public void Connect()
            {
                socket = new TcpClient
                {
                    ReceiveBufferSize = dataBufferSize,
                    SendBufferSize = dataBufferSize
                };

                receiveBuffer = new byte[dataBufferSize];
                socket.BeginConnect(instance.ip, instance.port, ConnectionCallback, socket);
            }

            private void ConnectionCallback(IAsyncResult _result)
            {
                socket.EndConnect(_result);

                if (!socket.Connected)
                {
                    return;
                }

                stream = socket.GetStream();

                receivedData = new Packet();

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }

            public void SendData(Packet _packet)
            {
                try
                {
                    if (socket != null)
                    {
                        stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                    }
                }
                catch (Exception _err)
                {

                    Console.WriteLine($"Error sending data to serve via TCP: {_err}");
                }
            }

            private void ReceiveCallback(IAsyncResult _result)
            {
                try
                {
                    int byteLenght = stream.EndRead(_result);

                    if (byteLenght <= 0)
                    {
                        //TODO disconnect
                        return;
                    }

                    byte[] data = new byte[byteLenght];
                    Array.Copy(receiveBuffer, data, byteLenght);

                    //handle data
                    receivedData.Reset(HandlerData(data)); //reset packet instance to allw it to be reused
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch (Exception _err)
                {

                    Console.WriteLine(_err);
                }
            }

            private bool HandlerData(byte[] _data)
            {
                int packetLenght = 0;

                receivedData.SetBytes(_data);

                if (receivedData.UnreadLength() >= 4)
                {
                    packetLenght = receivedData.ReadInt();
                    if (packetLenght <= 0)
                    {
                        return true;
                    }
                }

                while (packetLenght > 0 && packetLenght <= receivedData.UnreadLength())
                {
                    byte[] packetBytes = receivedData.ReadBytes(packetLenght);

                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet _packet = new Packet(packetBytes))
                        {
                            int packetId = _packet.ReadInt();

                            //execute a method on dictionary
                            packetHandlers[packetId](_packet);
                        }
                    });

                    packetLenght = 0;
                    if (receivedData.UnreadLength() >= 4)
                    {
                        packetLenght = receivedData.ReadInt();
                        if (packetLenght <= 0)
                        {
                            return true;
                        }
                    }
                }

                if (packetLenght <= 1)
                {
                    return true;
                }

                return false;
            }


        }


        //UDP Protocol
        public class UDP
        {
            public UdpClient socket; //protocol
            public IPEndPoint endPoint; //endpoint

            public UDP()
            {
                endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
            }


            public void Connect(int localPort)
            {
                socket = new UdpClient(); //instantiate a udp client
                socket.Connect(endPoint); //connect to endpoint
                socket.BeginReceive(ReceiveCallback, null); //Receives a datagram from a remote host asynchronously.

                using (Packet packet = new Packet())
                {
                    SendData(packet);
                }
            }

            public void SendData(Packet packet)
            {
                try
                {
                    packet.InsertInt(instance.myId);
                    if (socket != null)
                    {
                        socket.BeginSend(packet.ToArray(), packet.Length(), null, null);
                    }
                }
                catch (Exception ex)
                {

                    Console.WriteLine($"Error sending data to server via UDP: {ex}");
                }
            }

            //An AsyncCallback delegate that references the method to invoke when the operation is complete.
            private void ReceiveCallback(IAsyncResult result)
            {
                try
                {
                    byte[] data = socket.EndReceive(result, ref endPoint); //Ends a pending asynchronous receive.
                    socket.BeginReceive(ReceiveCallback, null); //Receives a datagram from a remote host asynchronously.

                    //make sure we have data to handle
                    if (data.Length < 4)
                    {
                        //TODO disconnect
                        return;
                    }

                    HandleData(data);
                }
                catch (Exception err)
                {

                    //TODO disconnect
                }
            }

            private void HandleData(byte[] data)
            {
                using (Packet packet = new Packet(data))
                {
                    int packetLength = packet.ReadInt();
                    data = packet.ReadBytes(packetLength);
                }

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet packet = new Packet(data))
                    {
                        int packetId = packet.ReadInt();
                        packetHandlers[packetId](packet);
                    }
                });
            }
        }


        private void InitializeClientData()
        {
            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ServerPackets.welcome, ClientHandle.Welcome }
            };

            Console.WriteLine("Initialized packetes.");

        }
    }
}
