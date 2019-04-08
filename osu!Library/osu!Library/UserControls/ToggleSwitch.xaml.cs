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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace osu_Library.UserControls
{
    /// <summary>
    /// Логика взаимодействия для ToggleSwitch.xaml
    /// </summary>
    public partial class ToggleSwitch : UserControl
    {
        public bool IsChecked { get; set; }

        public ToggleSwitch()
        {
            InitializeComponent();
            IsChecked = false;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ThicknessAnimation anim;
            if (!IsChecked)
            {
                IsChecked = true;

                anim = new ThicknessAnimation
                {
                    From = new Thickness(0, 0, 0, 0),
                    To = new Thickness(175, 0, 0, 0),
                    Duration = TimeSpan.FromSeconds(0.25)
                };

                Toggle.BeginAnimation(Canvas.MarginProperty, anim);
            }
            else
            {
                IsChecked = false;

                anim = new ThicknessAnimation
                {
                    From = new Thickness(175, 0, 0, 0),
                    To = new Thickness(0, 0, 0, 0),
                    Duration = TimeSpan.FromSeconds(0.25)
                };

                Toggle.BeginAnimation(Canvas.MarginProperty, anim);
            }
        }
    }
}
