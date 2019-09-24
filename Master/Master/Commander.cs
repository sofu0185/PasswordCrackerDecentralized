using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using static Common.ConsoleEnhancing;

namespace Master
{
    using Common;
    using Newtonsoft.Json;
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
        private Chunks _chunkHandler;

        public Passwords PasswordHandler { get; }
        public Stopwatch Stopwatch { get; }
        public bool EndOfChunks { get => _chunkHandler.EndOfChunks; }

        public Commander()
        {
            PasswordHandler = new Passwords();
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

                // Start stopwatch when server starts to send chunks to client
                Stopwatch.Start();
                try
                {
                    // Send list of users and there hashed passwords before looping over chunks
                    client.StreamWriter.WriteLine(PasswordHandler.UsersAndPasswordsAsString);

                    // keeps sending chunks as long as there are more chunks to iterate over
                    while (!_chunkHandler.EndOfChunks)
                    {
                        // Send chunks to client and read response
                        CommunicateWithClient(client);

                        // Checks if task has been canceled since last chunk was send
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }
                catch(Exception e)
                {
                    throw e;
                }
                // stops the stopwatch and closes the tcp client even if an exception is thrown
                finally
                {
                    Stopwatch.Stop();
                }
                client.NetworkStream.Close();
                client.TcpClient.Close();

            }, cancellationToken);
        }

        /// <summary>
        /// Wirtes one chunk of words to specified client then waits for the client to responde
        /// </summary>
        /// <param name="client"></param>
        /// <param name="cancellationToken"></param>
        private void CommunicateWithClient(Client client)
        {
            WriteToClient(client);
            ReadFromClient(client);
        }

        /// <summary>
        /// Helper method for sending messages to client containing: chunk index, list of users and passwords and a chunk of words from a dictionary.
        /// </summary>
        /// <param name="client"></param>
        private void WriteToClient(Client client)
        {
            client.StreamWriter.WriteLine(_chunkHandler.CurrenntChunkIndex);
            client.StreamWriter.WriteLine(_chunkHandler.GetStringChunk());
        }

        /// <summary>
        /// Helder method for reading incomming messages from client
        /// </summary>
        /// <param name="client"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="OperationCanceledException"></exception>
        private void ReadFromClient(Client client)
        {
            // Try to read a command from client
            // Command can be "passwd" or "Chunk"
            string slaveResponse;
            try
            {
                slaveResponse = client.StreamReader.ReadLine();
            }
            catch (IOException e)
            {
                // If client disconnected create new error message and throw it for error handling in server class
                if (e.InnerException.GetType() == typeof(SocketException))
                    throw new Exception("!!! Slave disconnected !!!");
                else
                    throw e;
            }

            // If client responded with "passwd" then read the cracked passwords
            if (!string.IsNullOrEmpty(slaveResponse) && slaveResponse == "passwd")
            {
                // Print how much time has elapsed since the start
                Console.Write("Minutes elapsed since start: ");
                WriteLineWithColor($"{Stopwatch.Elapsed:%m\\:ss\\:ffff}", ConsoleColor.DarkGray);
    
                // Deserialize incomming passwords
                List<UserInfo> results = JsonConvert.DeserializeObject<List<UserInfo>>(client.StreamReader.ReadLine());
                
                // Loop through the passwords and then print them
                foreach (UserInfo u in results)
                {
                    PasswordHandler.SetPlainTextPassword(u.Id, u.PlainTextPassword);
                    WriteLineWithColor($"\t{u}", ConsoleColor.Yellow);
                }
            }
        }
    }
}
