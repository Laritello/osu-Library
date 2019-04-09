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
        public string LeftName
        {
            get
            {
                return (string)GetValue(LeftNameProperty);
            }
            set
            {
                SetValue(LeftNameProperty, value);
            }
        }

        public string RightName
        {
            get
            {
                return (string)GetValue(RightNameProperty);
            }
            set
            {
                SetValue(RightNameProperty, value);
            }
        }

        public bool IsChecked
        {
            get
            {
                return (bool)GetValue(IsCheckedProperty);
            }
            set
            {
                SetValue(IsCheckedProperty, value);
                ValueChanged?.Invoke(new ValueChangedEventArgs { Value = (bool)GetValue(IsCheckedProperty) });
            }
        }

        public static readonly DependencyProperty LeftNameProperty = DependencyProperty.Register
        ("LeftName", typeof(string), typeof(ToggleSwitch), new PropertyMetadata(string.Empty, LeftNameValueChanged));
        public static readonly DependencyProperty RightNameProperty = DependencyProperty.Register("RightName", typeof(string), typeof(ToggleSwitch), new PropertyMetadata(string.Empty, RightNameValueChanged));
        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(bool), typeof(ToggleSwitch), new PropertyMetadata(false, IsCheckedValueChanged));


        public delegate void ValueChangedEventHandler(ValueChangedEventArgs e);
        public event ValueChangedEventHandler ValueChanged;

        public ToggleSwitch()
        {
            InitializeComponent();
            IsChecked = false;
        }

        private void ToggleValue(object sender, MouseButtonEventArgs e)
        {
            ThicknessAnimation anim;
            ColorAnimation animColorOff, animColorOn;

            animColorOn = new ColorAnimation
            {
                From = (Color)FindResource("ColorActive"),
                To = (Color)FindResource("ColorForeground"),
                Duration = TimeSpan.FromSeconds(0.25)
            };

            animColorOff = new ColorAnimation
            {
                From = (Color)FindResource("ColorForeground"),
                To = (Color)FindResource("ColorActive"),
                Duration = TimeSpan.FromSeconds(0.25)
            };

            if (!IsChecked)
            {
                IsChecked = true;

                anim = new ThicknessAnimation
                {
                    From = new Thickness(0, 0, 0, 0),
                    To = new Thickness(175, 0, 0, 0),
                    Duration = TimeSpan.FromSeconds(0.25)
                };

                LabelLeft.Foreground = new SolidColorBrush();
                LabelRight.Foreground = new SolidColorBrush();
                LabelLeft.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, animColorOff);
                LabelRight.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, animColorOn);
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

                LabelLeft.Foreground = new SolidColorBrush();
                LabelRight.Foreground = new SolidColorBrush();

                LabelLeft.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, animColorOn);
                LabelRight.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, animColorOff);
                Toggle.BeginAnimation(Canvas.MarginProperty, anim);
            }
        }

        private static void LeftNameValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ToggleSwitch;
            control.LabelLeft.Content = control.LeftName;
        }

        private static void RightNameValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ToggleSwitch;
            control.LabelRight.Content = control.RightName;
        }

        private static void IsCheckedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ToggleSwitch;
            if (control.IsChecked)
            {
                control.LabelLeft.Foreground = new SolidColorBrush(Colors.Gray);
                control.LabelRight.Foreground = new SolidColorBrush(Colors.White);
                control.Toggle.Margin = new Thickness(175, 0, 0, 0);
            }
            else
            {
                control.LabelLeft.Foreground = new SolidColorBrush(Colors.White);
                control.LabelRight.Foreground = new SolidColorBrush(Colors.Gray);
                control.Toggle.Margin = new Thickness(0, 0, 0, 0);
            }
        }
    }

    public class ValueChangedEventArgs : EventArgs
    {
        public bool Value { get; set; }
    }
}
