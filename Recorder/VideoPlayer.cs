using System;
using Xamarin.Forms;

using Recorder.Models;
using System.Diagnostics;
using System.Timers;

namespace Recorder
{
    public class VideoPlayer : View
    {
        public VideoPlayer()
        {
        }

        public event EventHandler ResetToStartRequested;

        public void Reset()
        {
            ResetToStartRequested?.Invoke(this, EventArgs.Empty);
        }

        public static readonly BindableProperty SourceProperty =
            BindableProperty.Create(nameof(Source), typeof(VideoSource), typeof(VideoPlayer), null);

        [TypeConverter(typeof(VideoSourceConverter))]
        public VideoSource Source
        {
            set => SetValue(SourceProperty, value);
            get => (VideoSource)GetValue(SourceProperty);
        }

        public static readonly BindableProperty IsMutedProperty =
            BindableProperty.Create(nameof(IsMuted), typeof(bool), typeof(VideoPlayer), true);

        public bool IsMuted
        {
            set => SetValue(IsMutedProperty, value);
            get => (bool)GetValue(IsMutedProperty);
        }

        // play=true starts video, false stops
        // this is implemented as a property to allow data binding directly from viewmodel
        // otherwise maybe a delegate action or some other direct method would work
        public static readonly BindableProperty PlayProperty = BindableProperty.Create(
            nameof(Play), typeof(bool), typeof(VideoPlayer),
            propertyChanged: OnPlayPropertyChanged
            );

        public bool Play
        {
            set => SetValue(PlayProperty, value);
            get => (bool)GetValue(PlayProperty);
        }

        // video start offset time in seconds
        public static readonly BindableProperty StartTimeProperty =
            BindableProperty.Create(nameof(StartTime), typeof(int), typeof(VideoPlayer));

        public int StartTime
        {
            set => SetValue(StartTimeProperty, value);
            get => (int)GetValue(StartTimeProperty);
        }

        // end offset time in seconds
        public static readonly BindableProperty EndTimeProperty =
            BindableProperty.Create(nameof(EndTime), typeof(int), typeof(VideoPlayer));

        public int EndTime
        {
            set => SetValue(EndTimeProperty, value);
            get => (int)GetValue(EndTimeProperty);
        }

        private Timer endMonitor;

        private void StartMonitoringEnd()
        {
            StopMonitoringEnd();

            Debug.WriteLine(string.Format("start monitoring end {0}", EndTime), "VideoPlayer");

            if (EndTime > 0)
            {
                int duration = EndTime - StartTime;

                endMonitor = new Timer(1000 * duration)
                {
                    AutoReset = false  // fire only once
                };

                endMonitor.Elapsed += (o, e) => Play = false;
                endMonitor.Start();
            }
        }

        private void StopMonitoringEnd()
        {
            if (endMonitor != null)
            {
                Debug.WriteLine("clearing timer", "VideoPlayer");
                endMonitor.Stop();
                endMonitor.Close();
                endMonitor = null;
            }
        }

        private static void OnPlayPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is VideoPlayer player && newValue is bool playValue)
            {
                if (playValue == true)
                {
                    Debug.WriteLine("play changed to true", "VideoPlayer");
                    player.StartMonitoringEnd();
                }
                else
                {
                    Debug.WriteLine("play changed to false", "VideoPlayer");
                    player.StopMonitoringEnd();
                }
            }
        }
    }
}
