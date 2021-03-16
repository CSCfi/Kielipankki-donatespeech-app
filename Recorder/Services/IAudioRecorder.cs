using System;
using Recorder.Models;

namespace Recorder.Services
{
    public class RecordingException : Exception
    {
        public RecordingException(string message) : base(message)
        {
        }
    }

    public interface IAudioRecorder
    {
        /**
         * Prepare the recorder and return the ID for the new recording
         */
        string Prepare();

        /**
         * Start recording
         */
        void Start();

        /**
         * Stop the recording and return the finished audio file
         */
        AudioFile Stop();

        bool IsRecording { get; }
    }
}
