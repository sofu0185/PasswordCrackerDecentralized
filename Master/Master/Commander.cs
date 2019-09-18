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
        private static Password pass = new Password();
        private static List<Task> monitorTasks = new List<Task>();
        private static int _index = 1;
        private static Stopwatch stopwatch = new Stopwatch();
        private static Dictionary dict = new Dictionary();
        
        public static void HandShake(TcpClient client)
        {
            NetworkStream networkStream = client.GetStream();
            stopwatch.Start();
            StreamReader streamReader = new StreamReader(networkStream);
            StreamWriter streamWriter = new StreamWriter(networkStream);
            streamWriter.AutoFlush = true;
            _Clients.Add(_index, new Client(client, networkStream, streamWriter, streamReader));
            monitorTasks.Add(MonitorTask(_Clients[_index], _index));
            _index++;
        }

        public static Task MonitorTask(Client c, int index)
        {
            return Task.Run((() =>
                             {
                                 lock (dict)
                                 {
                                     SendNext(c);
                                 }
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
                                         lock (dict)
                                         {
                                             dict.ResetCount();
                                             SendNext(c);
                                         }
                                         //Chat(index);
                                     }
                                     else if (s == "Chunk")
                                     {
                                         lock (dict)
                                         {
                                             SendNext(c);
                                         }
                                     }
                                 }
                             }));
        }

        public static void Chat(int index)
        {
            foreach (var client in _Clients)
            {
                if (index != client.Key)
                {
                    client.Value.StreamWriter.WriteLine("password");
                    SendNext(client.Value); // should write new password
                }
            }
        }

        public static void SendNext(Client c)
        {
            c.StreamWriter.WriteLine(pass.GetPass());
            c.StreamWriter.WriteLine(dict.ChunkToString());
        }
    }
}
