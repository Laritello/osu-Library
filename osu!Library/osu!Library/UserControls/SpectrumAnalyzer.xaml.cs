using NAudio.Dsp;
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
    /// Логика взаимодействия для SpectrumAnalyzer.xaml
    /// </summary>
    public partial class SpectrumAnalyzer : UserControl
    {
        private int columnsCount = 32;
        private SpectrumAnalyzerItem[] columns;

        public SpectrumAnalyzer()
        {
            InitializeComponent();
            columns = new SpectrumAnalyzerItem[]
            {
                item1, item2, item3, item4, item5, item6, item7, item8,
                item9, item10, item11, item12, item13, item14, item15, item16,
                item17, item18, item19, item20, item21, item22, item23, item24,
                item25, item26, item27, item28, item29, item30, item31, item32
            };
        }

        public void Update(Complex[] data)
        {
            int b0 = 0;
            
            for (int i = 0; i < columnsCount; i++)
            {
                double peak = 0;
                
                int b1 = (int)Math.Pow(2, i * 10 / (columnsCount - 1));
                if (b1 > 1023) b1 = 1023;
                if (b1 <= b0) b1 = b0 + 1;

                for (;b0<b1;b0++)
                {
                    double intensityDB = Math.Sqrt(data[1 + b0].X * data[1 + b0].X + data[1 + b0].Y * data[1 + b0].Y);

                    if (peak < intensityDB)
                        peak = intensityDB;
                }

                int y = (int)(Math.Sqrt(peak) * 3 * 255 - 4);

                if (y > 255) y = 255;
                if (y < 0) y = 0;

                columns[i].Value = y;
            }
        }
    }
}
