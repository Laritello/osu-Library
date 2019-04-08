using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

using osu_Library.Utility;

namespace osu_Library.UserControls
{
    /// <summary>
    /// Логика взаимодействия для ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : UserControl
    {
        public bool IsOpen { get; set; }

        public ColorPicker()
        {
            InitializeComponent();
            var button = ButtonSelectColor;
            IsOpen = false;
            RowSelector.Height = new GridLength(0);
        }

        #region Events
        public delegate void SelectedColorChangedEventHandler(SelectedColorChangedEventArgs e);
        public event SelectedColorChangedEventHandler SelectedColorChanged;
        #endregion

        #region User Interaction
        private void ButtonSelectColor_Click(object sender, RoutedEventArgs e)
        {
            GridLengthAnimation anim = new GridLengthAnimation();

            if (IsOpen)
            {
                anim.From = new GridLength(160);
                anim.To = new GridLength(0);
                anim.Duration = TimeSpan.FromSeconds(0.25);
            }
            else
            {
                anim.From = new GridLength(0);
                anim.To = new GridLength(160);
                anim.Duration = TimeSpan.FromSeconds(0.25);
            }

            IsOpen = !IsOpen;
            RowSelector.BeginAnimation(RowDefinition.HeightProperty, anim);
        }

        private void CanvasGamma_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                try
                {
                    Point currentPosition = CanvasGamma.PointToScreen(e.MouseDevice.GetPosition(CanvasGamma));
                    Color color = GetPixelColor(currentPosition);

                    var res = this.Resources;
                    res["CurrentColor"] = color;
                }
                catch
                {
                    var res = this.Resources;
                    res["CurrentColor"] = Colors.Black;
                }
            }
        }

        private void CanvasColor_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ResourceDictionary resource = Application.Current.Resources;
                Color _oldColor = (Color)resource.MergedDictionaries[0]["ColorMain"];

                try
                {
                    Point currentPosition = CanvasColor.PointToScreen(e.MouseDevice.GetPosition(CanvasColor));
                    Color _newColor = GetPixelColor(currentPosition);

                    SelectedColorChanged?.Invoke(new SelectedColorChangedEventArgs { OldColor = _oldColor, NewColor = _newColor });
                }
                catch
                {
                    resource.MergedDictionaries[0]["ColorMain"] = _oldColor;
                }
            }
        }
        #endregion

        #region Colors Definition
        [DllImport("gdi32")]
        private static extern int GetPixel(int hdc, int nXPos, int nYPos);

        [DllImport("user32")]
        private static extern int GetWindowDC(int hwnd);

        [DllImport("user32")]
        private static extern int ReleaseDC(int hWnd, int hDC);

        private static Color GetPixelColor(Point point)
        {
            int lDC = GetWindowDC(0);
            int intColor = GetPixel(lDC, (int)point.X, (int)point.Y);

            // Release the DC after getting the Color.
            ReleaseDC(0, lDC);

            byte a = (byte)((intColor >> 0x18) & 0xffL);
            byte b = (byte)((intColor >> 0x10) & 0xffL);
            byte g = (byte)((intColor >> 8) & 0xffL);
            byte r = (byte)(intColor & 0xffL);
            Color color = Color.FromRgb(r, g, b);
            return color;
        }
        #endregion
    }

    public class SelectedColorChangedEventArgs : EventArgs
    {
        public Color OldColor { get; set; }
        public Color NewColor { get; set; }
    }
}
