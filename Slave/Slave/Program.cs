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

                while (clientSocket.Connected)
                {
                    string chunkId = "";
                    List<UserInfo> usersAndHashedPasswords = new List<UserInfo>();
                    List<string> dicChunk = new List<string>();
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

                    Console.Write($"Chunk [");
                    WriteWithColor(chunkId, ConsoleColor.DarkGray);
                    Console.WriteLine("] and hashed password received.");
                    //WriteLineWithColor(hashedPassword, ConsoleColor.Gray);

                    // Can return success or newChunk
                    (bool isAnyCracked, List<UserInfo> userInfo) crackingResult = cracking.CheckWords(dicChunk, usersAndHashedPasswords);
                    
                    Console.Write("Did any passwords match in chunk? ");
                    ConsoleColor color = crackingResult.isAnyCracked ? ConsoleColor.Green : ConsoleColor.Red;
                    WriteLineWithColor(crackingResult.isAnyCracked, color);

                    sw.AutoFlush = true;
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
