using System;
namespace Recorder.Models
{
    public class AudioFile
    {
        public string FileName;
        public DateTime CreatedOn;
        public double Duration;
        public int BitDepth;
        public int SampleRate;
        public int NumberOfChannels;
        public string ContentType;
    }
}
