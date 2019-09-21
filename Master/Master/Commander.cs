using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using static Common.ConsoleEnhancing;

namespace Master
{
    using System.IO;
    using System.Net.Sockets;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles all communication with client.
    /// </summary>
    public class Commander
    {
        private Passwords _userHandler;
        private Chunks _chunkHandler;
        public Stopwatch Stopwatch { get; set; }
        public bool EndOfChunks { get => _chunkHandler.EndOfChunks; }

        public Commander()
        {
            _userHandler = new Passwords();
            _chunkHandler = new Chunks();
            Stopwatch = new Stopwatch();
        }

        /// <summary>
        /// Iterates through all chunks and writes them to the client
        /// </summary>
        /// <param name="client"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task SendAllChunksToClient(Client client, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                // Checks to see if task has been canceled before starting.
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    // keeps sending chunks as long as there are more chunks to iterate over
                    while (!_chunkHandler.EndOfChunks)
                    {
                        CommunicateWithClient(client, cancellationToken);
                    }
                }
                // stops the stopwatch and closes the tcp client even if an exception is thrown
                finally
                {
                    Stopwatch.Stop();
                    client.TcpClient.Close();
                }
            }, cancellationToken);
        }

        /// <summary>
        /// Wirtes one chunk of words to specified client then waits for the client to responde
        /// </summary>
        /// <param name="client"></param>
        /// <param name="cancellationToken"></param>
        private void CommunicateWithClient(Client client, CancellationToken cancellationToken)
        {
            WriteToClient(client);
            ReadFromClient(client, cancellationToken);
        }

        /// <summary>
        /// Helper method for sending messages to client containing: chunk index, list of users and passwords and a chunk of words from a dictionary.
        /// </summary>
        /// <param name="client"></param>
        private void WriteToClient(Client client)
        {
            client.StreamWriter.WriteLine(_chunkHandler.CurrenntChunkIndex);
            client.StreamWriter.WriteLine(_userHandler.UsersAndPasswordsAsString);
            client.StreamWriter.WriteLine(_chunkHandler.GetStringChunk());
        }

        /// <summary>
        /// Helder method for reading incomming messages from client
        /// </summary>
        /// <param name="client"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="OperationCanceledException"></exception>
        private void ReadFromClient(Client client, CancellationToken cancellationToken)
        {
            string slaveResponse;
            try
            {
                slaveResponse = client.StreamReader.ReadLine();
            }
            catch (IOException e)
            {
                if (e.InnerException.GetType() == typeof(SocketException))
                    throw new Exception("!!! Slave disconnected !!!");
                else
                    throw e;
            }

            // Checks if task has been canceled while waiting for a response from slave
            cancellationToken.ThrowIfCancellationRequested();

            // If client responded with "passwd" then read the cracked passwords
            if (!string.IsNullOrEmpty(slaveResponse) && slaveResponse == "passwd")
            {
                Console.Write("Minutes elapsed since start: ");
                WriteLineWithColor($"{Stopwatch.Elapsed:%m\\:ss\\:ffff}", ConsoleColor.DarkGray);

                int numberOfPassCracked = int.Parse(client.StreamReader.ReadLine());
                for (int i = 0; i < numberOfPassCracked; i++)
                {
                    WriteLineWithColor($"\t{client.StreamReader.ReadLine()}", ConsoleColor.Yellow);
                }
            }
        }
    }
}
