using System;
using System.Collections.Generic;
using System.Text;

namespace Master
{
    using System.IO;
    using System.Net.Sockets;

    public class Client
    {
        public Client(TcpClient tcpClient)
        {
            TcpClient = tcpClient;
            TcpClient.NoDelay = true;
            NetworkStream = TcpClient.GetStream();
            StreamWriter = new StreamWriter(NetworkStream);
            StreamWriter.AutoFlush = true;
            StreamReader = new StreamReader(NetworkStream);
        }
        public TcpClient TcpClient { get; set; }

        public NetworkStream NetworkStream { get; set; }

        public StreamReader StreamReader { get; set; }

        public StreamWriter StreamWriter { get; set; }
    }
}
