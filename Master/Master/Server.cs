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

            // Run AccepTcpClient in another task for cancellation purposes
            Task runServerTask = Task.Run(() =>
            {
                // Let new users connect as long as there are more chunks avalible
                while (!_commander.EndOfChunks || cts.IsCancellationRequested)
                {
                    try
                    {
                        // Wait for TcpClient to connect
                        TcpClient client = _server.AcceptTcpClient();
                        // Finish Connect in another thread
                        Task.Run(() => HandShake(client, cts));
                    }
                    catch(SocketException e)
                    {
                        // Dont throw exception if error code matches 10004
                        if (e.ErrorCode == 10004) ;
                        else
                            throw e;
                    }
                }
            });

            // Waiting for the previous task
            try
            {
                runServerTask.Wait(cts.Token);
            }
            // If a task was canceled find and wait for all other task to be canceled
            // and then print message error for the error that caused the tasks to be canceled
            catch (OperationCanceledException e)
            {
                string errorMessage = e.Message;

                // Loop through all monitored tasks
                foreach (Task t in _monitorTasks)
                {
                    // If task faulted due to an error get error message
                    if (t.IsFaulted)
                    {
                        // Gets lowest error message in chain of aggregate exceptions
                        int eAmount = t.Exception.InnerExceptions.Count;
                        errorMessage = t.Exception.InnerExceptions[eAmount - 1].Message;
                    }
                    else
                    {
                        try
                        {
                            // Try waiting for a non completed task
                            if (!t.IsCompleted)
                                t.Wait();
                        }
                        catch (AggregateException oce)
                        {
                            // Ignore error if task was cenceled
                            if (!(oce.InnerException is TaskCanceledException))
                                throw e;
                        }
                    }
                }

                // Write error message when all tasks have been canceled
                WriteLineWithColor($"\n{errorMessage}", ConsoleColor.Red);
            }
            catch (Exception e)
            {
                string errorMessage = e.Message;
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
            // Get id for client
            int clientId = _clients.Count;
            // Create add connected client to list of clients
            _clients.Add(clientId, new Client(client));

            // Start sending chunks to the client in a new task
            // Return a task for monitoring
            Task communicationTask = _commander.SendAllChunksToClient(_clients[clientId], cts.Token);

            // If task fails due to an error cancel all other tasks 
            communicationTask.ContinueWith(completedTask => cts.Cancel(), TaskContinuationOptions.OnlyOnFaulted);

            // If task completed successfully wait for all other tasks to complete then close server/TcpListener
            communicationTask.ContinueWith(completedTask => 
            {
                Task.WhenAll(_monitorTasks).Wait();
                _server.Stop();
                
            }, TaskContinuationOptions.OnlyOnRanToCompletion);

            // Add task to list
            _monitorTasks.Add(communicationTask);
        }
    }
}

