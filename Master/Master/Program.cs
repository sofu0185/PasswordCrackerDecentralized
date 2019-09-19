using System;
using System.Net.Sockets;

namespace Master
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting server");
            Server.StartServer();
        }
    }
}

