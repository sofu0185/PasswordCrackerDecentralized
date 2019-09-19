using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Master
{
    using System.IO;
    using System.Net.Sockets;
    using System.Runtime.Serialization.Formatters.Binary;
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
            StreamReader streamReader = new StreamReader(networkStream);
            StreamWriter streamWriter = new StreamWriter(networkStream);
            streamWriter.AutoFlush = true;

            _Clients.Add(_index, new Client(client, networkStream, streamWriter, streamReader));

            stopwatch.Start();
            monitorTasks.Add(MonitorTaskMultiplePasswords(_Clients[_index], _index));

            _index++;
        }

        public static Task MonitorTaskMultiplePasswords(Client c, int index)
        {
            return Task.Run(() =>
            {
                while (!dict.EndOfChunks)
                {
                    SendMultiplePasswords(c);

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

        public static void SendMultiplePasswords(Client c)
        {
            c.StreamWriter.WriteLine(dict.CurrenntChunkIndex);
            c.StreamWriter.WriteLine(pass.UsersAndPasswordsAsString);
            c.StreamWriter.WriteLine(dict.GetStringChunk());
        }

        public static void SendAsBytes(Client c)
        {
            NetworkStream ns = c.NetworkStream;
            BinaryFormatter formatter = new BinaryFormatter();

            formatter.Serialize(ns, dict.CurrenntChunkIndex.ToString());
            formatter.Serialize(ns, pass.UsersAndPasswordsAsString);
            ns.Write(dict.GetByteChunk());
        }
    }
}
