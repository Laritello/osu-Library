using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using osu_Library.Classes;
using osu_Library.UserControls;
using osu_Library.Utility;

namespace osu_Library.Pages
{
    /// <summary>
    /// Логика взаимодействия для PageSettings.xaml
    /// </summary>
    public partial class PageSettings : Page
    {
        public PageSettings()
        {
            InitializeComponent();

            DirectoryPickerGamePath.SelectedPath = AppSettings.GamePath;
            ToggleSwitchVolume.IsChecked = AppSettings.ExponentialVolume;

            ComboBoxLang.ItemsSource = App.Languages;
        }

        private void ColorPickerMain_SelectedColorChanged(SelectedColorChangedEventArgs e)
        {
            ResourceDictionary resource = Application.Current.Resources;

            resource.MergedDictionaries[0]["ColorMain"] = e.NewColor;
            resource.MergedDictionaries[0]["ColorSecondary"] = e.NewColor.GetSecondaryColor();

            AppSettings.AppColor = e.NewColor;
        }

        private void DirectoryPickerGamePath_SelectedPathChanged(SelectedPathChangedEventArgs e)
        {
            if (e.OldPath != e.NewPath)
            {
                AppSettings.GamePath = e.NewPath;
            }
        }

        private void ToggleSwitchVolume_ValueChanged(ValueChangedEventArgs e)
        {
            AppSettings.ExponentialVolume = e.Value;

            MainWindow parent = Application.Current.MainWindow as MainWindow;
            parent.UpdateSliderVolume();
        }

        private void ComboBoxLang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxLang.SelectedItem == null)
            {
                App.Language = new CultureInfo("en-US");
                return;
            }

            try
            {
                App.Language = ComboBoxLang.SelectedItem as CultureInfo;
            }
            catch
            {
                App.Language = new CultureInfo("en-US");
            }
        }
    }
}
