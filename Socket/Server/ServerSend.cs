using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    //this class have every method to send packed throw network
    public class ServerSend
    {
        //send packet to a specific client
        private static void SendTCPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[_toClient].tcp.SendData(_packet);
        }
        private static void SendUDPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[_toClient].udp.SendData(_packet);
        }


        //send pack to everyone/ broadcast
        private static void SendTCPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i < Server.clients.Count; i++)
            {
                Server.clients[i].tcp.SendData(_packet);
            }

        }
        private static void SendUDPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i < Server.clients.Count; i++)
            {
                Server.clients[i].udp.SendData(_packet);
            }

        }


        //send pack to everyone/ broadcast
        private static void SendTCPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i < Server.clients.Count; i++)
            {
                if (i != _exceptClient)
                {
                    Server.clients[i].tcp.SendData(_packet);
                }

            }

        }
        private static void SendUDPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i < Server.clients.Count; i++)
            {
                if (i != _exceptClient)
                {
                    Server.clients[i].udp.SendData(_packet);
                }

            }

        }


        #region Packets

        //Welcome is called when de client connect to server.
        public static void Welcome(int _toClient, string _msg)
        {
            using (Packet _packet = new Packet((int)ServerPackets.welcomePacketSent))
            {
                //IMPORTANT: we are using TCP, so we have make sure to sehnd the packed to client in the same order
                _packet.Write(_msg); //order 1
                _packet.Write(_toClient); //order 2

                SendTCPData(_toClient, _packet);
                //SendTCPDataToAll(_packet);
            }
        }

        //
        public static void Message(int _toClient, string _msg)
        {
            using (Packet _packet = new Packet((int)ServerPackets.message))
            {
                //IMPORTANT: we are using TCP, so we have make sure to sehnd the packed to client in the same order
                _packet.Write(_toClient);
                _packet.Write(_msg);

                SendTCPData(_toClient, _packet);
            }
        }

        #endregion
    }
}
