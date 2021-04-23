using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class ServerHandle
    {
        public static void WelcomeReceived(int _fromClient, Packet _packet)
        {

            //IMPORTANT: we are using TCP, so we have make sure to rceive the packed from client in the same order
            int clientIdCheck = _packet.ReadInt();
            string clientUserName = _packet.ReadString();

            Console.WriteLine($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfuly and is now Client: {_fromClient}");

            if (_fromClient != clientIdCheck)
            {
                Console.WriteLine($"User {clientUserName} ID: {_fromClient} has assumed the wrong client ID ({clientIdCheck})");
            }

            //

        }

        public static void Message(int _fromClient, Packet _packet)
        {

            //IMPORTANT: we are using TCP, so we have make sure to rceive the packed from client in the same order
            int clientId = _packet.ReadInt();
            string clientMessage = _packet.ReadString();

            Console.WriteLine($"User {clientId}: {clientMessage}");

           

        }
    }
}
