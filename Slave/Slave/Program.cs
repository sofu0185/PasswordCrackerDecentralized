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
        private static int _logicalCores = Environment.ProcessorCount;
        private static List<List<UserInfo>> _crackingResults;
        private static List<Task<List<UserInfo>>> _crackingTasks;
        static void Main(string[] args)
        {
            Console.WriteLine("Number Of Logical Processors: {0}", _logicalCores);

            Cracking cracking = new Cracking();

            TcpClient clientSocket = new TcpClient(IPADDRESS, PORT);

            using (NetworkStream ns = clientSocket.GetStream())
            {
                StreamWriter sw = new StreamWriter(ns);
                StreamReader sr = new StreamReader(ns);
                sw.AutoFlush = true;
                //BinaryFormatter formatter = new BinaryFormatter();

                while (clientSocket.Connected)
                {
                    string chunkId = null;
                    List<UserInfo> usersAndHashedPasswords = null;
                    List<string> dicChunk = null;
                    List<List<string>> listDicChunks = new List<List<string>>();
                    try
                    {
                        //chunkId = (string)formatter.Deserialize(ns);
                        chunkId = sr.ReadLine();

                        //string allPasswords = (string)formatter.Deserialize(ns);
                        usersAndHashedPasswords = JsonConvert.DeserializeObject<List<UserInfo>>(sr.ReadLine());
                        
                        //dicChunk = (List<string>)formatter.Deserialize(ns);
                        string allWords = sr.ReadLine();
                        dicChunk = JsonConvert.DeserializeObject<List<string>>(allWords);

                    }
                    catch (IOException e)
                    {
                        if (e.InnerException.GetType() != typeof(SocketException))
                            throw e;
                    }

                    int count = 0;
                    List<string> chunk = new List<string>();
                    foreach (string s in dicChunk)
                    {
                        if (count == dicChunk.Count / _logicalCores)
                        {
                            listDicChunks.Add(chunk);
                            chunk = new List<string>();
                            count = 0;
                        }
                        chunk.Add(s);
                        count++;
                    }
                    listDicChunks.Add(chunk);

                    Console.Write($"Chunk [");
                    WriteWithColor(chunkId, ConsoleColor.DarkGray);
                    Console.WriteLine("] and hashed password received.");
                    //WriteLineWithColor(hashedPassword, ConsoleColor.Gray);
                    _crackingResults = new List<List<UserInfo>>();
                    _crackingTasks = new List<Task<List<UserInfo>>>();
                    Task<List<UserInfo>> MethodWithParameter(int b)
                    {
                        Task<List<UserInfo>> a = new Task<List<UserInfo>>((() =>
                                                                           {
                                                                               Cracking c = new Cracking();
                                                                               return c
                                                                                      .CheckWords(listDicChunks[b],
                                                                                                  usersAndHashedPasswords)
                                                                                      .Item2;}));
                        a.Start();
                        return a;
                    }
                    for (int i = 0; i < listDicChunks.Count; i++)
                    {
                        Task<List<UserInfo>> a = MethodWithParameter(i);
                        
                        _crackingTasks.Add(a);
                        
                    }

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

                    (bool isAnyCracked, List<UserInfo> userInfo) crackingResult = (succes, crackedUsers);
                    
                    Console.Write("Did any passwords match in chunk? ");
                    ConsoleColor color = crackingResult.isAnyCracked ? ConsoleColor.Green : ConsoleColor.Red;
                    WriteLineWithColor(crackingResult.isAnyCracked, color);

                    // Can return success or newChunk
                    if (crackingResult.isAnyCracked)
                    {
                        sw.WriteLine("passwd");
                        sw.WriteLine(crackingResult.userInfo.Count);
                        foreach (UserInfo userAndPass in crackingResult.userInfo)
                        {
                            string userAndPassAsString = $"{userAndPass.Username}: {userAndPass.PlainTextPassword}";
                            sw.WriteLine(userAndPassAsString);
                            WriteLineWithColor($"\t{userAndPassAsString}", ConsoleColor.Yellow);
                        }
                    }
                    else
                    {
                        sw.WriteLine("Chunk");
                    }

                    Console.WriteLine();
                }
            }
        }
    }
}
