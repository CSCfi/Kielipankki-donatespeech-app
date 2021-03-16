using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Android.Media;
using Recorder.Droid;
using Recorder.Models;
using Recorder.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(AudioRecorder))]
namespace Recorder.Droid
{
    public class AudioRecorder : IAudioRecorder
    {
        const int BIT_DEPTH = 16;
        const int SAMPLE_RATE = 44_100;
        const int CHANNEL_COUNT = 1;

        const string FILE_EXTENSION = "flac";
        const string MIME_TYPE = "audio/flac";

        enum State { Idle, Prepared, Recording };

        private volatile State currentState = State.Idle;

        // final returned external recording model

        private AudioFile audioFile;

        // low-level recording / encoding API objects
        private AudioRecord recorder;
        private MediaCodec encoder;

        // recording is done in a Task, a worker thread, that needs to be
        // cancelled when Stop() is called
        private Task recordingTask;
        private CancellationTokenSource recordingTaskCTS;

        // recorder & encoder file output
        private FileStream encoderOutput;

        // the checksum object needs to be kept in memory during recording as
        // it is being calculated live during recording
        private MD5 checksum;

        // recording input buffer length is dynamic per-device
        private readonly int bufferSize = AudioRecord.GetMinBufferSize(SAMPLE_RATE, ChannelIn.Mono, Encoding.Pcm16bit);

        // number of bytes per sample
        private readonly int sampleSize = BIT_DEPTH / 8 * CHANNEL_COUNT;

        // total recorded samples count
        private volatile uint sampleCount;

        // older FLAC encoder discards the end of the stream, prevent writing
        // incorrect headers by setting this to false
        private bool addFinishedRecordingHeaders;

        public AudioRecorder()
        {
        }

        public bool IsRecording
        {
            get => currentState == State.Recording;
        }

        /**
         * Set up a new recording file and prepare the recording & encoding
         * subsystems
         **/
        public string Prepare()
        {
            var recordingId = Guid.NewGuid().ToString();
            var fileName = $"{recordingId}.{FILE_EXTENSION}";

            MediaCodecList mcl = new MediaCodecList(MediaCodecListKind.RegularCodecs);
            MediaFormat format = new MediaFormat();

            format.SetString(MediaFormat.KeyMime, MIME_TYPE);
            format.SetInteger(MediaFormat.KeySampleRate, SAMPLE_RATE);
            format.SetInteger(MediaFormat.KeyChannelCount, CHANNEL_COUNT);

            try
            {
                var codecname = mcl.FindEncoderForFormat(format);
                encoder = MediaCodec.CreateByCodecName(codecname);
                encoder.Configure(format, null, null, MediaCodecConfigFlags.Encode);
            }
            catch (IOException)
            {
                throw new RecordingException("Unable to initialize the encoder");
            }

            recorder = new AudioRecord(AudioSource.Mic, SAMPLE_RATE, ChannelIn.Mono, Encoding.Pcm16bit, bufferSize);
            checksum = MD5.Create();

            if (recorder.State != Android.Media.State.Initialized)
            {
                throw new RecordingException("Unable to initialize the recorder");
            }

            encoderOutput = File.Create(GetFullPathNameForRecording(fileName));
            addFinishedRecordingHeaders = true;

            audioFile = new AudioFile()
            {
                FileName = fileName,
                BitDepth = BIT_DEPTH,
                SampleRate = SAMPLE_RATE,
                NumberOfChannels = CHANNEL_COUNT,
                ContentType = MIME_TYPE,
            };

            currentState = State.Prepared;
            return recordingId;
        }

        /**
         * Start the recorder & encoder process and kick off the recording loop
         * in a new thread (@see StartRecording()).
         **/
        public void Start()
        {
            if (currentState != State.Prepared)
            {
                throw new RecordingException(string.Format("Invalid state {0}", currentState));
            }

            currentState = State.Recording;
            recordingTaskCTS = new CancellationTokenSource();
            sampleCount = 0;
            audioFile.CreatedOn = DateTime.Now;
            recordingTask = Task.Run(() => RecordingTask(recordingTaskCTS.Token), recordingTaskCTS.Token);
        }

        /**
         * Signal the recording thread to stop, wait for the process to complete and
         * finish off the recording file, returning the completed AudioFile object.
         **/
        public AudioFile Stop()
        {
            if (currentState != State.Recording)
            {
                throw new RecordingException(string.Format("Invalid state {0}", currentState));
            }

            try
            {
                recordingTaskCTS.Cancel();
                recordingTask.Wait();

                return FinalizeRecording();
            }
            catch (Exception e)
            {
                throw new RecordingException(string.Format("Recording failed: {0}", e));
            }
            finally
            {
                currentState = State.Idle;
            }
        }

        /**
         * Add the missing FLAC headers to the output file and return the final
         * recording object.
         * 
         * As the encoding process is done in parallel with the recording, the
         * encoder does not know the final sample count or MD5 checksum of the
         * input data when it writes down the headers. Fortunately, we can easily
         * just find-and-replace this information.
         **/
        private AudioFile FinalizeRecording()
        {
            if (addFinishedRecordingHeaders)
            {
                // the sample spec is always in a fixed position
                encoderOutput.Seek(18, SeekOrigin.Begin);

                using (var reader = new BinaryReader(encoderOutput, System.Text.Encoding.ASCII, true))
                {
                    using (var writer = new BinaryWriter(encoderOutput, System.Text.Encoding.ASCII, true))
                    {
                        // read the 128-bit sample spec
                        var sampleInfo = swapEndianness(reader.ReadUInt64());

                        // replace the last 36 bits with the sample size
                        sampleInfo |= sampleCount;

                        // the reader advanced the seek pointer, go back a bit
                        encoderOutput.Seek(-8, SeekOrigin.Current);

                        // write it down
                        writer.Write(swapEndianness(sampleInfo));

                        // the next field is the MD5 checksum, no need to seek here
                        writer.Write(checksum.Hash);
                    }
                }
            }

            encoderOutput.Close();

            audioFile.Duration = (double)sampleCount / SAMPLE_RATE;

            return audioFile;
        }

        /**
         * 8-byte int endianness swap
         * 
         * FLAC headers are big-endian and Microsoft hates it, so we have to implement this manually.
         * */
        private ulong swapEndianness(ulong value)
        {
            var b1 = (value >> 0) & 0xff;
            var b2 = (value >> 8) & 0xff;
            var b3 = (value >> 16) & 0xff;
            var b4 = (value >> 24) & 0xff;
            var b5 = (value >> 32) & 0xff;
            var b6 = (value >> 40) & 0xff;
            var b7 = (value >> 48) & 0xff;
            var b8 = (value >> 56) & 0xff;
            return b1 << 56 | b2 << 48 | b3 << 40 | b4 << 32 | b5 << 24 | b6 << 16 | b7 << 8 | b8 << 0;
        }

        private string GetFullPathNameForRecording(string fileName)
        {
            return Path.Combine(
                Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments),
                fileName
            );
        }

        /**
         * The actual recording loop
         * 
         * The loop works in two parallel stages:
         * 
         * 1) Read microphone input and pass the recorded data to the encoder process
         * 2) Read the encoded data and write it to the disk
         * 
         * As the user might stop the recording at any given time, we first need to stop
         * the recording part of the loop and then wait for the encoder to finish
         * processing the input it has been given.
         **/
        private void RecordingTask(CancellationToken token)
        {
            try
            {
                encoder.Start();
                recorder.StartRecording();

                bool finishedRecording = false;
                bool finishedEncoding = false;

                while (!finishedEncoding)
                {
                    if (!finishedRecording)
                    {
                        finishedRecording = token.IsCancellationRequested;
                        var inputData = new byte[bufferSize];
                        int length = recorder.Read(inputData, 0, bufferSize);

                        if (length != bufferSize)
                        {
                            return;
                        }

                        var inputBufferIdx = encoder.DequeueInputBuffer(-1);

                        if (inputBufferIdx >= 0)
                        {
                            sampleCount += (uint)(bufferSize / sampleSize);

                            var inputBuffer = encoder.GetInputBuffer(inputBufferIdx);
                            inputBuffer.Put(inputData);

                            if (!finishedRecording)
                            {
                                checksum.TransformBlock(inputData, 0, bufferSize, null, 0);
                                encoder.QueueInputBuffer(inputBufferIdx, 0, bufferSize, 0, 0);
                            }
                            else
                            {
                                checksum.TransformFinalBlock(inputData, 0, bufferSize);
                                encoder.QueueInputBuffer(inputBufferIdx, 0, bufferSize, 0, MediaCodecBufferFlags.EndOfStream);
                            }
                        }
                    }

                    var outputBufferInfo = new MediaCodec.BufferInfo();
                    var outputBufferIdx = encoder.DequeueOutputBuffer(outputBufferInfo, 10_000);

                    if (outputBufferIdx >= 0)
                    {
                        var outputBuffer = encoder.GetOutputBuffer(outputBufferIdx);

                        outputBuffer.Position(outputBufferInfo.Offset);
                        outputBuffer.Limit(outputBufferInfo.Offset + outputBufferInfo.Size);

                        var data = new byte[outputBuffer.Remaining()];
                        outputBuffer.Get(data);

                        if (encoderOutput.Position == 0)
                        {
                            VerifyFLACHeader(data[0..8]);
                        }

                        encoderOutput.Write(data);

                        encoder.ReleaseOutputBuffer(outputBufferIdx, false);
                    }

                    if (outputBufferInfo.Flags.HasFlag(MediaCodecBufferFlags.EndOfStream))
                    {
                        finishedEncoding = true;
                    }
                }
            }

            catch (Exception e)
            {
                throw e;
            }

            finally
            {
                encoder.Stop();
                recorder.Stop();
                encoder.Release();
                recorder.Release();
            }
        }

        private void VerifyFLACHeader(byte[] header)
        {
            var validHeaders = new List<byte[]>()
            {
                new byte[] { 0x66, 0x4c, 0x61, 0x43, 0x80, 0x00, 0x00, 0x22 },
                new byte[] { 0x66, 0x4c, 0x61, 0x43, 0x00, 0x00, 0x00, 0x22 }
            };

            if (validHeaders.Any(h => header.SequenceEqual(h)))
            {
                // the FLAC header looks correct, do nothing
                return;
            }

            // skip writing the sample count and checksum headers as they're
            // probably not accurate
            addFinishedRecordingHeaders = false;

            using (var writer = new BinaryWriter(encoderOutput, System.Text.Encoding.ASCII, true))
            {
                // minimal valid header with one STREAMINFO block
                writer.Write(validHeaders[0]);

                // block sizes unknown, use defaults
                // minimum size 16 (endianness swap!)
                writer.Write((ushort)(16 << 8));
                writer.Write(ushort.MaxValue);

                // frame sizes not known (24+24 bits)
                writer.Write(ushort.MinValue);
                writer.Write(uint.MinValue);

                // sample info (first 28 bits)
                writer.Write(
                    swapEndianness(
                        (ulong)SAMPLE_RATE << 44 |
                        (ulong)(CHANNEL_COUNT - 1) << 41 |
                        (ulong)(BIT_DEPTH - 1) << 36
                    )
                );

                // rest of the empty header
                writer.Write(ulong.MinValue);
                writer.Write(ulong.MinValue);
            }
        }
    }
}
