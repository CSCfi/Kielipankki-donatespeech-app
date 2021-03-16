
namespace Recorder.Services
{
    public interface IRecordingManager
    {
        bool IsRecording { get; }

        // start a new recording, but will throw exception if currently recording
        void StartRecording(string itemId);

        // stop and send current recording, no action if not currently recording
        // optional user answer provided during recording can be given as parameter
        void FinalizeRecording(string answer);

        // store a user answer only, no audio recording
        void FinalizeAnswer(string itemId, string answer);
    }
}
