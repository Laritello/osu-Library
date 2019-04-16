using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace osu_Library.Classes
{
    public static class AppSettings
    {
        public static string GamePath
        {
            get
            {
                return Properties.Settings.Default.GamePath;
            }
            set
            {
                Properties.Settings.Default.GamePath = value;
                Properties.Settings.Default.Save();
            }
        }

        public static string SongsPath
        {
            get
            {
                return Properties.Settings.Default.GamePath + "\\Songs";
            }
        }

        public static double WindowHeight
        {
            get
            {
                return Properties.Settings.Default.WindowHeight;
            }
            set
            {
                Properties.Settings.Default.WindowHeight = value;
                Properties.Settings.Default.Save();
            }
        }

        public static double WindowWidth
        {
            get
            {
                return Properties.Settings.Default.WindowWidth;
            }
            set
            {
                Properties.Settings.Default.WindowWidth = value;
                Properties.Settings.Default.Save();
            }
        }

        public static WindowMode OverlayMode
        {
            get
            {
                return (WindowMode)Properties.Settings.Default.OverlayMode;
            }
            set
            {
                Properties.Settings.Default.OverlayMode = (int)value;
                Properties.Settings.Default.Save();
            }
        }

        public static Color AppColor
        {
            get
            {
                return Properties.Settings.Default.AppColor;
            }
            set
            {
                Properties.Settings.Default.AppColor = value;
                Properties.Settings.Default.Save();
            }
        }

        public static float Volume
        {
            get
            {
                return Properties.Settings.Default.PlayerVolume;
            }
            set
            {
                Properties.Settings.Default.PlayerVolume = value;
                Properties.Settings.Default.Save();
            }
        }

        public static bool ExponentialVolume
        {
            get
            {
                return Properties.Settings.Default.ExponentialVolume;
            }
            set
            {
                Properties.Settings.Default.ExponentialVolume = value;
                Properties.Settings.Default.Save();
            }
        }

        public static CultureInfo Language
        {
            get
            {
                return Properties.Settings.Default.Language;
            }
            set
            {
                Properties.Settings.Default.Language = value;
                Properties.Settings.Default.Save();
            }
        }
    }
}
