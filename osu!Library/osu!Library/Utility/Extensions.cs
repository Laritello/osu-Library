using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace osu_Library.Utility
{
    public static class Extensions
    {
        public static Color GetSecondaryColor(this Color mainColor)
        {
            float k = 0.7f;
            return Color.FromRgb((byte)(mainColor.R * k), (byte)(mainColor.G * k), (byte)(mainColor.B * k));
        }
    }
}
