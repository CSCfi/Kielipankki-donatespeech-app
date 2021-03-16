using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.Media;
using Java.Lang;
using Recorder.Models;
using Recorder.Services;

namespace Recorder.Droid
{
    public class LowLevelAudioRecorder : IAudioRecorder
    {
        enum State { Idle, Prepared, Recording };
        AudioFile output;
        State currentState = State.Idle;

        const int BIT_RATE = 16;
        const int SAMPLING_RATE = 44100;
        const int CHANNELS = 2;

        // Settings for PCM/WAV:
        const string FILE_EXTENSION = "wav";
        const string MIME_TYPE = "audio/wav";

        string filePath = null;
        byte[] audioBuffer = null;
        AudioRecord audioRecord = null;
        bool endRecording = false;

        public bool IsRecording => currentState == State.Recording;

        private int bufferSize = 0;  // filled in later

        string GetFullPathNameForRecording(string fileName)
        {
            return Path.Combine(
                Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments),
                fileName
            );
        }

        public string Prepare()
        {
            ChannelIn ch = ChannelIn.Mono;
            if (CHANNELS == 2)
            {
                ch = ChannelIn.Stereo;
            }

            bufferSize = AudioRecord.GetMinBufferSize
              (SAMPLING_RATE, ch, Android.Media.Encoding.Pcm16bit) * 3;

            var recordingId = Guid.NewGuid().ToString();
            var fileName = $"{recordingId}.{FILE_EXTENSION}";

            endRecording = false;

            audioBuffer = new System.Byte[bufferSize];

            try
            {
                filePath = GetFullPathNameForRecording(fileName);
        
                audioRecord = new AudioRecord(
                    // Hardware source of recording.
                    AudioSource.Mic,
                    // Frequency
                    SAMPLING_RATE,
                    // Mono or stereo
                    ch,
                    // Audio encoding
                    Android.Media.Encoding.Pcm16bit,
                    // Length of the audio clip.
                    audioBuffer.Length
                );

                output = new AudioFile()
                {
                    FileName = fileName,
                    BitDepth = BIT_RATE,
                    SampleRate = SAMPLING_RATE,
                    NumberOfChannels = CHANNELS,
                    ContentType = MIME_TYPE,
                };

                currentState = State.Prepared;
                return recordingId;
            }
            catch (IllegalStateException e)
            {
                throw new RecordingException(e.ToString());
            }
        }

        public void Start()
        {
            if (currentState == State.Prepared)
            {
                audioRecord.StartRecording();

                // Off line this so that we do not block the UI thread.
                ReadAudio();

                output.CreatedOn = DateTime.UtcNow;
                currentState = State.Recording;
            }
        }

        private async void ReadAudio()
        {
            await ReadAudioAsync();

        }

        async Task ReadAudioAsync()
        {
            using (var fileStream = new FileStream(filePath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                while (true)
                {
                    if (endRecording)
                    {
                        endRecording = false;
                        break;
                    }
                    try
                    {
                        // Keep reading the buffer while there is audio input.
                        int numBytes = await audioRecord.ReadAsync(audioBuffer, 0, audioBuffer.Length);
                        await fileStream.WriteAsync(audioBuffer, 0, numBytes);
                        // Do something with the audio input.
                    }
                    catch (System.Exception ex)
                    {
                        Console.Out.WriteLine(ex.Message);
                        break;
                    }
                }
                fileStream.Close();
            }
            audioRecord.Stop();
            audioRecord.Release();

            currentState = State.Idle;
            //isRecording = false;

            //RaiseRecordingStateChangedEvent();
        }


        public AudioFile Stop()
        {
            endRecording = true;
            System.Threading.Thread.Sleep(500); // Give it time to drop out.

            if (currentState == State.Recording)
            {
                output.Duration = (DateTime.UtcNow - output.CreatedOn).TotalSeconds;
                currentState = State.Idle;
                return output;
            }

            throw new RecordingException("Invalid state, cannot stop recording");
        }

        private string GetTempFilename()
        {
            var cacheFile = Path.Combine(Xamarin.Essentials.FileSystem.CacheDirectory,
                this.output.FileName);
            if (File.Exists(cacheFile))
            {
                File.Delete(cacheFile);
            }
            return cacheFile;
        }

        private void DeleteTempFile()
        {

        }

        private void CopyWavFile(string inFilename, string outFilename)
        {
            FileStream inputStream = null;
            FileStream outputStream = null;
            long totalAudioLen = 0;
            long totalDataLen = totalAudioLen + 36;
            long longSampleRate = SAMPLING_RATE;
            int channels = 2;
            long byteRate = BIT_RATE * SAMPLING_RATE * channels / 8;

            byte[] data = new byte[bufferSize];

            try
            {
                inputStream = File.OpenRead(inFilename);
                outputStream = File.OpenWrite(outFilename);
                totalAudioLen = inputStream.Length;
                totalDataLen = totalAudioLen + 36;

                //AppLog.logString("File size: " + totalDataLen);

                WriteWavHeader(outputStream, totalAudioLen, totalDataLen,
                             longSampleRate, channels, byteRate);

                while (inputStream.Read(data) != -1) {
                    outputStream.Write(data);
                }

                inputStream.Close();
                outputStream.Close();
            }
            catch (FileNotFoundException e)
            {
            }
            catch (IOException e)
            {
            }
        }

        private void WriteWavHeader(FileStream outputStream, long totalAudioLen,
            long totalDataLen, long longSampleRate, int channels, long byteRate)
        {
            byte[] header = new byte[44];

            header[0] = (byte)'R';  // RIFF/WAVE header
            header[1] = (byte)'I';
            header[2] = (byte)'F';
            header[3] = (byte)'F';
            header[4] = (byte) (totalDataLen & 0xff);
            header[5] = (byte) ((totalDataLen >> 8) & 0xff);
            header[6] = (byte) ((totalDataLen >> 16) & 0xff);
            header[7] = (byte) ((totalDataLen >> 24) & 0xff);
            header[8] = (byte)'W';
            header[9] = (byte)'A';
            header[10] = (byte)'V';
            header[11] = (byte)'E';
            header[12] = (byte)'f';  // 'fmt ' chunk
            header[13] = (byte)'m';
            header[14] = (byte)'t';
            header[15] = (byte)' ';
            header[16] = 16;  // 4 bytes: size of 'fmt ' chunk
            header[17] = 0;
            header[18] = 0;
            header[19] = 0;
            header[20] = 1;  // format = 1
            header[21] = 0;
            header[22] = (byte) channels;
            header[23] = 0;
            header[24] = (byte) (longSampleRate & 0xff);
            header[25] = (byte) ((longSampleRate >> 8) & 0xff);
            header[26] = (byte) ((longSampleRate >> 16) & 0xff);
            header[27] = (byte) ((longSampleRate >> 24) & 0xff);
            header[28] = (byte) (byteRate & 0xff);
            header[29] = (byte) ((byteRate >> 8) & 0xff);
            header[30] = (byte) ((byteRate >> 16) & 0xff);
            header[31] = (byte) ((byteRate >> 24) & 0xff);
            header[32] = (byte) (2 * 16 / 8);  // block align
            header[33] = 0;
            header[34] = BIT_RATE;  // bits per sample. NOTE: bit rate != bit depth!
            header[35] = 0;
            header[36] = (byte)'d';
            header[37] = (byte)'a';
            header[38] = (byte)'t';
            header[39] = (byte)'a';
            header[40] = (byte) (totalAudioLen & 0xff);
            header[41] = (byte) ((totalAudioLen >> 8) & 0xff);
            header[42] = (byte) ((totalAudioLen >> 16) & 0xff);
            header[43] = (byte)((totalAudioLen >> 24) & 0xff);

            outputStream.Write(header, 0, 44);
        }
    }
}
