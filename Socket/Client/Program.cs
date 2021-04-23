//add the Net and Sockets directive to use udp and ftp
using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;

//System.Text to be able using Encoding.ASCII.GetBytes(someString)
using System.Text;
using System.Threading;
using System.Collections.Generic;

namespace Client
{
    public class Program
    {

        private static bool isRunning = false;

        static void Main(string[] args)
        {
            Console.Title = "[POC TCP/UDP] Client";
            isRunning = true;

            //start a new thead
            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();

            //Start UDP/FTP client.
            Client.Instance.ConnectToServer();
        }

        private static void MainThread()
        {
            Console.WriteLine($"Main thread started running at {Constants.TICKS_PER_SEC} ticks per second.");

            DateTime nextLoop = DateTime.Now;

            while (isRunning)
            {
                //loop
                while (nextLoop < DateTime.Now)
                {
                    //update thread
                    ThreadManager.UpdateMain();

                    nextLoop = nextLoop.AddMilliseconds(Constants.MS_PER_TICK);

                    //To reduce CPU consumption
                    if (nextLoop > DateTime.Now)
                    {
                        Thread.Sleep(nextLoop - DateTime.Now);
                    }
                }
            }
        }


    }
}
