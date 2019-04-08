using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace osu_Library
{
    /// <summary>
    /// Логика взаимодействия для Splash.xaml
    /// </summary>
    public partial class Splash : Window
    {
        public Splash()
        {
            InitializeComponent();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        public void UpdateProgressText(string text)
        {
            this.Dispatcher.Invoke(() => {
                this.ProgressStatus.Content = text;
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
