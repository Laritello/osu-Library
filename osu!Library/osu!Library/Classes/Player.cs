using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_Library.Classes
{
    class Player : IDisposable
    {
        public float Volume
        {
            get
            {
                return _volume;
            }
            set
            {
                if (value > 1)
                    _volume = 1;
                else if (value < 0)
                    _volume = 0;
                else
                    _volume = value;

                if (!Muted)
                    SetVolume(_volume);
            }
        }
        public bool Muted
        {
            get
            {
                return _muted;
            }
            set
            {
                _muted = value;

                if (_muted)
                    SetVolume(0);
                else
                    SetVolume(Volume);
            }
        }
        public PlayMode Mode
        {
            get
            {
                return _mode;
            }
            private set
            {
                _mode = value;
                ModeChanged?.Invoke(_mode);
            }
        }

        private float _volume;
        private bool _muted;
        private string _pathToSong;
        private PlayMode _mode;
        private WaveOutEvent _player;
        private AudioFileReader _songFile;

        public delegate void SongEndedEventHandler();
        public event SongEndedEventHandler SongEnded;

        public delegate void ModeChangedEventHandler(PlayMode newMode);
        public event ModeChangedEventHandler ModeChanged;

        public Player()
        {
            _player = new WaveOutEvent();
            Volume = 100;
            Mode = PlayMode.Unloaded;
        }

        public bool LoadSong(string path)
        {
            if (!System.IO.File.Exists(path))
                return false;

            ResetPlayer();

            try
            {
                _songFile = new AudioFileReader(path);
            }
            catch
            {
                return false;
            }

            try
            {
                _player = new WaveOutEvent();
                _player.PlaybackStopped += PlaybackStopped;

                if (Muted)
                    SetVolume(0);
                else
                    SetVolume(Volume);

                _player.Init(_songFile);
            }
            catch
            {
                return false;
            }

            _pathToSong = path;
            return true;
        }

        public void Play()
        {
            try
            {
                if (_player != null)
                {
                    _player.Play();
                }
                else if (!string.IsNullOrEmpty(_pathToSong))
                {
                    LoadSong(_pathToSong);
                    _player.Play();
                }
                else
                {
                    Mode = PlayMode.Stop;
                    return;
                }

                Mode = PlayMode.Play;
            }
            catch
            {
                ResetPlayer();
            }
        }

        public void Pause()
        {
            if (_player != null)
            {
                try
                {
                    _player.Pause();
                    Mode = PlayMode.Pause;
                }
                catch
                {
                    ResetPlayer();
                }
            }    
        }

        public void Stop()
        {
            if (_player != null)
            {
                try
                {
                    _player.Stop();
                    Mode = PlayMode.Stop;
                }
                catch
                {
                    ResetPlayer();
                }
            }
        }

        public TimeSpan GetTotalLength()
        {
            try
            {
                if (_songFile == null)
                    return TimeSpan.Zero;

                return _songFile.TotalTime;
            }
            catch
            {
                return TimeSpan.Zero;
            }
        }

        public TimeSpan GetCurrentPosition()
        {
            try
            {
                if (_songFile == null)
                    return TimeSpan.Zero;

                return _songFile.CurrentTime;
            }
            catch
            {
                return TimeSpan.Zero;
            }
        }

        public void SetCurrentPosition(TimeSpan pos)
        {
            try
            {
                if (_songFile != null)
                    _songFile.CurrentTime = pos;
            }
            catch { }
        }

        private void SetVolume(float value)
        {
            try
            {
                if (_player != null)
                    _player.Volume = value;
            }
            catch { }
        }

        private void ResetPlayer()
        {
            if (_player != null)
            {
                try
                {
                    _player.Dispose();
                    _player = null;
                }
                catch { }
            }

            if (_songFile != null)
            {
                try
                {
                    _songFile.Dispose();
                    _songFile = null;
                }
                catch { }
            }

            Mode = PlayMode.Stop;
        }

        private void PlaybackStopped(object sender, StoppedEventArgs e)
        {
            try
            {
                if (_songFile.CurrentTime >= _songFile.TotalTime)
                    SongEnded?.Invoke();
            }
            catch { }
        }

        #region IDisposable
        bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                ResetPlayer();
            }

            disposed = true;
        }
        #endregion
    }
}
