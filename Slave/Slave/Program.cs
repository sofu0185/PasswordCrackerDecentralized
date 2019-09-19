using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

namespace Slave
{
    class Program
    {
        public const int PORT = 6789;
        public const string IPADDRESS = "localhost";
        
        static void Main(string[] args)
        {
            Cracking cracking = new Cracking();

            TcpClient clientSocket = new TcpClient(IPADDRESS, PORT);

            using (NetworkStream ns = clientSocket.GetStream())
            {
                StreamWriter sw = new StreamWriter(ns);
                StreamReader sr = new StreamReader(ns);
                BinaryFormatter formatter = new BinaryFormatter();


                Task<(bool, List<(string, string)>)> crackingTask = null;

                while (clientSocket.Connected)
                {
                    CancellationTokenSource crackingTokenSource = new CancellationTokenSource();
                    CancellationToken cct = crackingTokenSource.Token;

                    string chunkId = "";
                    List<(string, string)> hashedPasswords = new List<(string, string)>();
                    List<string> dicChunk = new List<string>();
                    try
                    {
                        //chunkId = (string)formatter.Deserialize(ns);
                        chunkId = sr.ReadLine();

                        //string allPasswords = (string)formatter.Deserialize(ns);
                        string allPasswords = sr.ReadLine();
                        foreach (string user in allPasswords.Split(','))
                        {
                            string[] tempSplit = user.Split(':');
                            hashedPasswords.Add((tempSplit[0], tempSplit[1]));
                        }

                        //dicChunk = (List<string>)formatter.Deserialize(ns);
                        string allWords = sr.ReadLine();
                        dicChunk = JsonConvert.DeserializeObject<List<string>>(allWords);

                    }
                    catch (IOException e)
                    {
                        if (e.InnerException.GetType() != typeof(SocketException))
                            throw e;
                    }

                    Console.Write($"Chunk [");
                    WriteWithColor(chunkId, ConsoleColor.DarkGray);
                    Console.WriteLine("] and hashed password received.");
                    //WriteLineWithColor(hashedPassword, ConsoleColor.Gray);

                    // Can return success or newChunk
                    crackingTask = Task.Run(() => cracking.CheckWords(dicChunk, hashedPasswords), cct);

                    //Task t = Task.Run(() =>
                    //{
                    //    string extraMessage = sr.ReadLine();
                    //    if (extraMessage == "password")
                    //    {
                    //        crackingTokenSource.Cancel();
                    //    }

                    //});
                    crackingTask.Wait();
                    //tcpTokenSource.Cancel();
                

                    if (crackingTask.IsCompletedSuccessfully)
                    {
                        Console.Write("Did any passwords match in chunk? ");
                        ConsoleColor color = crackingTask.Result.Item1 ? ConsoleColor.Green : ConsoleColor.Red;
                        WriteLineWithColor(crackingTask.Result.Item1, color);

                        sw.AutoFlush = true;
                        if (crackingTask.Result.Item1)
                        {
                            sw.WriteLine("passwd");
                            sw.WriteLine(crackingTask.Result.Item2.Count);
                            foreach ((string name, string pass) userAndPass in crackingTask.Result.Item2)
                            {
                                string userAndPassAsString = $"{userAndPass.name}: {userAndPass.pass}";
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

        public static void WriteWithColor(object message, ConsoleColor color)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(message);
            Console.ForegroundColor = originalColor;
        }
        public static void WriteLineWithColor(object message, ConsoleColor color)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = originalColor;
        }

    }
}
