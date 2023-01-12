using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GameClient
{
    public partial class MainWindow : Window
    {
        public Player Player { get; set; } = new Player();
        private TcpClient Server { get; set; }
        private IList<Cell> Cells { get; set; } 
        private const int Capacity = 25;

        public MainWindow()
        {
            InitializeComponent();

            new SignUpWindow(Player).ShowDialog();
            if (Player.Name is null)
            {
                Application.Current.Shutdown();
                return;
            }
            Title = new string(Title.Concat(Player.Name).ToArray());
            Cells = ((CellCollection)DataContext).Cells;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Server = new TcpClient(Player.Ip, int.Parse(Player.Port));
                RecieveFromServer();
            }
            catch (SocketException ex)
            {
                MessageBox.Show(ex.Message);
                Close();
            }

        }
        private async void RecieveFromServer()
        {
            BinaryReader reader = new BinaryReader(Server.GetStream(), Encoding.Unicode);
            while(true)
            {
                char[] area = await Task.Run(() => reader.ReadChars(Capacity));
                foreach (Cell cell in Cells)
                {
                    cell.Text = area[Cells.IndexOf(cell)];
                    cell.IsOpen = cell.Text != ' ';
                }
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e) 
        {
            Cell cell = (Cell)((Button)sender).DataContext; 
            int index = Cells.IndexOf(Cells.Where(c => c == cell).First());

            BinaryWriter writer = new BinaryWriter(Server.GetStream(), Encoding.Unicode);
            await Task.Run(() => writer.Write(index));
        }
    }
}
