using System;
using System.Diagnostics;
using System.IO;
using AVFoundation;
using Foundation;
using Recorder.iOS;
using Recorder.Models;
using Recorder.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(AudioRecorder))]
namespace Recorder.iOS
{
    public class AudioRecorder : IAudioRecorder
    {
        const int BIT_DEPTH = 16;
        const int SAMPLE_RATE = 44100;
        const int CHANNEL_COUNT = 2;

        // Settings for MPEG-4/AAC
        //const string FILE_EXTENSION = "m4a";
        //const string MIME_TYPE = "audio/m4a";
        //const int AUDIO_FORMAT = (int)AudioToolbox.AudioFormatType.MPEG4AAC;

        // Settings for FLAC
        const string FILE_EXTENSION = "flac";
        const string MIME_TYPE = "audio/flac";
        const int AUDIO_FORMAT = (int)AudioToolbox.AudioFormatType.Flac;

        AVAudioRecorder recorder;
        NSDictionary settings;
        AudioFile output;

        public AudioRecorder()
        {
        }

        bool IAudioRecorder.IsRecording
        {
            get => recorder?.Recording ?? false;
        }

        public string Prepare()
        {
            var recId = Guid.NewGuid().ToString();
            var audioSession = AVAudioSession.SharedInstance();
            var err = audioSession.SetCategory(AVAudioSessionCategory.PlayAndRecord);

            if (err != null)
            {
                throw new RecordingException($"Unable to set category: {err}");
            }

            err = audioSession.SetActive(true);

            if (err != null)
            {
                throw new RecordingException($"Unable to set audio session active: {err}");
            }

            string fileName = $"{recId}.{FILE_EXTENSION}";

            // Put the recordings into the Documents folder
            var documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string recordingFilename = Path.Combine(documentsFolder, fileName);
            var audioFilePath = NSUrl.FromFilename(recordingFilename);

            output = new AudioFile() {
                FileName = fileName,
                BitDepth = BIT_DEPTH,
                SampleRate = SAMPLE_RATE,
                NumberOfChannels = CHANNEL_COUNT,
                ContentType = MIME_TYPE,
            };

            settings = new NSDictionary(
                AVAudioSettings.AVSampleRateKey,
                NSNumber.FromInt32(SAMPLE_RATE),

                AVAudioSettings.AVFormatIDKey,
                NSNumber.FromInt32(AUDIO_FORMAT),

                AVAudioSettings.AVNumberOfChannelsKey,
                NSNumber.FromInt32(CHANNEL_COUNT),

                AVAudioSettings.AVLinearPCMBitDepthKey,
                NSNumber.FromInt32(BIT_DEPTH),

                AVAudioSettings.AVLinearPCMIsBigEndianKey,
                NSNumber.FromBoolean(false),

                AVAudioSettings.AVLinearPCMIsFloatKey,
                NSNumber.FromBoolean(false)
            );

            // Set recorder parameters
            recorder = AVAudioRecorder.Create(audioFilePath, new AudioSettings(settings), out err);

            if (recorder == null)
            {
                throw new RecordingException($"Unable to create audio recorder: {err}");
            }

            // Set Recorder to Prepare To Record
            if (!recorder.PrepareToRecord())
            {
                recorder.Dispose();
                recorder = null;
                throw new RecordingException("AVAudioRecorder.PrepareToRecord() failed");
            }

            recorder.FinishedRecording += delegate (object sender, AVStatusEventArgs e) {
                recorder?.Dispose();
                recorder = null;
                Debug.WriteLine($"Done recording (status: {e.Status})");
            };

            return recId;
        }

        public void Start()
        {
            if (recorder != null && output != null)
            {
                recorder.Record();
                output.CreatedOn = DateTime.UtcNow;
            }
            else
            {
                throw new RecordingException("Unable to start, recording has not been prepared");
            }
        }

        public AudioFile Stop()
        {
            if (recorder?.Recording == true && output != null)
            {
                try
                {
                    // duration needs to be captured before the recorder is stopped
                    output.Duration = recorder.currentTime;

                    recorder.Stop();
                    recorder.Dispose();

                    // reset the category to prevent nearly muted video playback
                    AVAudioSession.SharedInstance().SetCategory(AVAudioSessionCategory.Playback);

                    return output;
                }
                finally
                {
                    recorder = null;
                    output = null;
                }
            }

            throw new RecordingException("Unable to stop, recording has not been started yet");
        }
    }
}
