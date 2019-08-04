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
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Runtime.InteropServices;
using System.Windows.Interop;

using osu_Library.Classes;

namespace osu_Library.UserControls
{
    /// <summary>
    /// Логика взаимодействия для TitleBar.xaml
    /// </summary>
    public partial class TitleBar : UserControl
    {
        // TODO: Даблклик убирает список песен оставляя маленький плеер
        public TitleBar()
        {
            InitializeComponent();
        }

        public delegate void OverlayModeChangedEventHandler(WindowMode overlayModeOn);
        public event OverlayModeChangedEventHandler OverlayModeChanged;


        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (Application.Current.MainWindow.WindowState == WindowState.Maximized)
                {
                    // Return window back to normal state. Grab window by the middle
                    Application.Current.MainWindow.WindowState = WindowState.Normal;
                    Application.Current.MainWindow.Left = e.GetPosition(this).X - Application.Current.MainWindow.Width / 2;
                    Application.Current.MainWindow.Top = e.GetPosition(this).Y - 5;
                }

                Application.Current.MainWindow.DragMove();
            }  
        }

        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Application.Current.MainWindow.Topmost)
                OverlayModeChanged?.Invoke(WindowMode.Window);
            else
                OverlayModeChanged?.Invoke(WindowMode.Overlay);
        }

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonGitHub_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://github.com/Laritello/osu-Library");
            }
            catch
            {
                // Do nothing for now
            }
        }
    }
}
