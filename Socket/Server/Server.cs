using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    public class Server
    {
        //UDP and TCP listeners
        private static TcpListener tcpListener;
        private static UdpClient udpListener;

        //Port and Max connection config
        public static int maxConnection { get; private set; }
        public static int port { get; private set; }

        //store the FTP clients, id and keys in a List
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();

        //Packet Handle
        public delegate void PacketHandler(int _fromClient, Packet _packet); //define delegate to handle with packets received
        public static Dictionary<int, PacketHandler> packetHandlers; //we are store here delegates


        public static void Start(int _maxConnection, int _port)
        {
           
            //set port and maxConnection
            maxConnection = _maxConnection;
            port = _port;

            //
            Console.WriteLine($"Starting server...");
            InitializeServerData();

            //Begin accept TCP clients.
            tcpListener = new TcpListener(IPAddress.Any, port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

            //Begin accept UDP clients. 
            udpListener = new UdpClient(port);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            Console.WriteLine($"Server started on {port}.");

        }

        private static void InitializeServerData()
        {
            //Initialize all empty clients 
            for (int i = 1; i <= maxConnection; i++)
            {
                clients.Add(i, new Client(i));
            }

            //Initialize PacketHandler dictionary that we reaceive from cliets
            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientPackets.welcomePacketReceived, ServerHandle.WelcomeReceived },
                { (int)ClientPackets.message, ServerHandle.Message }
            };

            Console.WriteLine($"Initialized packet.");
        }

        #region UDP
        public static void UDPReceiveCallback(IAsyncResult _result)
        {
            try
            {
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = udpListener.EndReceive(_result, ref clientEndPoint);
                udpListener.BeginReceive(UDPReceiveCallback, null);

                if (data.Length < 4)
                {
                    return;
                }

                //Handle received Data
                using (Packet _packet = new Packet(data))
                {
                    int clientId = _packet.ReadInt();

                    //
                    if (clientId == 0)
                    {
                        return;
                    }

                    if (clients[clientId].udp.endPoint == null)
                    {
                        clients[clientId].udp.Connect(clientEndPoint);
                        return;
                    }

                    if (clients[clientId].udp.endPoint.ToString() == clientEndPoint.ToString())
                    {
                        clients[clientId].udp.HandleData(_packet);
                    }
                }

            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error receiving UDP data: {ex}");
            }
        }


        public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packed)
        {
            try
            {
                if (_clientEndPoint != null)
                {
                    udpListener.BeginSend(_packed.ToArray(), _packed.Length(), _clientEndPoint, null, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending data to {_clientEndPoint} via UDP: {ex}");
            }
        }
        #endregion

        #region TCP

        private static void TCPConnectCallback(IAsyncResult _result)
        {
            TcpClient client = tcpListener.EndAcceptTcpClient(_result);
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

            Console.WriteLine($"Incoming connection from {client.Client.RemoteEndPoint}...");

            for (int i = 1; i < maxConnection; i++)
            {
                if (clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(client);
                    return;
                }
            }

            Console.WriteLine($"{client.Client.RemoteEndPoint} failed to connect: Server full.");
        }
        #endregion
    }
}
