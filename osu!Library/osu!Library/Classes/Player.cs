﻿using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_Library.Classes
{
    class Player
    {
        private IWavePlayer playbackDevice;
        private WaveStream fileStream;
        private float _volume;
        private bool _muted;
        private PlayMode _mode;

        public PlayMode Mode
        {
            get
            {
                return _mode;
            }

            set
            {
                _mode = value;
                ModeChanged?.Invoke(_mode);
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
            }
        }

        public delegate void SongEndedEventHandler();
        public event SongEndedEventHandler SongEnded;

        public delegate void ModeChangedEventHandler(PlayMode newMode);
        public event ModeChangedEventHandler ModeChanged;

        public TimeSpan TotalTime
        {
            get
            {
                if (fileStream != null)
                    return fileStream.TotalTime;
                else
                    return TimeSpan.Zero;
            }
        }

        public TimeSpan CurrentTime
        {
            get
            {
                if (fileStream != null)
                    return fileStream.CurrentTime;
                else
                    return TimeSpan.Zero;
            }

            set
            {
                if (fileStream != null && value != null)
                    fileStream.CurrentTime = value;
            }
        }

        public float Volume
        {
            get
            {
                return _volume;
            }
            set
            {
                _volume = value;

                if (playbackDevice != null)
                    playbackDevice.Volume = value;
            }
        }

        public event EventHandler<FftEventArgs> FftCalculated;

        protected virtual void OnFftCalculated(FftEventArgs e)
        {
            FftCalculated?.Invoke(this, e);
        }

        public event EventHandler<MaxSampleEventArgs> MaximumCalculated;

        protected virtual void OnMaximumCalculated(MaxSampleEventArgs e)
        {
            MaximumCalculated?.Invoke(this, e);
        }

        protected virtual void OnPlaybackStopped(StoppedEventArgs e)
        {
            if (fileStream.CurrentTime >= fileStream.TotalTime || (fileStream.TotalTime - fileStream.CurrentTime).TotalMilliseconds < 200)
                SongEnded?.Invoke();
        }

        public void Load(string fileName)
        {
            Stop();
            CloseFile();
            DisposeDevice();
            EnsureDeviceCreated();
            OpenFile(fileName);
        }

        private void CloseFile()
        {
            if (fileStream != null)
            {
                fileStream.Dispose();
                fileStream = null;
            }
        }

        private void DisposeDevice()
        {
            if (playbackDevice != null)
            {
                playbackDevice.Dispose();
                playbackDevice = null;
            }
        }

        private void OpenFile(string fileName)
        {
            try
            {
                var inputStream = new AudioFileReader(fileName);
                fileStream = inputStream;

                var aggregator = new SampleAggregator(inputStream)
                {
                    NotificationCount = inputStream.WaveFormat.SampleRate / 100,
                    PerformFFT = true
                };

                aggregator.FftCalculated += (s, a) => OnFftCalculated(a);
                aggregator.MaximumCalculated += (s, a) => OnMaximumCalculated(a);

                if (playbackDevice != null)
                    playbackDevice.Dispose();

                CreateDevice();

                playbackDevice.Init(aggregator);
                playbackDevice.PlaybackStopped += (s, a) => OnPlaybackStopped(a);
            }
            catch (Exception e)
            {
                CloseFile();
                throw new Exception($"Problem opening file. { e.Message }");
            }
        }

        private void EnsureDeviceCreated()
        {
            if (playbackDevice == null)
            {
                CreateDevice();
            }
        }

        private void CreateDevice()
        {
            playbackDevice = new WaveOut { DesiredLatency = 200 };
        }

        public void Play()
        {
            if (playbackDevice != null && fileStream != null && playbackDevice.PlaybackState != PlaybackState.Playing)
            {
                playbackDevice.Play();
                Mode = PlayMode.Play;
            }
        }

        public void Pause()
        {
            if (playbackDevice != null)
            {
                playbackDevice.Pause();
                Mode = PlayMode.Pause;
            }
        }

        public void Stop()
        {
            if (playbackDevice != null)
            {
                playbackDevice.Stop();
                Mode = PlayMode.Stop;
            }
            if (fileStream != null)
            {
                fileStream.Position = 0;
            }
        }

        public void Dispose()
        {
            Stop();
            CloseFile();
            DisposeDevice();
        }
    }
}
