using System;
using System.Collections.Generic;
using System.Text;

namespace Client
{
    ////PACKETS TO BE SENT TO SERVER - HANDLE HERE
    public class ClientSend
    {
        private static void SendTCPData(Packet packet)
        {
            packet.WriteLength();
            Client.Instance.tcp.SendData(packet);
        }

        private static void SendUDPData(Packet packet)
        {
            packet.WriteLength();
            Client.Instance.udp.SendData(packet);
        }

        #region Packet
        public static void WelcomeReceived()
        {
            using (Packet packet = new Packet((int)ClientPackets.welcomeReceived))
            {
                //IMPORTANT: we are using TCP, so we have make sure to send the packed to client in the same order

                packet.Write(Client.Instance.myId);
                packet.Write("USER " + Client.Instance.myId);

                SendTCPData(packet);
            }
        }

        public static void Message(string msg)
        {
            using (Packet packet = new Packet((int)ClientPackets.message))
            {
                //IMPORTANT: we are using TCP, so we have make sure to send the packed to client in the same order
                packet.Write(Client.Instance.myId);
                packet.Write(msg);

                SendTCPData(packet);
            }
        }


        #endregion
    }
}
