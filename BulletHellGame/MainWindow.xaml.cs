using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using General_logic;
using Player;

namespace BulletHellGame
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Exit(object sender, RoutedEventArgs e)
        {
            Close();
        }


        private void NewGame(object sender, RoutedEventArgs e)
        {
            Floor1 floor1 = new Floor1();
            floor1.Show();
            Close();
        }
    }
}
