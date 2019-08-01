using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

using Microsoft.Win32;
using System.Runtime.InteropServices;

using osu_Library.Classes;
using osu_Library.Pages;
using osu_Library.Utility;
using System.Globalization;

namespace osu_Library
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const double sliderAnimationTime = 0.25; // Time in seconds
        LibraryManager _manager;
        private bool loadingFailed = false;
        //////////////////////////////////////////////////////////////////
        ///Player
        Player _player;
        Beatmap _selectedSong;
        Stack<int> _history;

        DispatcherTimer _timerDuration;

        bool Repeat = false;
        bool Shuffle = false;
        //////////////////////////////////////////////////////////////////

        // Remember last size to restore window from overlay mode
        private double _lastWindowWidth;
        private double _lastWindowHeight;
        private bool isOverlayChanging = false;

        //////////////////////////////////////////////////////////////////
        // Menus
        private MenuSection _selectedMenu;
        private Dictionary<MenuSection, Page> _menus;
        private bool isMenuOpen = false;

        #region Initialization
        public MainWindow()
        {
            InitializeComponent();

            InitializeWindow();
            InitializePlayer();
            InitializeMenu();


        }

        private void InitializeMenu()
        {
            _menus = new Dictionary<MenuSection, Page>
            {
                { MenuSection.Settings, new PageSettings() },
                { MenuSection.About, new PageAbout() }
            };
        }

        private void InitializeWindow()
        {
            this.Width = AppSettings.WindowWidth;
            this.Height = AppSettings.WindowHeight;

            _lastWindowWidth = AppSettings.WindowWidth;
            _lastWindowHeight = AppSettings.WindowHeight;

            ResourceDictionary resource = Application.Current.Resources;

            resource.MergedDictionaries[0]["ColorMain"] = AppSettings.AppColor;
            resource.MergedDictionaries[0]["ColorSecondary"] = AppSettings.AppColor.GetSecondaryColor();

            WindowModeSetup(AppSettings.OverlayMode);

            App.LanguageChanged += LanguageChanged;
        }

        private void InitializePlayer()
        {
            _player = new Player();
            _history = new Stack<int>();

            _player.ModeChanged += _player_ModeChanged;
            _player.SongEnded += _player_SongEnded;
            _player.Volume = AppSettings.Volume;

            _timerDuration = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };

            _timerDuration.Tick += _timerDuration_Tick;
            _timerDuration.Start();

            if (AppSettings.ExponentialVolume)
                SliderVolume.Value = GetSliderValueFromExponantialVolume(_player.Volume);
            else
                SliderVolume.Value = _player.Volume;
        }
        #endregion

        #region GUI Logic
        private void ButtonVolume_MouseEnter(object sender, MouseEventArgs e)
        {
            AnimateSliderVolume(AnimationAction.Open);
        }

        private void ButtonVolume_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!StackPanelVolume.IsMouseOver)
                AnimateSliderVolume(AnimationAction.Close);
        }

        private void StackPanelVolume_MouseLeave(object sender, MouseEventArgs e)
        {
            AnimateSliderVolume(AnimationAction.Close);
        }
        #endregion

        #region GUI Functions and Animations
        private void AnimateSliderVolume(AnimationAction action)
        {
            DoubleAnimation sliderAnimation = new DoubleAnimation();
            double time = 0;
            switch (action)
            {
                case AnimationAction.Open:
                    time = (SliderVolume.MaxWidth - SliderVolume.ActualWidth) / ((1 / sliderAnimationTime) * SliderVolume.MaxWidth);

                    sliderAnimation.From = SliderVolume.ActualWidth;
                    sliderAnimation.To = SliderVolume.MaxWidth;
                    sliderAnimation.Duration = TimeSpan.FromSeconds(time);

                    SliderVolume.BeginAnimation(Slider.WidthProperty, sliderAnimation);

                    break;

                case AnimationAction.Close:
                    time = (SliderVolume.ActualWidth - SliderVolume.MinWidth) / ((1 / sliderAnimationTime) * SliderVolume.MaxWidth);

                    sliderAnimation.From = SliderVolume.ActualWidth;
                    sliderAnimation.To = SliderVolume.MinWidth;
                    sliderAnimation.Duration = TimeSpan.FromSeconds(time);

                    SliderVolume.BeginAnimation(Slider.WidthProperty, sliderAnimation);
                    break;
            }
        }

        private void AnimateProgress(AnimationProgressStage status)
        {
            DoubleAnimation anim;

            switch (status)
            {
                case AnimationProgressStage.Active:
                    ControlProgress.Content = (Canvas)FindResource("CanvasLoading");

                    anim = new DoubleAnimation
                    {
                        Duration = TimeSpan.FromSeconds(0.25),
                        From = 0,
                        To = 1
                    };

                    anim.Completed += new EventHandler(delegate (object a, EventArgs ev)
                    {
                        anim = new DoubleAnimation
                        {
                            Duration = TimeSpan.FromSeconds(3),
                            From = 0,
                            To = 360,
                            RepeatBehavior = RepeatBehavior.Forever
                        };

                        RotateTransformControlProgress.BeginAnimation(RotateTransform.AngleProperty, anim);
                    });

                    ControlProgress.BeginAnimation(OpacityProperty, anim);
                    break;

                case AnimationProgressStage.Finished:
                    anim = new DoubleAnimation
                    {
                        Duration = TimeSpan.FromSeconds(0.25),
                        From = 1,
                        To = 0
                    };

                    // The following madness of events makes the following animation -> Rotating arrows slowly hide,
                    // then check mark slowly appears, exists for some time and also slowly hides.
                    // Yes, i didn't come up with better solution yet. I suppose i should use storyboards, but
                    // i'll do it later.
                    anim.Completed += new EventHandler(delegate (object o, EventArgs e)
                    {
                        ControlProgress.Content = (Canvas)FindResource("CanvasCheck");
                        RotateTransformControlProgress.BeginAnimation(RotateTransform.AngleProperty, null);

                        anim = new DoubleAnimation
                        {
                            Duration = TimeSpan.FromSeconds(0.25),
                            From = 0,
                            To = 1
                        };

                        anim.Completed += new EventHandler(delegate (object a, EventArgs ev)
                        {
                            anim = new DoubleAnimation
                            {
                                BeginTime = TimeSpan.FromSeconds(1),
                                Duration = TimeSpan.FromSeconds(0.25),
                                From = 1,
                                To = 0
                            };

                            ControlProgress.BeginAnimation(OpacityProperty, anim);
                        });

                        ControlProgress.BeginAnimation(OpacityProperty, anim);
                    });

                    ControlProgress.BeginAnimation(OpacityProperty, anim);
                    break;
            }
        }

        private void AnimateMenu(AnimationAction action)
        {
            Storyboard storyboard;
            DoubleAnimation anim;

            switch (action)
            {
                case AnimationAction.Open:
                    anim = new DoubleAnimation
                    {
                        From = 0,
                        To = 1,
                        Duration = TimeSpan.FromSeconds(0.25),
                    };

                    storyboard = new Storyboard
                    {
                        FillBehavior = FillBehavior.HoldEnd
                    };

                    anim.Completed += new EventHandler((object o, EventArgs e) =>
                    {
                        anim = new DoubleAnimation
                        {
                            From = 0,
                            To = 1,
                            Duration = TimeSpan.FromSeconds(0.25),
                        };

                        FrameMenusContent.Content = _menus[_selectedMenu];
                        FrameMenusContent.BeginAnimation(OpacityProperty, anim);
                        isMenuOpen = true;
                    });

                    Storyboard.SetTarget(anim, this.GridMenus);
                    Storyboard.SetTargetProperty(anim, new PropertyPath("RenderTransform.ScaleY"));
                    storyboard.Children.Add(anim);

                    storyboard.Begin();
                    break;

                case AnimationAction.Close:
                    anim = new DoubleAnimation
                    {
                        From = 1,
                        To = 0,
                        Duration = TimeSpan.FromSeconds(0.25)
                    };

                    anim.Completed += new EventHandler((object o, EventArgs e) =>
                    {
                        FrameMenusContent.Content = null;
                        _selectedMenu = MenuSection.None;

                        storyboard = new Storyboard
                        {
                            FillBehavior = FillBehavior.HoldEnd
                        };

                        anim = new DoubleAnimation
                        {
                            From = 1,
                            To = 0,
                            Duration = TimeSpan.FromSeconds(0.25)
                        };

                        Storyboard.SetTarget(anim, this.GridMenus);
                        Storyboard.SetTargetProperty(anim, new PropertyPath("RenderTransform.ScaleY"));
                        storyboard.Children.Add(anim);

                        storyboard.Begin();

                        isMenuOpen = false;
                    });

                    FrameMenusContent.BeginAnimation(OpacityProperty, anim);
                    break;
            }
        }

        private void WindowModeSetup(WindowMode mode)
        {
            isOverlayChanging = true;
            switch (mode)
            {
                case WindowMode.Overlay:
                    MinHeight = 210;
                    Topmost = true;
                    Height = 210;
                    Width = 400;
                    ResizeMode = ResizeMode.NoResize;
                    break;
                case WindowMode.Window:
                    Topmost = false;
                    Height = _lastWindowHeight;
                    Width = _lastWindowWidth;
                    ResizeMode = ResizeMode.CanResize;
                    MinHeight = 400;
                    break;
            }
            isOverlayChanging = false;
        }

        private void LoadMenu(MenuSection section)
        {
            if (_selectedMenu == section)
            {
                AnimateMenu(AnimationAction.Close);
                return;
            }

            _selectedMenu = section;
            
            if (isMenuOpen)
            {
                FrameMenusContent.Content = _menus[_selectedMenu];
            }
            else
            {
                AnimateMenu(AnimationAction.Open);
            }
        }
        #endregion

        #region User Interactions
        private void ButtonVolume_Click(object sender, RoutedEventArgs e)
        {
            if (_player != null)
                _player.Muted = !_player.Muted;

            if (_player.Muted)
                ButtonVolume.Tag = "Muted";
            else
                ButtonVolume.Tag = "Unmuted";
        }

        private void ButtonPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (ListBoxSongs.Items == null)
                return;

            if (_player != null)
                _player.Stop();

            Beatmap nextSong = GetFromHistory();

            if (nextSong == null)
                return;

            _selectedSong = nextSong;

            Play();
        }

        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            if (_player != null)
            {
                switch(_player.Mode)
                {
                    case PlayMode.Play:
                        _player.Pause();
                        break;

                    case PlayMode.Pause:
                        _player.Play();
                        break;

                    case PlayMode.Stop:
                        _player.Play();
                        break;

                    case PlayMode.Unloaded:
                        if (ListBoxSongs.Items == null)
                            return;
                        else if (ListBoxSongs.SelectedItem == null)
                            ListBoxSongs.SelectedIndex = 0;

                        _selectedSong = ListBoxSongs.SelectedItem as Beatmap;

                        Play();
                        break;
                }
            }
            else
            {
                if (ListBoxSongs.Items == null)
                    return;
                else if (ListBoxSongs.SelectedItem == null)
                    ListBoxSongs.SelectedIndex = 0;

                _selectedSong = ListBoxSongs.SelectedItem as Beatmap;

                Play();
            }
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            NextSong();
        }

        private void ToggleButtonShuffle_Checked(object sender, RoutedEventArgs e)
        {
            Shuffle = true;
        }

        private void ToggleButtonShuffle_Unchecked(object sender, RoutedEventArgs e)
        {
            Shuffle = false;
        }

        private void ToggleButtonRepeat_Checked(object sender, RoutedEventArgs e)
        {
            Repeat = true;
        }

        private void ToggleButtonRepeat_Unchecked(object sender, RoutedEventArgs e)
        {
            Repeat = false;
        }

        private void SliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_player != null)
            {
                float sliderValue = (float)e.NewValue;

                if (AppSettings.ExponentialVolume)
                    _player.Volume = GetExponantialVolume(sliderValue);
                else
                    _player.Volume = sliderValue;

                AppSettings.Volume = _player.Volume;
                LabelVolume.Content = $"{(_player.Volume * 100).ToString("0")}";
            }
        }

        // Updates slider value based on current volume and volume mode
        public void UpdateSliderVolume()
        {
            if (AppSettings.ExponentialVolume)
                SliderVolume.Value = GetSliderValueFromExponantialVolume(_player.Volume);
            else
                SliderVolume.Value = _player.Volume;
        }

        private void SliderDuration_ValueChanged(TimeSpan newValue)
        {
            if (_player != null)
            {
                try
                {
                    _player.Stop();
                    _player.CurrentTime = newValue;
                    _player.Play();
                }
                catch
                {
                    // TODO: Добавить определение ошибки
                }
            }
        }

        private void StackPanelVolume_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            SliderVolume.Value += Math.Sign(e.Delta) * 0.02;
        }

        private void ListBoxSongs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ListBoxSongs.Items == null || ListBoxSongs.SelectedItem == null)
                return;

            if (_selectedSong != null)
                AddToHistory(_selectedSong.ID);

            _selectedSong = ListBoxSongs.SelectedItem as Beatmap;

            Play();
        }

        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            LoadMenu(MenuSection.Settings);
        }

        private void ButtonAbout_Click(object sender, RoutedEventArgs e)
        {
            LoadMenu(MenuSection.About);
        }

        private void ButtonRefreshLibrary_Click(object sender, RoutedEventArgs e)
        {
            if (_player != null && _player.Mode == PlayMode.Play)
            {
                // TODO: Make so after reset player will be unloaded.
                _player.Stop();
            }
            ListBoxSongs.ItemsSource = null;

            _manager.Load(AppSettings.SongsPath);
        }

        private void TextBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(TextBoxSearch.Text) && ListBoxSongs.ItemsSource != null)
                {
                    // If there is entered text in search box use filter.
                    ICollectionView cv = CollectionViewSource.GetDefaultView(ListBoxSongs.ItemsSource);
                    string filter = TextBoxSearch.Text.ToLower();

                    cv.Filter = o => {
                        Beatmap p = o as Beatmap;
                        string data = $"{p.Title.ToLower()} {p.Artist.ToLower()} {p.Creator.ToLower()}";

                        if (data.Contains(filter) && filter.Length > 0)
                            return true;

                        return false;
                    };
                } else if (ListBoxSongs.ItemsSource != null)
                {
                    // If ItemSource exists, but text are not entered reset filter
                    ICollectionView cv = CollectionViewSource.GetDefaultView(ListBoxSongs.ItemsSource);
                    cv.Filter = null;
                }
            }
            catch
            {
                return;
            }
        }
        #endregion

        #region Event Handlers
        private void WindowMain_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(AppSettings.GamePath))
            {
                string _path = GetGamePath();

                if (!string.IsNullOrEmpty(_path) && Directory.Exists(_path))
                    AppSettings.GamePath = _path;
            }

            _manager = new LibraryManager();
            _manager.LoadingStarted += _manager_LoadingStarted;
            _manager.LoadingChanged += _manager_LoadingProgressChanged;
            _manager.LoadingCompleted += _manager_LoadingProgressCompleted;
            _manager.SavingStarted += _manager_SavingStarted;
            _manager.SavingChanged += _manager_SavingChanged;
            _manager.SavingCompleted += _manager_SavingCompleted;

            if (_manager.PlaylistExist())
            {
                _manager.LoadPlaylist();
            }
            else if (Directory.Exists(AppSettings.SongsPath))
                _manager.Load(AppSettings.SongsPath);
            else
                loadingFailed = true;
        }

        private void WindowMain_ContentRendered(object sender, EventArgs e)
        {
            if (loadingFailed)
                MessageBox.Show("Directory not found! Try changing it manually.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void WindowMain_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (AppSettings.OverlayMode == WindowMode.Window && isOverlayChanging == false)
            {
                _lastWindowWidth = this.Width;
                _lastWindowHeight = this.Height;
            }
        }

        private void WindowMain_Closing(object sender, CancelEventArgs e)
        {
            if (AppSettings.OverlayMode == WindowMode.Window)
            {
                AppSettings.WindowWidth = this.Width;
                AppSettings.WindowHeight = this.Height;
            }
            else
            {
                AppSettings.WindowWidth = _lastWindowWidth;
                AppSettings.WindowHeight = _lastWindowHeight;
            }

            if (ListBoxSongs.HasItems)
                _manager.SavePlaylist(ListBoxSongs.ItemsSource as List<Beatmap>);
        }

        private void LanguageChanged(Object sender, EventArgs e)
        {
            CultureInfo currLang = App.Language;
        }


        private void _player_ModeChanged(PlayMode newMode)
        {
            string packUri = string.Empty;

            switch (newMode)
            {
                case PlayMode.Play:
                    ButtonPlay.Tag = "Play";
                    break;
                case PlayMode.Pause:
                    ButtonPlay.Tag = "Stop";
                    break;
                case PlayMode.Stop:
                    ButtonPlay.Tag = "Stop";
                    break;
                case PlayMode.Unloaded:
                    ButtonPlay.Tag = "Stop";
                    break;
            }
        }

        private void _player_SongEnded()
        {
            NextSong(true);
        }

        private void _manager_LoadingStarted(LoadingStartedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                AnimateProgress(AnimationProgressStage.Active);

                LabelProgress.Visibility = Visibility.Visible;
            });
        }

        private void _manager_LoadingProgressChanged(LoadingChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                LabelProgress.Content = $"{e.Current} / {e.Total}";
            });
        }

        private void _manager_LoadingProgressCompleted(LoadingCompletedEventArgs e)
        {
            if (e.Status == Status.Completed)
            {
                ListBoxSongs.ItemsSource = e.Beatmaps;
            }

            Dispatcher.Invoke(() =>
            {
                AnimateProgress(AnimationProgressStage.Finished);
                LabelProgress.Visibility = Visibility.Hidden;
            });
        }

        private void _manager_SavingStarted(SavingStartedEventArgs e)
        {
            
        }

        private void _manager_SavingChanged(SavingChangedEventArgs e)
        {
            
        }

        private void _manager_SavingCompleted(SavingCompletedEventArgs e)
        {
            
        }

        private void TitleBar_OverlayModeChanged(WindowMode mode)
        {
            AppSettings.OverlayMode = mode;

            WindowModeSetup(AppSettings.OverlayMode);
        }
        #endregion

        #region Utility
        
        private float GetExponantialVolume(float value)
        {
            float a = 0.001f;
            float volume = (float)(a * Math.Exp(6.909f * value)) - a;
            return volume;
        }

        private float GetSliderValueFromExponantialVolume(float volume)
        {
            if (volume >= 1)
                return 1;

            float a = 0.001f;
            float value = (float)(Math.Log((volume + a) / a)) / 6.909f;
            return value;
        }

        private string GetGamePath()
        {
            RegistryKey parentKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
            string[] nameList = parentKey.GetSubKeyNames();
            for (int i = 0; i < nameList.Length; i++)
            {
                RegistryKey regKey = parentKey.OpenSubKey(nameList[i]);
                try
                {
                    var val = regKey.GetValue("DisplayName");
                    if (val != null && regKey.GetValue("DisplayName").ToString() == "osu!")
                    {
                        if (regKey.GetValue("DisplayIcon") != null)
                            return System.IO.Path.GetDirectoryName(regKey.GetValue("DisplayIcon").ToString());
                    }
                }
                catch { }
            }
            return "";
        }

        private void AddToHistory(int songId)
        {
            if (_history == null)
                _history = new Stack<int>();

            if (_history.Count == 0)
            {
                _history.Push(songId);
            }
            else if(_history.Peek() != songId)
            {
                _history.Push(songId);
            }
        }

        private Beatmap GetFromHistory()
        {
            if (_history != null && _history.Count > 0)
            {
                int id = _history.Pop();
                var source = ListBoxSongs.ItemsSource as IEnumerable<Beatmap>;

                Beatmap item = source.Where(x => x.ID == id).FirstOrDefault();
                
                if (item != null)
                {
                    return item;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        private void UpdateInformation()
        {
            try
            {
                ImageBackground.Source = new BitmapImage(new Uri(_selectedSong.PathToBackground, UriKind.Absolute));
            }
            catch
            {
                ImageBackground.Source = new BitmapImage(new Uri(@"pack://application:,,,/Resources/Images/Player/player_background.jpg"));
            }

            SliderDuration.Maximum = _player.TotalTime;
            SliderDuration.Value = TimeSpan.Zero;
            LabelTitle.Content = _selectedSong.Title;
            LabelArtist.Content = _selectedSong.Artist;

            this.Title = $"{_selectedSong.Artist} - {_selectedSong.Title}";

            if (!ListBoxSongs.IsMouseOver && ListBoxSongs.Items.Contains(_selectedSong))
                ListBoxSongs.ScrollIntoView(_selectedSong);
        }

        private bool Play()
        {
            try
            {
                _player.Load(_selectedSong.PathToAudio);
                UpdateInformation();
                _player.Play();
                return true;
            }
            catch
            {
                return false;
            }                
        }

        private void NextSong(bool repeatEnabled = false)
        {
            if (ListBoxSongs.Items == null)
                return;

            if (_player != null)
                _player.Stop();

            if (Repeat && repeatEnabled)
            {
                Play();
                return;
            }

            int newId = 0;

            if (ListBoxSongs.SelectedItem != null)
            {
                if (_selectedSong != null)
                    AddToHistory(_selectedSong.ID);


                if (Shuffle)
                {
                    do newId = new Random().Next(0, ListBoxSongs.Items.Count);
                    while (newId == ListBoxSongs.SelectedIndex);
                }
                else
                {
                    if (ListBoxSongs.SelectedIndex == ListBoxSongs.Items.Count - 1)
                        newId = 0;
                    else
                        newId = ListBoxSongs.SelectedIndex + 1;
                }
            }
            else
            {
                newId = 0;
            }

            ListBoxSongs.SelectedIndex = newId;

            _selectedSong = ListBoxSongs.SelectedItem as Beatmap;

            if(!Play())
                NextSong();
        }
        #endregion

        #region Timers
        private void _timerDuration_Tick(object sender, EventArgs e)
        {
            if (_player != null)
            {
                TimeSpan currentPosition = _player.CurrentTime;
                TimeSpan totalLength = _player.TotalTime;

                SliderDuration.Value = currentPosition;
                LabelDuration.Content = $"{currentPosition.ToString("mm\\:ss")}/{totalLength.ToString("mm\\:ss")}";
            }
            else
            {
                LabelDuration.Content = $"00:00/00:00";
            }
        }
        #endregion

        #region Fullscreen bug fix
        // Thanks to LesterLobo: 
        // https://blogs.msdn.microsoft.com/llobo/2006/08/01/maximizing-window-with-windowstylenone-considering-taskbar/
        // and
        // https://stackoverflow.com/questions/6890472/wpf-maximize-window-with-windowstate-problem-application-will-hide-windows-ta

        private void WindowMain_SourceInitialized(object sender, EventArgs e)
        {
            IntPtr handle = (new WindowInteropHelper(this)).Handle;
            HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WindowProc));
            MinWidth = 400;
        }

        private static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = false;
                    break;
            }

            return (IntPtr)0;
        }

        private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {

            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

            // Adjust the maximized size and position to fit the work area of the correct monitor
            int MONITOR_DEFAULTTONEAREST = 0x00000002;
            IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

            if (monitor != IntPtr.Zero)
            {
                MONITORINFO monitorInfo = new MONITORINFO();
                GetMonitorInfo(monitor, monitorInfo);
                RECT rcWorkArea = monitorInfo.rcWork;
                RECT rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
            }

            Marshal.StructureToPtr(mmi, lParam, true);
        }

        /// <summary>
        /// POINT aka POINTAPI
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            /// <summary>
            /// x coordinate of point.
            /// </summary>
            public int x;
            /// <summary>
            /// y coordinate of point.
            /// </summary>
            public int y;

            /// <summary>
            /// Construct a point of coordinates (x,y).
            /// </summary>
            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };

        /// <summary>
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MONITORINFO
        {
            /// <summary>
            /// </summary>            
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));

            /// <summary>
            /// </summary>            
            public RECT rcMonitor = new RECT();

            /// <summary>
            /// </summary>            
            public RECT rcWork = new RECT();

            /// <summary>
            /// </summary>            
            public int dwFlags = 0;
        }

        /// <summary> Win32 </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct RECT
        {
            /// <summary> Win32 </summary>
            public int left;
            /// <summary> Win32 </summary>
            public int top;
            /// <summary> Win32 </summary>
            public int right;
            /// <summary> Win32 </summary>
            public int bottom;

            /// <summary> Win32 </summary>
            public static readonly RECT Empty = new RECT();

            /// <summary> Win32 </summary>
            public int Width
            {
                get { return Math.Abs(right - left); }  // Abs needed for BIDI OS
            }

            /// <summary> Win32 </summary>
            public int Height
            {
                get { return bottom - top; }
            }

            /// <summary> Win32 </summary>
            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }

            /// <summary> Win32 </summary>
            public RECT(RECT rcSrc)
            {
                this.left = rcSrc.left;
                this.top = rcSrc.top;
                this.right = rcSrc.right;
                this.bottom = rcSrc.bottom;
            }

            /// <summary> Win32 </summary>
            public bool IsEmpty
            {
                get
                {
                    // BUGBUG : On Bidi OS (hebrew arabic) left > right
                    return left >= right || top >= bottom;
                }
            }

            /// <summary> Return a user friendly representation of this struct </summary>
            public override string ToString()
            {
                if (this == RECT.Empty) { return "RECT {Empty}"; }
                return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom + " }";
            }

            /// <summary> Determine if 2 RECT are equal (deep compare) </summary>
            public override bool Equals(object obj)
            {
                if (!(obj is Rect)) { return false; }
                return (this == (RECT)obj);
            }

            /// <summary>Return the HashCode for this struct (not garanteed to be unique)</summary>
            public override int GetHashCode()
            {
                return left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();
            }

            /// <summary> Determine if 2 RECT are equal (deep compare)</summary>
            public static bool operator ==(RECT rect1, RECT rect2)
            {
                return (rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom);
            }

            /// <summary> Determine if 2 RECT are different(deep compare)</summary>
            public static bool operator !=(RECT rect1, RECT rect2)
            {
                return !(rect1 == rect2);
            }
        }

        [DllImport("user32")]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        [DllImport("User32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);
        #endregion
    }
}
