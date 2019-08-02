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

namespace osu_Library.UserControls
{
    /// <summary>
    /// Логика взаимодействия для SpectrumAnalyzerItem.xaml
    /// </summary>
    public partial class SpectrumAnalyzerItem : UserControl
    {
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

            double percentage = (int)e.NewValue / 255.0;

            control.rcMain.Height = control.ActualHeight * percentage;
        }

        public SpectrumAnalyzerItem()
        {
            InitializeComponent();
        }
    }
}
