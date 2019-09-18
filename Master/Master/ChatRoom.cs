using System;
using System.Collections.Generic;
using System.Text;

namespace Master
{
    using System.IO;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    public static class ChatRoom
    {
        private static Dictionary<string, Client> _Clients     = new Dictionary<string, Client>();
        private static List<Task>                 monitorTasks = new List<Task>();
        public static void HandShake(TcpClient client)
        {
            NetworkStream networkStream = client.GetStream();
            StreamReader streamReader = new StreamReader(networkStream);
            StreamWriter streamWriter = new StreamWriter(networkStream);
            streamWriter.AutoFlush = true;
            streamWriter.WriteLine("Input name");
            string name = streamReader.ReadLine();
            _Clients.Add(name, new Client(client, networkStream, streamWriter, streamReader));
            monitorTasks.Add(MonitorTask(_Clients[name], name));
        }

        public static Task MonitorTask(Client c, string name)
        {
            return Task.Run((() =>
                             {
                                 while (true)
                                 {
                                     string s = c.StreamReader.ReadLine();
                                     Chat(name, s);
                                 }
                             }));
        }

        public static void Chat(string name, string message)
        {
            foreach (var client in _Clients)
            {
                if (name != client.Key)
                {
                    client.Value.StreamWriter.WriteLine($"{name}: {message}");
                }
            }
        }
    }
}
