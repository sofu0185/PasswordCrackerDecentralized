using System;
using System.Net.Sockets;

namespace Master
{
    class Program
    {
        static void Main(string[] args)
        {
            Commander.Start();

            while (true)
            {
                TcpClient client = Server.AcceptClient().Result;
                Commander.HandShake(client);
            }
        }
    }
}

