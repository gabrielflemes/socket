using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;

namespace Client
{
    //PACKETS FROM SERVER -  - HANDLE HERE
    public class ClientHandle
    {
        public static void Welcome(Packet _packet)
        {

            //IMPORTANT: we are using TCP, so we have make sure to receive the packed in the same order
            string _msg = _packet.ReadString();
            int _myId = _packet.ReadInt();

            Console.WriteLine($"{_myId} : {_msg}");

            Client.Instance.myId = _myId;

            //send welcome received packet
            ClientSend.WelcomeReceived();

            Client.Instance.udp.Connect(((IPEndPoint)Client.Instance.tcp.socket.Client.LocalEndPoint).Port);
        }


        //
        public static void Message(Packet _packet)
        {

            //IMPORTANT: we are using TCP, so we have make sure to receive the packed in the same order
            int id = _packet.ReadInt();
            string message = _packet.ReadString();

            Console.WriteLine($"{id}:{message}");

            Client.Instance.udp.Connect(((IPEndPoint)Client.Instance.tcp.socket.Client.LocalEndPoint).Port);
        }


    }
}
