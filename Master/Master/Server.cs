using System;
using System.Collections.Generic;
using System.Text;

namespace Master
{
    using System.Collections.Concurrent;
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

            // Wait for other clients in seperate thread
            Task runServerTask = Task.Run(() =>
            {
                while (!_commander.EndOfChunks || cts.IsCancellationRequested)
                {
                    try
                    {
                        TcpClient client = _server.AcceptTcpClient();
                        Task.Run(() => HandShake(client, cts));
                    }
                    catch(SocketException e)
                    {
                        if (e.ErrorCode != 10004)
                            throw e;
                    }
                }
            });

            // Wait
            try
            {
                runServerTask.Wait(cts.Token);
            }
            catch(Exception e)
            {
                string errorMessage = e.Message;

                // Wait for all task to be canceled
                foreach (Task t in _monitorTasks)
                {
                    if (t.IsFaulted)
                    {
                        // Gets error message
                        int eAmount = t.Exception.InnerExceptions.Count;
                        errorMessage = t.Exception.InnerExceptions[eAmount - 1].Message;
                    }
                    else
                    {
                        try
                        {
                            if (!t.IsCompleted)
                                t.Wait();
                        }
                        catch (AggregateException oce)
                        {
                            if (!(oce.InnerException is TaskCanceledException))
                                throw e;
                        }
                    }
                }

                // Write error message when all tasks have been canceled
                WriteLineWithColor($"\n{errorMessage}", ConsoleColor.Red);
            }


            Console.Write($"\nTotal time: ");
            WriteLineWithColor(_commander.Stopwatch.Elapsed, ConsoleColor.Yellow);
        }

        /// <summary>
        /// Adds connected client to client dictionary and starts a communication task.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="cancellationToken"></param>
        private static void HandShake(TcpClient client, CancellationTokenSource cts)
        {
            int clientId = _clients.Count;
            _clients.Add(clientId, new Client(client));

            Task communicationTask = _commander.SendAllChunksToClient(_clients[clientId], cts.Token);

            // If task fails due to an error cancel all other tasks 
            communicationTask.ContinueWith(completedTask => cts.Cancel(), TaskContinuationOptions.OnlyOnFaulted);
            communicationTask.ContinueWith(completedTask => 
            {
                Task.WhenAll(_monitorTasks).Wait();
                _server.Stop();
                
            }, TaskContinuationOptions.OnlyOnRanToCompletion);

            _monitorTasks.Add(communicationTask);
        }
    }
}

