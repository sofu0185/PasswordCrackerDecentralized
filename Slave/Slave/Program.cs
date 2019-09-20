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

            Cracking cracking = new Cracking();

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
                        
                        string allWords = sr.ReadLine();
                        dicChunk = JsonConvert.DeserializeObject<List<string>>(allWords);

                    }
                    catch (IOException e)
                    {
                        if (e.InnerException.GetType() != typeof(SocketException))
                            throw e;
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
            List<Task<List<UserInfo>>> _crackingTasks = new List<Task<List<UserInfo>>>();

            Task<List<UserInfo>> MethodWithParameter(int b)
            {
                Task<List<UserInfo>> a = new Task<List<UserInfo>>((() =>
                {
                    Cracking c = new Cracking();
                    return c.CheckWords(subChunks[b], usersAndHashedPasswords).Item2;
                }));
                a.Start();
                return a;
            }
            for (int i = 0; i < subChunks.Count; i++)
            {
                Task<List<UserInfo>> a = MethodWithParameter(i);

                _crackingTasks.Add(a);
            }

            List<List<UserInfo>> _crackingResults = new List<List<UserInfo>>();

            foreach (var t in _crackingTasks)
            {
                t.Wait();
                _crackingResults.Add(t.Result);
            }
            List<UserInfo> crackedUsers = new List<UserInfo>();
            bool succes = false;
            foreach (var result in _crackingResults)
            {
                if (result.Count > 0)
                {
                    succes = true;
                    foreach (var userInfo in result)
                    {
                        crackedUsers.Add(userInfo);
                    }
                }
            }

            return (succes, crackedUsers);
        }
    }
}
