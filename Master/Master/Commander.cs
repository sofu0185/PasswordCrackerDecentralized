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

    public class Commander
    {
        private Passwords pass;
        private Chunks dict;
        public Stopwatch Stopwatch { get; set; }

        public Commander()
        {
            pass = new Passwords();
            Stopwatch = new Stopwatch();
            dict = new Chunks();
        }

        public Task MonitorTaskMultiplePasswords(Client c, int index)
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
                        
                        Console.WriteLine(Stopwatch.Elapsed);
                    }
                }
                Stopwatch.Stop();
                Console.WriteLine(Stopwatch.Elapsed);
                Console.ReadLine();

            });
        }

        public void SendMultiplePasswords(Client c)
        {
            c.StreamWriter.WriteLine(dict.CurrenntChunkIndex);
            c.StreamWriter.WriteLine(pass.UsersAndPasswordsAsString);
            c.StreamWriter.WriteLine(dict.GetStringChunk());
        }

        public void SendAsBytes(Client c)
        {
            NetworkStream ns = c.NetworkStream;
            BinaryFormatter formatter = new BinaryFormatter();

            formatter.Serialize(ns, dict.CurrenntChunkIndex.ToString());
            formatter.Serialize(ns, pass.UsersAndPasswordsAsString);
            ns.Write(dict.GetByteChunk());
        }
    }
}
