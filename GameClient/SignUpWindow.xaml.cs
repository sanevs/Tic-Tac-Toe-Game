﻿using System.IO;
using System.Windows;

namespace GameClient
{
    public class Player
    {
        public string Ip { get; set; }
        public string Port { get; set; }
        public string Name { get; set; }
    }
    public partial class SignUpWindow : Window
    {
        public Player Player { get; set; }
        public SignUpWindow(Player player)
        {
            InitializeComponent();
            Player = player;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ip.Text = File.ReadAllText("ip.txt");
            port.Text = File.ReadAllText("port.txt");
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ip.Text == string.Empty || port.Text == string.Empty || name.Text == string.Empty)
            {
                MessageBox.Show("Заполните все поля", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Player.Ip = ip.Text;
            Player.Port = port.Text;
            Player.Name = name.Text;
            Close();
        }

    }
}
