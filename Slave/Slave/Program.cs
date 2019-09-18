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
        public const int PORT = 0;
        public const string IPADDRESS = "";
        
        static void Main(string[] args)
        {
            Cracking cracking = new Cracking();

            TcpClient clientSocket = new TcpClient(IPADDRESS, PORT);

            using (NetworkStream ns = clientSocket.GetStream())
            {
                Task<ValueTuple<bool, string>> crackingTask = null;

                

                while (clientSocket.Connected)
                {
                    //CancellationTokenSource tcpTokenSource = new CancellationTokenSource();
                    //CancellationToken tct = tcpTokenSource.Token;

                    CancellationTokenSource crackingTokenSource = new CancellationTokenSource();
                    CancellationToken cct = crackingTokenSource.Token;
                    using (StreamReader sr = new StreamReader(ns))
                    {
                        string hashedPassword = null;
                        List<string> dicChunk = null;
                        try
                        {
                            hashedPassword = sr.ReadLine();
                            string allWords = sr.ReadLine();

                            dicChunk = allWords.Split(',').ToList();
                        }
                        catch (IOException e)
                        {
                            if (e.InnerException.GetType() != typeof(SocketException))
                                throw e;
                        }

                        // Can return success or newChunk
                        crackingTask = Task<ValueTuple<bool, string>>.Run(() => cracking.CheckWordsWithVariations(dicChunk, hashedPassword), cct);
                        Task t = Task.Run(() =>
                        {
                            string extraMessage = sr.ReadLine();
                            if (extraMessage == "password")
                            {
                                crackingTokenSource.Cancel();
                            }

                        });
                        crackingTask.Wait();
                        //tcpTokenSource.Cancel();
                    }

                    if (crackingTask.IsCompletedSuccessfully)
                    {
                        using (StreamWriter sw = new StreamWriter(ns))
                        {
                            sw.AutoFlush = true;
                            if (crackingTask.Result.Item1)
                            {
                                sw.WriteLine("passwd");
                                sw.WriteLine(crackingTask.Result.Item2);
                            }
                            else if(crackingTask.Result.Item1)
                            {
                                sw.WriteLine("Chunk");
                            }
                        }                        
                    }
                }
            }
        }

        


    }
}
