using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace osu_Library.Classes
{
    class LibraryManager
    {
        public bool IsBusy { get; private set; }

        private List<Beatmap> _beatmaps;
        private Dictionary<string, bool> _mapImportStatus;
        private string _path;

        private int _total;
        private int _current;

        private BackgroundWorker _workerLoading;
        private BackgroundWorker _workerLoadingPlaylist;
        private BackgroundWorker _workerSavingPlaylist;

        #region Events
        public delegate void LoadingStartedEventHandler(LoadingStartedEventArgs e);
        public event LoadingStartedEventHandler LoadingStarted;

        public delegate void LoadingChangedEventHandler(LoadingChangedEventArgs e);
        public event LoadingChangedEventHandler LoadingChanged;

        public delegate void LoadingCompletedEventHandler(LoadingCompletedEventArgs e);
        public event LoadingCompletedEventHandler LoadingCompleted;

        public delegate void SavingStartedEventHandler(SavingStartedEventArgs e);
        public event SavingStartedEventHandler SavingStarted;

        public delegate void SavingChangedEventHandler(SavingChangedEventArgs e);
        public event SavingChangedEventHandler SavingChanged;

        public delegate void SavingCompletedEventHandler(SavingCompletedEventArgs e);
        public event SavingCompletedEventHandler SavingCompleted;
        #endregion

        public LibraryManager()
        {
            _beatmaps = new List<Beatmap>();
            _mapImportStatus = new Dictionary<string, bool>();

            _workerLoading = new BackgroundWorker
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = true
            };

            _workerLoadingPlaylist = new BackgroundWorker
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = true
            };

            _workerSavingPlaylist = new BackgroundWorker
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = true
            };

            _workerLoading.DoWork += _workerLoading_DoWork;
            _workerLoading.RunWorkerCompleted += _workerLoading_RunWorkerCompleted;
            _workerLoadingPlaylist.DoWork += _workerLoadingPlaylist_DoWork;
            _workerLoadingPlaylist.RunWorkerCompleted += _workerLoadingPlaylist_RunWorkerCompleted;
            _workerSavingPlaylist.DoWork += _workerSavingPlaylist_DoWork;
            _workerSavingPlaylist.RunWorkerCompleted += _workerSavingPlaylist_RunWorkerCompleted;

            IsBusy = false;
        }

        public void Load(string pathToSongDirectory)
        {
            if (!IsBusy)
            {
                _path = pathToSongDirectory;

                _workerLoading.RunWorkerAsync();
            }
        }

        public void LoadPlaylist()
        {
            if (!IsBusy)
            {
                _workerLoadingPlaylist.RunWorkerAsync();
            }
        }

        public void SavePlaylist(List<Beatmap> beatmaps)
        {
            if (!IsBusy)
            {
                _beatmaps = beatmaps;

                _workerSavingPlaylist.RunWorkerAsync();
            }
        }

        public bool PlaylistExist()
        {
            if (AppDataManager.Exists("playlist.ospl"))
                return true;
            else
                return false;
        }

        #region Background workers
        private void _workerLoading_DoWork(object sender, DoWorkEventArgs e)
        {
            IsBusy = true;

            _beatmaps = new List<Beatmap>();
            _mapImportStatus = new Dictionary<string, bool>();

            var song_dirs = Directory.GetDirectories(_path);

            _current = 0;
            _total = song_dirs.Length;

            LoadingStarted?.Invoke(new LoadingStartedEventArgs());

            foreach (string song_dir in song_dirs)
            {
                if (_workerLoading.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                string song_file = Directory.GetFiles(song_dir, "*.osu").FirstOrDefault();

                if (song_file != null || song_file != string.Empty)
                {
                    try
                    {
                        Beatmap beatmap = new Beatmap(song_file);

                        if (beatmap.ID != -1)
                        {
                            _beatmaps.Add(beatmap);

                            _mapImportStatus.Add(song_dir, true);
                        }
                        else
                        {
                            _mapImportStatus.Add(song_dir, false);
                        }
                    }
                    catch
                    {
                        _mapImportStatus.Add(song_dir, false);
                    }
                }

                _current++;
                LoadingChanged?.Invoke(new LoadingChangedEventArgs { Total = _total, Current = _current });
            }

            _beatmaps = _beatmaps.OrderBy(x => x.Title).ToList();
        }

        private void _workerLoading_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var ea = new LoadingCompletedEventArgs
            {
                Beatmaps = new List<Beatmap>(),
                ImportStatus = new Dictionary<string, bool>()
            };

            if (e.Cancelled)
                ea.Status = Status.Cancelled;
            else if (e.Error != null)
                ea.Status = Status.Error;
            else
            {
                ea.Status = Status.Completed;
                ea.Beatmaps = _beatmaps;
                ea.ImportStatus = _mapImportStatus;
            }

            LoadingCompleted?.Invoke(ea);
            IsBusy = false;
        }

        private void _workerLoadingPlaylist_DoWork(object sender, DoWorkEventArgs e)
        {
            IsBusy = true;

            if (!AppDataManager.Exists("playlist.ospl"))
            {
                e.Cancel = true;
                return;
            }

            _beatmaps = new List<Beatmap>();

            string[] songs = AppDataManager.Load("playlist.ospl");

            _current = 0;
            _total = songs.Length;

            SavingStarted?.Invoke(new SavingStartedEventArgs());

            foreach (string song in songs)
            {
                if (_workerLoadingPlaylist.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                if (string.IsNullOrEmpty(song))
                    continue;

                var info = Regex.Split(song, @"\|\>\<\|");

                try
                {
                    Beatmap beatmap = new Beatmap
                    {
                        ID = Convert.ToInt32(info[0]),
                        Artist = info[1],
                        Title = info[2],
                        Creator = info[3],
                        PathToAudio = info[4],
                        PathToBackground = info[5]
                    };

                    _beatmaps.Add(beatmap);
                }
                catch { }

                _current++;
                SavingChanged?.Invoke(new SavingChangedEventArgs { Total = _total, Current = _current });
            }

            _beatmaps = _beatmaps.OrderBy(x => x.Title).ToList();
        }

        private void _workerLoadingPlaylist_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var ea = new LoadingCompletedEventArgs
            {
                Beatmaps = new List<Beatmap>(),
                ImportStatus = new Dictionary<string, bool>()
            };

            if (e.Cancelled)
                ea.Status = Status.Cancelled;
            else if (e.Error != null)
                ea.Status = Status.Error;
            else
            {
                ea.Status = Status.Completed;
                ea.Beatmaps = _beatmaps;
                ea.ImportStatus = _mapImportStatus;
            }

            LoadingCompleted?.Invoke(ea);
            IsBusy = false;
        }

        private void _workerSavingPlaylist_DoWork(object sender, DoWorkEventArgs e)
        {
            IsBusy = true;

            if (_beatmaps == null)
            {
                e.Cancel = true;
                return;
            }

            _total = _beatmaps.Count;
            _current = 0;

            StringBuilder playlist = new StringBuilder();
            foreach (Beatmap map in _beatmaps)
            {
                if (_workerSavingPlaylist.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                string info = $"{map.ID}|><|{map.Artist}|><|{map.Title}|><|{map.Creator}|><|{map.PathToAudio}|><|{map.PathToBackground}";

                playlist.AppendLine(info);
                _current++;
            }

            AppDataManager.Save(playlist.ToString(), "playlist.ospl");
        }

        private void _workerSavingPlaylist_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var ea = new SavingCompletedEventArgs
            {
                Status = Status.Completed,
                Total = 0,
                Saved = 0
            };

            if (e.Cancelled)
                ea.Status = Status.Cancelled;
            else if (e.Error != null)
                ea.Status = Status.Error;
            else
            {
                ea.Status = Status.Completed;
                ea.Total = _total;
                ea.Saved = _current;
            }

            SavingCompleted?.Invoke(ea);
            IsBusy = false;
        }
        #endregion
    }

    class LoadingStartedEventArgs : EventArgs
    {

    }

    class LoadingChangedEventArgs : EventArgs
    {
        public int Total { get; set; }
        public int Current { get; set; }
    }

    class LoadingCompletedEventArgs : EventArgs
    {
        public Status Status { get; set; }
        public List<Beatmap> Beatmaps { get; set; }
        public Dictionary<string, bool> ImportStatus { get; set; }
    }

    class SavingStartedEventArgs : EventArgs
    {

    }

    class SavingChangedEventArgs : EventArgs
    {
        public int Total { get; set; }
        public int Current { get; set; }
    }

    class SavingCompletedEventArgs : EventArgs
    {
        public Status Status { get; set; }
        public int Saved { get; set; }
        public int Total { get; set; }
    }

    public enum Status
    {
        Cancelled,
        Completed,
        Error
    }
}
