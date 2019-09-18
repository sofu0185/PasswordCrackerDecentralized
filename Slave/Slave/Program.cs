using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

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
                Task <ValueTuple<bool, string>> crackingTask = null;

                

                while (clientSocket.Connected)
                {
                    CancellationTokenSource crackingTokenSource = new CancellationTokenSource();
                    CancellationToken cct = crackingTokenSource.Token;

                    string chunkId = null;
                    string hashedPassword = null;
                    List<string> dicChunk = null;
                    try
                    {
                        chunkId = sr.ReadLine();
                        hashedPassword = sr.ReadLine();
                        string allWords = sr.ReadLine();

                        dicChunk = allWords.Split(',').ToList();
                    }
                    catch (IOException e)
                    {
                        if (e.InnerException.GetType() != typeof(SocketException))
                            throw e;
                    }

                    Console.Write($"Chunk [");
                    WriteWithColor(chunkId, ConsoleColor.DarkGray);
                    Console.Write("] and hashed password recived:\n\t");
                    WriteLineWithColor(hashedPassword, ConsoleColor.Gray);

                    // Can return success or newChunk
                    crackingTask = Task<ValueTuple<bool, string>>.Run(() => cracking.CheckWordsWithVariations(dicChunk, hashedPassword), cct);
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
                            sw.WriteLine(crackingTask.Result.Item2);

                            WriteLineWithColor(crackingTask.Result.Item2, ConsoleColor.Yellow);
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
