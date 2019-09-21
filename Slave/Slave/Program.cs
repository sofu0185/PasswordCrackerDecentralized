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
        public const string IPADDRESS = "localhost";
        private static readonly int LOGICALCORES = Environment.ProcessorCount;

        static void Main(string[] args)
        {
            Console.WriteLine("Number Of Logical Processors: {0}", LOGICALCORES);
            
            TcpClient clientSocket = new TcpClient(IPADDRESS, PORT);
            using (NetworkStream ns = clientSocket.GetStream())
            {
                StreamWriter sw = new StreamWriter(ns);
                StreamReader sr = new StreamReader(ns);
                sw.AutoFlush = true;

                while (clientSocket.Connected)
                {
                    string chunkId = null;
                    List<UserInfo> usersAndHashedPasswords = null;
                    List<string> dicChunk = null;
                    try
                    {
                        chunkId = sr.ReadLine();

                        usersAndHashedPasswords = JsonConvert.DeserializeObject<List<UserInfo>>(sr.ReadLine());
                        
                        dicChunk = JsonConvert.DeserializeObject<List<string>>(sr.ReadLine());

                    }
                    catch (IOException e)
                    {
                        if (e.InnerException.GetType() != typeof(SocketException))
                            throw e;
                        else
                            WriteLineWithColor($"\nMaster closed connection due to an error!", ConsoleColor.Red);
                    }

                    if (!string.IsNullOrWhiteSpace(chunkId))
                    {
                        List<List<string>> subChunks = SplitIntoSubchunks(dicChunk, LOGICALCORES);

                        Console.Write($"Chunk [");
                        WriteWithColor(chunkId, ConsoleColor.DarkGray);
                        Console.WriteLine("] and hashed password received and processed.");

                        (bool isAnyCracked, List<UserInfo> crackedPasswords) crackingResult = CheckMultipleWordsAtOnce(subChunks, usersAndHashedPasswords);

                        Console.Write("Did any passwords match in chunk? ");
                        ConsoleColor color = crackingResult.isAnyCracked ? ConsoleColor.Green : ConsoleColor.Red;
                        WriteLineWithColor(crackingResult.isAnyCracked, color);

                        ResponseToMaster(sw, crackingResult.isAnyCracked, crackingResult.crackedPasswords);
                    }
                }
            }
        }

        private static void ResponseToMaster(StreamWriter sw, bool isAnyCracked, List<UserInfo> crackedPasswords)
        {
            // response "passwd" to server if any passwords was cracked
            if (isAnyCracked)
            {
                sw.WriteLine("passwd");
                sw.WriteLine(crackedPasswords.Count);
                foreach (UserInfo userAndPass in crackedPasswords)
                {
                    string userAndPassAsString = $"{userAndPass.Username}: {userAndPass.PlainTextPassword}";
                    sw.WriteLine(userAndPassAsString);
                    WriteLineWithColor($"\t{userAndPassAsString}", ConsoleColor.Yellow);
                }
            }
            // else ask for new chunks
            else
            {
                sw.WriteLine("Chunk");
            }

            Console.WriteLine();
        }

        private static List<List<string>> SplitIntoSubchunks(List<string> chunk, int subchunkAmount)
        {
            List<List<string>> result = new List<List<string>>();
            int count = 0;
            List<string> subChunk = new List<string>();
            foreach (string s in chunk)
            {
                if (count == chunk.Count / LOGICALCORES)
                {
                    result.Add(subChunk);
                    subChunk = new List<string>();
                    count = 0;
                }
                subChunk.Add(s);
                count++;
            }
            result.Add(subChunk);
            return result;
        }

        private static (bool, List<UserInfo>) CheckMultipleWordsAtOnce(List<List<string>> subChunks, List<UserInfo> usersAndHashedPasswords)
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
