﻿using System;
using System.Net.Sockets;

namespace Master
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            while (true)
            {
                TcpClient client = Server.AcceptClient().Result;
                ChatRoom.HandShake(client);
            }
        }
    }
}

