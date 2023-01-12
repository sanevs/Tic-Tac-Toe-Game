using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
//Encoding.Unicode

namespace GameServer
{
    enum GameStatus { Initial, XWins, OWins, Draw }
    class Program
    {
        private const int SideLength = 5;
        private IList<TcpClient> Clients { get; } = new List<TcpClient>();
        private char[] Area { get; } = Enumerable
            .Range(0, SideLength * SideLength)
            .Select(i => ' ')
            .ToArray();
        private bool Movement { get; set; } = true; // в начале игры ход крестиков
        private GameStatus GameStatus { get; set; } = GameStatus.Initial;
        static void Main(string[] args) => new Program().Run();

        private void Run()
        {
            TcpListener listener = new TcpListener(IPAddress.Parse(
                File.ReadAllText("ip.txt")), int.Parse(File.ReadAllText("port.txt")));
            listener.Start();
            Console.WriteLine($"Server is listening on  {listener.LocalEndpoint}");

            Connect(listener); //1 игрок
            Connect(listener); //2 игрок
            listener.Stop();
            Console.WriteLine($"Server stopped listen");
            
            Queue<Task> tasks = new Queue<Task>();
            while (true)
            {
                foreach (TcpClient client in Clients)
                    tasks.Enqueue(Task.Run(() => OperateClient(client)));
                Task.WhenAll(tasks.Dequeue(), tasks.Dequeue()).Wait();

                CheckWin();
                if (GameStatus != GameStatus.Initial)
                    break;
            }

            if(GameStatus == GameStatus.XWins)
                Console.WriteLine("Крестики выиграли!");
            else if(GameStatus == GameStatus.OWins)
                Console.WriteLine("Нолики выиграли!");
            else
                Console.WriteLine("Ничья.");
        }
        private void Connect(TcpListener listener)
        {
            TcpClient client = listener.AcceptTcpClient();
            lock (Clients)
                Clients.Add(client);
            Console.WriteLine($"Client {client.Client.RemoteEndPoint} connected");
        }
        private async Task OperateClient(TcpClient client)
        {
            bool isX = client == Clients.First();
            BinaryReader reader = new BinaryReader(client.GetStream(), Encoding.Unicode);
            int index = await Task.Run(() => reader.ReadInt32());
            if (isX == Movement && Area[index] == ' ')
            {
                Area[index] = isX ? 'X' : 'O';
                Movement = !Movement; //переход хода
                SendAll();
            }
            else
            {
                SendAll();
                OperateClient(client);
            }
        }
        private void SendAll()
        {
            foreach (TcpClient otherClient in Clients)
            {
                BinaryWriter writer = new BinaryWriter(otherClient.GetStream(), Encoding.Unicode);
                writer.Write(Area);
            }
        }
        private void CheckWin()
        {
            bool?[,] field = new bool?[SideLength, SideLength];
            int k = 0;
            for (int i = 0; i < SideLength; i++)
                for (int j = 0; j < SideLength; j++, k++)
                {
                    if (Area[k] == ' ')
                        continue;
                    field[i, j] = Area[k] == 'X';
                }

            if (((field[0, 0] == field[0, 1] && field[0, 0] == field[0, 2] && field[0, 0] == field[0, 3] && field[0, 0] == field[0, 4]) && field[0, 0] == true) ||
                ((field[1, 0] == field[1, 1] && field[1, 0] == field[1, 2] && field[1, 0] == field[1, 3] && field[1, 0] == field[1, 4]) && field[1, 0] == true) ||
                ((field[2, 0] == field[2, 1] && field[2, 0] == field[2, 2] && field[2, 0] == field[2, 3] && field[2, 0] == field[2, 4]) && field[2, 0] == true) ||
                ((field[3, 0] == field[3, 1] && field[3, 0] == field[3, 2] && field[3, 0] == field[3, 3] && field[3, 0] == field[3, 4]) && field[3, 0] == true) ||
                ((field[4, 0] == field[4, 1] && field[4, 0] == field[4, 2] && field[4, 0] == field[4, 3] && field[4, 0] == field[4, 4]) && field[4, 0] == true) ||
                                                                                         
                ((field[0, 0] == field[1, 0] && field[0, 0] == field[2, 0] && field[0, 0] == field[3, 0] && field[0, 0] == field[4, 0]) && field[0, 0] == true) ||
                ((field[0, 1] == field[1, 1] && field[0, 1] == field[2, 1] && field[0, 1] == field[3, 1] && field[0, 1] == field[4, 1]) && field[0, 1] == true) ||
                ((field[0, 2] == field[1, 2] && field[0, 2] == field[2, 2] && field[0, 2] == field[3, 2] && field[0, 2] == field[4, 2]) && field[0, 2] == true) ||
                ((field[0, 3] == field[1, 3] && field[0, 3] == field[2, 3] && field[0, 3] == field[3, 3] && field[0, 3] == field[4, 3]) && field[0, 3] == true) ||
                ((field[0, 4] == field[1, 4] && field[0, 4] == field[2, 4] && field[0, 4] == field[3, 4] && field[0, 4] == field[4, 4]) && field[0, 4] == true) ||
                                                                                                                 
                ((field[0, 0] == field[1, 1] && field[0, 0] == field[2, 2] && field[0, 0] == field[3, 3] && field[0, 0] == field[4, 4]) && field[0, 0] == true) ||
                ((field[4, 0] == field[3, 1] && field[4, 0] == field[2, 2] && field[4, 0] == field[1, 3] && field[4, 0] == field[0, 4]) && field[4, 0] == true))
                GameStatus = GameStatus.XWins;

            else if (((field[0, 0] == field[0, 1] && field[0, 0] == field[0, 2] && field[0, 0] == field[0, 3] && field[0, 0] == field[0, 4]) && field[0, 0] == false) ||
                ((field[1, 0] == field[1, 1] && field[1, 0] == field[1, 2] && field[1, 0] == field[1, 3] && field[1, 0] == field[1, 4]) && field[1, 0] == false) ||
                ((field[2, 0] == field[2, 1] && field[2, 0] == field[2, 2] && field[2, 0] == field[2, 3] && field[2, 0] == field[2, 4]) && field[2, 0] == false) ||
                ((field[3, 0] == field[3, 1] && field[3, 0] == field[3, 2] && field[3, 0] == field[3, 3] && field[3, 0] == field[3, 4]) && field[3, 0] == false) ||
                ((field[4, 0] == field[4, 1] && field[4, 0] == field[4, 2] && field[4, 0] == field[4, 3] && field[4, 0] == field[4, 4]) && field[4, 0] == false) ||
                                                                                         
                ((field[0, 0] == field[1, 0] && field[0, 0] == field[2, 0] && field[0, 0] == field[3, 0] && field[0, 0] == field[4, 0]) && field[0, 0] == false) ||
                ((field[0, 1] == field[1, 1] && field[0, 1] == field[2, 1] && field[0, 1] == field[3, 1] && field[0, 1] == field[4, 1]) && field[0, 1] == false) ||
                ((field[0, 2] == field[1, 2] && field[0, 2] == field[2, 2] && field[0, 2] == field[3, 2] && field[0, 2] == field[4, 2]) && field[0, 2] == false) ||
                ((field[0, 3] == field[1, 3] && field[0, 3] == field[2, 3] && field[0, 3] == field[3, 3] && field[0, 3] == field[4, 3]) && field[0, 3] == false) ||
                ((field[0, 4] == field[1, 4] && field[0, 4] == field[2, 4] && field[0, 4] == field[3, 4] && field[0, 4] == field[4, 4]) && field[0, 4] == false) ||
                                                                                                                 
                ((field[0, 0] == field[1, 1] && field[0, 0] == field[2, 2] && field[0, 0] == field[3, 3] && field[0, 0] == field[4, 4]) && field[0, 0] == false) ||
                ((field[4, 0] == field[3, 1] && field[4, 0] == field[2, 2] && field[4, 0] == field[1, 3] && field[4, 0] == field[0, 4]) && field[4, 0] == false))
                GameStatus = GameStatus.OWins;

            else if (Area.Where(c => c == ' ').Count() == 1)
                GameStatus = GameStatus.Draw;
        }
    }
}
