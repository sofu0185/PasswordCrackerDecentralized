using System;
using System.Collections.Generic;
using System.Text;

namespace Master
{
    using System.Net.Sockets;
    using System.Threading.Tasks;

    public static class Server
    {
        private static TcpListener _server;

        public static Task<TcpClient> AcceptClient()
        {
            if (_server == null)
            {
                _server = TcpListener.Create(6789);
                _server.Start();
            }
            return Task.Run((() => _server.AcceptTcpClient()));
        }
    }
}

