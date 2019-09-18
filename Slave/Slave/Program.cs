using System;
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
            Console.WriteLine("Hello World!");

            TcpClient clientSocket = new TcpClient(IPADDRESS, PORT);

            NetworkStream ns = clientSocket.GetStream();

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken ct = tokenSource.Token;

            // Runs task on another thread
            Task.Run(() =>
            {
                StreamWriter sw = new StreamWriter(ns);
                while (true)
                {
                    sw.WriteLine(Console.ReadLine());
                    if (ct.IsCancellationRequested)
                        break;
                    sw.Flush();
                }
            }, ct);

            StreamReader sr = new StreamReader(ns);
            string clientMessage = "";
            while (clientSocket.Connected && clientMessage != "QUIT")
            {
                try
                {
                    clientMessage = sr.ReadLine();

                    int leftPositionCursor = Console.CursorLeft;
                    if (leftPositionCursor > 0)
                    {
                        int topPositionCursor = Console.CursorTop;
                        Console.MoveBufferArea(0, topPositionCursor, leftPositionCursor, 1, 0, topPositionCursor + 1);
                        Console.CursorLeft = 0;
                    }

                    Console.WriteLine(/*"Server says: " + */clientMessage);
                    Console.CursorLeft = leftPositionCursor;
                }
                catch (IOException e)
                {
                    if (e.InnerException.GetType() != typeof(SocketException))
                        throw e;
                }
            }

            tokenSource.Cancel();
            ns.Close();

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nConnection ended...");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
