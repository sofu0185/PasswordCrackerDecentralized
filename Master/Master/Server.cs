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
        /// <summary>
        /// Contains all connected clients, key is the order clients connected to server.
        /// </summary>
        private static Dictionary<int, Client> _clients;
        /// <summary>
        /// A list of all currently running communication tasks.
        /// </summary>
        private static List<Task> _monitorTasks;
        private static Commander _commander;
        private static TcpListener _server;

        /// <summary>
        /// Initialize server with core variables.
        /// </summary>
        private static void Start()
        {
            Console.WriteLine("Starting server...");
            _clients = new Dictionary<int, Client>();
            _monitorTasks = new List<Task>();
            _commander = new Commander();
            _server = TcpListener.Create(Constants.TCP_SERVER_PORT);

            _server.Start();
            Console.WriteLine();
        }

        /// <summary>
        /// Runs the cracking server. Craking starts when a least one client connects.
        /// </summary>
        public static void RunServer()
        {
            Start();

            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken ct = cts.Token;

            // Wait for first client to connect
            TcpClient initialClient = _server.AcceptTcpClient();
            HandShake(initialClient, ref ct);

            // Start stopwatch when client is fully connected
            _commander.Stopwatch.Start();

            // Wait for other clients in seperate thread
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
            while (_monitorTasks.Count > 0)
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

            Console.WriteLine($"\nTotal time: {_commander.Stopwatch.Elapsed}");
        }

        /// <summary>
        /// Adds connected client to client dictionary and starts a communication task.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="cancellationToken"></param>
        private static void HandShake(TcpClient client, ref CancellationToken cancellationToken)
        {
            int clientId = _clients.Count;
            _clients.Add(clientId, new Client(client));
            _monitorTasks.Add(_commander.SendAllChunksToClient(_clients[clientId], cancellationToken));
        }
    }
}

