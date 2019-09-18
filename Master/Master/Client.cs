using System;
using System.Collections.Generic;
using System.Text;

namespace Master
{
    using System.IO;
    using System.Net.Sockets;

    public class Client
    {
        public Client(TcpClient tcpClient, NetworkStream networkStream, StreamWriter streamWriter, StreamReader streamReader)
        {
            TcpClient = tcpClient;
            NetworkStream = networkStream;
            StreamWriter = streamWriter;
            StreamReader = streamReader;
        }
        public TcpClient TcpClient { get; set; }

        public NetworkStream NetworkStream { get; set; }

        public StreamReader StreamReader { get; set; }

        public StreamWriter StreamWriter { get; set; }
    }
}
