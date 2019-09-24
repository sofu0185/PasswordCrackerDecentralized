using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using Common;

using static Common.ConsoleEnhancing;

namespace Slave
{
    class Program
    {
        public const int PORT = 6789;
        public static string IPADDRESS = "localhost";
        private static readonly int LOGICALCORES = Environment.ProcessorCount;

        static void Main(string[] args)
        {
            // if there is one argument set it to ip address
            if(args.Length > 0) IPADDRESS = args[0];

            Console.Write($"Number Of Logical Processors: ");
            WriteLineWithColor($"{LOGICALCORES}", ConsoleColor.DarkGray);
            Console.Write($"Server located at: ");
            WriteLineWithColor($"{IPADDRESS}:{PORT}\n", ConsoleColor.DarkGray);

            // Connecto to server/master
            TcpClient clientSocket = new TcpClient(IPADDRESS, PORT);

            using (NetworkStream ns = clientSocket.GetStream())
            {
                StreamWriter sw = new StreamWriter(ns);
                StreamReader sr = new StreamReader(ns);
                sw.AutoFlush = true;

                // Read passwords and there hashed passwords once
                string serializedUsers = sr.ReadLine();
                List<UserInfo> usersAndHashedPasswords = JsonConvert.DeserializeObject<List<UserInfo>>(serializedUsers);

                // Convert all hashed passwords into byte arrays for comparison
                List<(UserInfo, byte[])> usersAndHashedPassAsByteArray = new List<(UserInfo, byte[])>();
                foreach (UserInfo userInfo in usersAndHashedPasswords)
                {
                    usersAndHashedPassAsByteArray.Add((userInfo, Convert.FromBase64String(userInfo.HashedPassword)));
                }

                while (clientSocket.Connected)
                {

                    string chunkId = null;
                    string[] dicChunk = null;
                    try
                    {
                        chunkId = sr.ReadLine();
                        dicChunk = JsonConvert.DeserializeObject<string[]>(sr.ReadLine() ?? "");
                    }
                    catch (IOException e)
                    {
                        if (e.InnerException.GetType() == typeof(SocketException))
                        {
                            WriteLineWithColor($"Master closed connection due to an error!\n", ConsoleColor.Red);
                            break;
                        }
                        else
                            throw e;
                    }

                    if (chunkId != null)
                    {
                        List<string[]> subChunks = SplitIntoSubchunks(dicChunk, LOGICALCORES);

                        Console.Write($"Chunk [");
                        WriteWithColor(chunkId, ConsoleColor.DarkGray);
                        Console.WriteLine("] and hashed password received and processed.");

                        (bool isAnyCracked, List<UserInfo> crackedPasswords) crackingResult = CheckMultipleWordsAtOnce(subChunks, usersAndHashedPassAsByteArray);

                        Console.Write("Did any passwords match in chunk? ");
                        ConsoleColor color = crackingResult.isAnyCracked ? ConsoleColor.Green : ConsoleColor.Red;
                        WriteLineWithColor(crackingResult.isAnyCracked, color);

                        ResponseToMaster(sw, crackingResult.isAnyCracked, crackingResult.crackedPasswords);

                        Console.WriteLine();
                    }
                    // if chunkId is null server has probably closed the connection
                    else
                    {
                        WriteLineWithColor($"Program completed successfully\n", ConsoleColor.Blue);
                        clientSocket.Close();
                    }
                }
            }
        }

        private static void ResponseToMaster(StreamWriter sw, bool isAnyCracked, List<UserInfo> crackedPasswords)
        {
            // response "passwd" to server if any passwords was cracked
            if (isAnyCracked)
            {
                foreach (UserInfo userAndPass in crackedPasswords)
                {
                    WriteLineWithColor($"\t{userAndPass}", ConsoleColor.Yellow);
                }

                sw.WriteLine("passwd");
                sw.WriteLine(JsonConvert.SerializeObject(crackedPasswords));
            }
            // else ask for new chunks
            else
            {
                sw.WriteLine("Chunk");
            }

        }

        /// <summary>
        /// Split a chunk of words into a certain number of sub chunks
        /// </summary>
        /// <param name="chunk">The list/chunk of words to be be split</param>
        /// <param name="subchunkAmount">How many sub chunks there should be</param>
        /// <returns></returns>
        private static List<string[]> SplitIntoSubchunks(Span<string> chunk, int subchunkAmount)
        {
            List<string[]> result = new List<string[]>();

            // Get length of the sub chunks
            int subchunkLength = chunk.Length / subchunkAmount;

            // Create the calculated amount of sub chunks
            for(int i = 0; i < subchunkAmount; i++)
            {
                // Claculate where the sub chunk should start in the large chunk of words
                int sliceStart = i * subchunkLength;

                // If last sub chunk add all remaining words to it
                if (i == subchunkAmount - 1)
                    result.Add(chunk.Slice(sliceStart).ToArray());
                else
                    result.Add(chunk.Slice(sliceStart, subchunkLength).ToArray());
            }
            return result;
        }

        /// <summary>
        /// Check all sub chunks for matching passwords simultaneously on seprate threads
        /// </summary>
        /// <param name="subChunks"></param>
        /// <param name="usersAndHashedPasswords"></param>
        /// <returns></returns>
        private static (bool, List<UserInfo>) CheckMultipleWordsAtOnce(List<string[]> subChunks, List<(UserInfo, byte[])> usersAndHashedPasswords)
        {
            List<UserInfo> crackedUsers = new List<UserInfo>();
            Parallel.ForEach(subChunks, subChunk =>
            {
                Cracking c = new Cracking();
                // Check chunk of words for password matches
                List<UserInfo> result = c.CheckWords(subChunk, usersAndHashedPasswords);
                crackedUsers.AddRange(result);
            });

            return (crackedUsers.Count > 0, crackedUsers);
        }
    }
}
