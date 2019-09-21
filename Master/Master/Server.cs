using System;
using System.Collections.Generic;
using System.Text;

namespace Master
{
    using System.IO;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    using static Common.ConsoleEnhancing;

    public static class Server
    {
        private static Dictionary<int, Client> _clients;
        private static List<Task> _monitorTasks;
        private static Commander _commander;
        private static TcpListener _server;

        static Server()
        {
            _clients = new Dictionary<int, Client>();
            _monitorTasks = new List<Task>();            
            _commander = new Commander();
            _server = TcpListener.Create(Constants.TCP_SERVER_PORT);

            _server.Start();
            Console.WriteLine();
        }

        public static void StartServer()
        {
            // static constructor runs before this line
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken ct = cts.Token;
            
            TcpClient initialClient = _server.AcceptTcpClient();
            HandShake(initialClient, ref ct);

            Task.Run(() =>
            {
                while (!_commander.EndOfChunks)
                {
                    TcpClient client = _server.AcceptTcpClient();
                    Task.Run(() => HandShake(client, ref ct));
                }
            }, ct);

            // Cancel all other tasks if one fails
            string errorMessage = null;
            while(_monitorTasks.Count > 0)
            {
                Task<Task> t = Task.WhenAny(_monitorTasks);
                Task completedTask = t.Result;
                _monitorTasks.Remove(completedTask);
                if (completedTask.IsFaulted)
                {
                    int eAmount = completedTask.Exception.InnerExceptions.Count;
                    errorMessage = completedTask.Exception.InnerExceptions[eAmount - 1].Message;

                    cts.Cancel();
                }
            }
            // Write error message when all tasks have been canceled
            if (cts.IsCancellationRequested)
                WriteLineWithColor($"\n{errorMessage}\n", ConsoleColor.Red);


            Console.WriteLine($"Total time: {_commander.Stopwatch.Elapsed}");
            Console.ReadLine();
        }

        private static void HandShake(TcpClient client, ref CancellationToken cancellationToken)
        {
            int clientId = _clients.Count;

            _clients.Add(clientId, new Client(client));

            _commander.Stopwatch.Start();
            _monitorTasks.Add(_commander.MonitorTaskMultiplePasswords(_clients[clientId], cancellationToken));
        }
    }
}

