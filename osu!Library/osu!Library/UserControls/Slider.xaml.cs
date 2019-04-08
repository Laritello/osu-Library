using osu_Library.Classes;
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
    /// Логика взаимодействия для Slider.xaml
    /// </summary>
    public partial class Slider : UserControl
    {
        public TimeSpan Minimum { get; set; }
        public TimeSpan Maximum { get; set; }
        public TimeSpan Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;

                double max = RectangleBackground.Width;

                RectangleForeground.Width = (double)_value.TotalSeconds * max / Maximum.TotalSeconds;
            }
        }

        private TimeSpan _value;

        public delegate void ValueChangedEventHandler(TimeSpan newValue);
        public event ValueChangedEventHandler ValueChanged;

        public Slider()
        {
            InitializeComponent();
        }

        private void SliderArea_MouseEnter(object sender, MouseEventArgs e)
        {
            AnimateSlider(AnimationAction.Open);
        }

        private void SliderArea_MouseLeave(object sender, MouseEventArgs e)
        {
            AnimateSlider(AnimationAction.Close);
        }

        private void AnimateSlider(AnimationAction state)
        {
            DoubleAnimation sliderAnimation = new DoubleAnimation();

            double time = 0;
            switch (state)
            {
                case AnimationAction.Open:
                    time = (RectangleForeground.MaxHeight - RectangleForeground.ActualHeight) / ((1 / 0.25) * RectangleForeground.MaxHeight);

                    sliderAnimation.From = RectangleForeground.ActualHeight;
                    sliderAnimation.To = RectangleForeground.MaxHeight;
                    sliderAnimation.Duration = TimeSpan.FromSeconds(time);
                    RectangleForeground.BeginAnimation(Slider.HeightProperty, sliderAnimation);
                    RectangleBackground.BeginAnimation(Slider.HeightProperty, sliderAnimation);
                    break;

                case AnimationAction.Close:
                    time = (RectangleForeground.ActualHeight - RectangleForeground.MinHeight) / ((1 / 0.25) * RectangleForeground.MaxHeight);

                    sliderAnimation.From = RectangleForeground.ActualHeight;
                    sliderAnimation.To = RectangleForeground.MinHeight;
                    sliderAnimation.Duration = TimeSpan.FromSeconds(time);
                    RectangleForeground.BeginAnimation(Slider.HeightProperty, sliderAnimation);
                    RectangleBackground.BeginAnimation(Slider.HeightProperty, sliderAnimation);
                    break;
            }
        }

        private void SliderArea_MouseDown(object sender, MouseButtonEventArgs e)
        {
            double max = RectangleBackground.Width;
            double current = e.GetPosition(this).X;

            int newLength = Convert.ToInt32(Maximum.TotalSeconds * current / max);

            Value = new TimeSpan(0,0, newLength);

            RectangleForeground.Width = current;

            ValueChanged?.Invoke(Value);
        }

        private void SliderPart_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double max = RectangleBackground.Width;

            RectangleForeground.Width = (double)_value.TotalSeconds * max / Maximum.TotalSeconds;
        }
    }
}
