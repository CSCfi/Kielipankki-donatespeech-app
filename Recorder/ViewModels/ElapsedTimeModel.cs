using System;
using System.Timers;

namespace Recorder.ViewModels
{
    public class ElapsedTimeModel
    {
        private Timer timer;
        private DateTime startTime;

        public ElapsedTimeModel()
        {
            timer = new Timer(1000);
            timer.Elapsed += OnElapsed;
        }

        public event EventHandler<SecondElapsedEventArgs> SecondElapsed;

        public void Start()
        {
            startTime = DateTime.Now;
            timer.Start();
        }

        public void Stop()
        {
            timer.Stop();
        }

        private void OnElapsed(object source, ElapsedEventArgs e)
        {
            TimeSpan sinceStart = e.SignalTime - startTime;
            SecondElapsed?.Invoke(this, new SecondElapsedEventArgs(sinceStart));
        }

        public class SecondElapsedEventArgs : EventArgs
        {
            public TimeSpan ElapsedTime { get; }

            public SecondElapsedEventArgs(TimeSpan elapsedTime)
            {
                ElapsedTime = elapsedTime;
            }
        }
    }
}
