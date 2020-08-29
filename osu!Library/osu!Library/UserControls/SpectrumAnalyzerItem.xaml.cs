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
    /// Логика взаимодействия для SpectrumAnalyzerItem.xaml
    /// </summary>
    public partial class SpectrumAnalyzerItem : UserControl
    {
        private double _movementSpeed = 15;
        public int Value
        {
            get
            {
                return (int)GetValue(ValueProperty);
            }
            set
            {
                SetValue(ValueProperty, value);
            }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register
        ("Value", typeof(int), typeof(SpectrumAnalyzerItem), new PropertyMetadata(0, ValuePropertyChanged));

        private static void ValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as SpectrumAnalyzerItem;
            int newValue = (int)e.NewValue;

            if (newValue < 0 || newValue > 255)
                throw new ArgumentException("SpectrumAnalyzerItem value can't be less than 0 and more than 255.");

            double distance = Math.Abs(control.rcMain.ActualHeight - control.ActualHeight * newValue / 255.0);
            double time = distance / control._movementSpeed;
            double speed = Math.Sign(control.ActualHeight * newValue / 255.0 - control.rcMain.ActualHeight) / 0.1;

            if (distance == 0)
                return;

            var animation = new DoubleAnimation
            {
                By = speed,
                Duration = TimeSpan.FromSeconds(0.1)
            };

            control.rcMain.BeginAnimation(Rectangle.HeightProperty, animation);
        }

        public SpectrumAnalyzerItem()
        {
            InitializeComponent();
            rcMain.Height = 0;
        }
    }
}
