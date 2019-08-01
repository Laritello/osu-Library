using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_Library.Classes
{
    class AudioPlayback : IDisposable
    {
        private IWavePlayer playbackDevice;
        private WaveStream fileStream;
        private float _volume;

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
                return playbackDevice.Volume;
            }
            set
            {
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

        public event EventHandler<StoppedEventArgs> PlayBackStopped;

        protected virtual void OnPlaybackStopped(StoppedEventArgs e)
        {
            PlayBackStopped?.Invoke(this, e);
        }

        public void Load(string fileName)
        {
            Stop();
            CloseFile();
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

        private void OpenFile(string fileName)
        {
            try
            {
                var inputStream = new AudioFileReader(fileName);
                fileStream = inputStream;
                var aggregator = new SampleAggregator(inputStream);
                aggregator.NotificationCount = inputStream.WaveFormat.SampleRate / 100;
                aggregator.PerformFFT = true;
                aggregator.FftCalculated += (s, a) => OnFftCalculated(a);
                aggregator.MaximumCalculated += (s, a) => OnMaximumCalculated(a);
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
            }
        }

        public void Pause()
        {
            if (playbackDevice != null)
            {
                playbackDevice.Pause();
            }
        }

        public void Stop()
        {
            if (playbackDevice != null)
            {
                playbackDevice.Stop();
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
            if (playbackDevice != null)
            {
                playbackDevice.Dispose();
                playbackDevice = null;
            }
        }
    }
}
