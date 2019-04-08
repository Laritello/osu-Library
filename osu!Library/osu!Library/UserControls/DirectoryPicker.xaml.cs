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
    /// Логика взаимодействия для DirectoryPicker.xaml
    /// </summary>
    public partial class DirectoryPicker : UserControl
    {
        public DirectoryPicker()
        {
            InitializeComponent();
        }

        public string SelectedPath
        {
            get
            {
                return TextBoxPath.Text;
            }
            set
            {
                TextBoxPath.Text = value;
            }
        }

        #region Events
        public delegate void SelectedPathChangedEventHandler(SelectedPathChangedEventArgs e);
        public event SelectedPathChangedEventHandler SelectedPathChanged;
        #endregion

        private void ButtonOpen_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    string _oldPath = SelectedPath;
                    SelectedPath = dialog.SelectedPath;

                    SelectedPathChanged?.Invoke(new SelectedPathChangedEventArgs { OldPath = _oldPath, NewPath = SelectedPath });
                }
            }
        }
    }

    public class SelectedPathChangedEventArgs : EventArgs
    {
        public string OldPath { get; set; }
        public string NewPath { get; set; }
    }
}
