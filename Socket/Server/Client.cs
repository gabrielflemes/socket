using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    public class Client
    {
        public static int dataBufferSize = 4096; //4mb 

        //client info
        public int clientId;
        public string name;

        //
        public TCP tcp;
        public UDP udp;


        public Client(int _clientId)
        {
            clientId = _clientId;
            tcp = new TCP(clientId);
            udp = new UDP(clientId);
        }


        //TCP Protocol
        public class TCP
        {
            public TcpClient socket;

            private readonly int clientId;

            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;


            public TCP(int _clientId)
            {
                clientId = _clientId;
            }

            public void Connect(TcpClient _socket)
            {
                socket = _socket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                stream = socket.GetStream();

                receivedData = new Packet();
                receiveBuffer = new byte[dataBufferSize];

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

                //send welcome package
                ServerSend.Welcome(clientId, "Welcome to the Server.");

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
                    Console.WriteLine($"Error sending data to player {clientId} via TCP: {_err}");
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

                    receivedData.Reset(HandlerData(data));

                    //TODO handle data
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

                }
                catch (Exception _err)
                {

                    Console.WriteLine($"Errir receiving TCP Data: {_err}");

                    //TODO: disconnect

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
                            Server.packetHandlers[packetId](packetId, _packet);
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
            public IPEndPoint endPoint;
            private int id;

            public UDP(int _id)
            {
                id = _id;
            }

            public void Connect(IPEndPoint _endPoint)
            {

                endPoint = _endPoint;

            }

            public void SendData(Packet _packet)
            {
                Server.SendUDPData(endPoint, _packet);
            }

            public void HandleData(Packet _packetData)
            {
                int packetLength = _packetData.ReadInt();
                byte[] packetBytes = _packetData.ReadBytes(packetLength);

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(packetBytes))
                    {
                        int packetId = _packet.ReadInt();
                        Server.packetHandlers[packetId](id, _packet);
                    }
                });

            }
        }

     
    }
}
