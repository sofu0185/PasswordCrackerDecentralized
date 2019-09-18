using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Slave
{
    class Program
    {
        public const int PORT = 0;
        public const string IPADDRESS = "";
        
        static void Main(string[] args)
        {
            TcpClient clientSocket = new TcpClient(IPADDRESS, PORT);

            using (NetworkStream ns = clientSocket.GetStream())
            {
                Task<string> crackingTask = null;

                

                while (clientSocket.Connected)
                {
                    //CancellationTokenSource tcpTokenSource = new CancellationTokenSource();
                    //CancellationToken tct = tcpTokenSource.Token;

                    Task t = Task.Run(() =>
                    {
                        CancellationTokenSource crackingTokenSource = new CancellationTokenSource();
                        CancellationToken cct = crackingTokenSource.Token;
                        using (StreamReader sr = new StreamReader(ns))
                        {
                            string hashedPassword;
                            List<string> dicChunk;
                            try
                            {
                                hashedPassword = sr.ReadLine();

                                string allWords = sr.ReadLine();
                                dicChunk = new List<string>(allWords.Split(','));
                            }
                            catch (IOException e)
                            {
                                if (e.InnerException.GetType() != typeof(SocketException))
                                    throw e;
                            }

                            // Can return success or newChunk
                            crackingTask = Task<string>.Run(() => {
                                return "";

                            }, cct);

                            string extraMessage = sr.ReadLine();
                            if (extraMessage == "password")
                            {
                                crackingTokenSource.Cancel();
                            }
                        }
                    });

                    crackingTask.Wait();
                    //tcpTokenSource.Cancel();

                    if (crackingTask.IsCompletedSuccessfully)
                    {
                        using (StreamWriter sw = new StreamWriter(ns))
                        {
                            switch (crackingTask.Result)
                            {
                                case "success":
                                    sw.WriteLine("password");
                                    break;
                                case "newChunk":
                                    sw.WriteLine("chunck");
                                    break;
                            }
                            sw.Flush();
                        }                        
                    }
                }
            }
        }

        


    }
}
