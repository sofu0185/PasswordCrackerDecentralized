using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Master
{
    using System.IO;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    public static class Commander
    {
        private static Dictionary<int, Client> _Clients = new Dictionary<int, Client>();
        private static Passwords pass = new Passwords();
        private static List<Task> monitorTasks = new List<Task>();
        private static int _index = 1;
        private static Stopwatch stopwatch = new Stopwatch();
        private static Chunks dict;

        public static void Start()
        {
            Console.WriteLine("Starting server");
            dict = new Chunks();
        }
        
        public static void HandShake(TcpClient client)
        {
            client.NoDelay = true;
            NetworkStream networkStream = client.GetStream();
            stopwatch.Start();
            StreamReader streamReader = new StreamReader(networkStream);
            StreamWriter streamWriter = new StreamWriter(networkStream);
            streamWriter.AutoFlush = true;
            _Clients.Add(_index, new Client(client, networkStream, streamWriter, streamReader));
            //monitorTasks.Add(MonitorTask(_Clients[_index], _index));
            monitorTasks.Add(MonitorTask(_Clients[_index], _index));
            _index++;
        }

        public static Task MonitorTask(Client c, int index)
        {
            return Task.Run(() => 
                            {
                                 SendNext(c);
                                 while (true)
                                 {
                                     string s = c.StreamReader.ReadLine();
                                     if (String.IsNullOrEmpty(s))
                                     {
                                         
                                     }
                                     else if (s.Contains("passwd"))
                                     {
                                         Console.WriteLine(s);
                                         Console.WriteLine(pass.GetName() + ": " + c.StreamReader.ReadLine());
                                         if (pass.NextPass())
                                         {
                                             stopwatch.Stop();
                                             Console.WriteLine(stopwatch.Elapsed);
                                             Console.ReadLine();
                                         }
                                         Console.WriteLine(stopwatch.Elapsed);
                                         
                                             dict.ResetCount();
                                             SendNext(c);
                                         
                                         //Chat(index);
                                     }
                                     else if (s == "Chunk")
                                     {
                                         if (dict.CurrenntChunkIndex == dict.ChunkList.Count)
                                             {
                                                 Console.WriteLine("Pass not found");
                                                 if (pass.NextPass())
                                                 {
                                                     stopwatch.Stop();
                                                     Console.WriteLine(stopwatch.Elapsed);
                                                     Console.ReadLine();
                                                 }
                                             }
                                             SendNext(c);
                                         
                                     }
                                 }
                             });
        }
        public static Task MonitorTaskMultiplePasswords(Client c, int index)
        {
            return Task.Run(() =>
            {
                while (!dict.EndOfChunks)
                {
                    SendNextMultiplePasswords(c);

                    string slaveResponse = c.StreamReader.ReadLine();
                    if (!String.IsNullOrEmpty(slaveResponse) && slaveResponse == "passwd")
                    {
                        int numberOfPassCracked = int.Parse(c.StreamReader.ReadLine());
                        for(int i = 0; i < numberOfPassCracked; i++)
                        {
                            Console.WriteLine(c.StreamReader.ReadLine());
                        }
                        
                        Console.WriteLine(stopwatch.Elapsed);
                    }
                }
                stopwatch.Stop();
                Console.WriteLine(stopwatch.Elapsed);
                Console.ReadLine();

            });
        }

        public static void SendNext(Client c)
        {
            c.StreamWriter.WriteLine(dict.CurrenntChunkIndex);
            c.StreamWriter.WriteLine(pass.GetPass());
            Task t = new Task(() => c.StreamWriter.WriteLine(dict.GetNextChunk()));
            t.Start();
        }

        public static void SendNextMultiplePasswords(Client c)
        {
            c.StreamWriter.WriteLine(dict.CurrenntChunkIndex);
            c.StreamWriter.WriteLine(pass.UsersAndPasswordsAsString);
            c.StreamWriter.WriteLine(dict.GetChunk());
        }
    }
}
