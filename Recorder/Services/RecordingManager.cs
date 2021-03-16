using System;
using System.Diagnostics;

using Xamarin.Forms;

using Recorder.Models;
using Xamarin.Essentials;
using System.Collections.Generic;

namespace Recorder.Services
{
    public class RecordingManager : IRecordingManager
    {
        private Recording recording;
        private readonly IAudioRecorder recorder;
        private readonly IAppConfiguration appConfiguration;

        public RecordingManager(IAppConfiguration appConfiguration)
        {
            this.appConfiguration = appConfiguration;
            recorder = DependencyService.Get<IAudioRecorder>();

            if (recorder == null)
            {
                Debug.WriteLine("Problem getting IAudioRecorder");
            }
        }

        public bool IsRecording => recorder.IsRecording;

        public void StartRecording(string itemId)
        {
            if (IsRecording)
            {
                throw new Exception("Cannot start recording when already recording");
            }

            recording = new Recording()
            {
                RecordingId = recorder.Prepare(),
                ItemId = itemId,
                ClientId = Preferences.Get(Constants.ClientIdKey, "unknown"),
                Timestamp = DateTime.UtcNow,
            };

            Debug.WriteLine($"About to start recording, rec ID = '{itemId}'");
            recorder.Start();
        }

        public void FinalizeRecording(string answer)
        {
            if (!IsRecording)
            {
                Debug.WriteLine("Skipping recording finalize since not recording");
                return;
            }

            Debug.WriteLine($"RecordingManager.FinalizeRecording, item ID = '{recording.ItemId}'");

            var file = recorder.Stop();
            
            RecordingMetadata metadata = new RecordingMetadata(recording.RecordingId)
            {
                ClientId = recording.ClientId,
                ItemId = recording.ItemId,
                RecordingTimestamp = file.CreatedOn,
                RecordingDuration = file.Duration,
                RecordingBitDepth = file.BitDepth,
                RecordingSampleRate = file.SampleRate,
                RecordingNumberOfChannels = file.NumberOfChannels,
                ContentType = file.ContentType,
            };

            Debug.WriteLine($"ContentType = '{file.ContentType}'");

            // Explicitly set the User object to null for a recording
            metadata.User = null;

            string metadataString = metadata.ToJsonString();
            Debug.WriteLine($"After finalizing recording, metadata = '{metadataString}'");
            recording.FileName = file.FileName;
            recording.Metadata = metadataString;

            recording.UploadStatus = UploadStatus.Pending;
            App.Database.SaveRecordingAsync(recording);
            Debug.WriteLine($"Saved recording information for '{recording.RecordingId}' in the database");

            var dict = new Dictionary<string, string>
            {
                //{ AnalyticsParameterNamesConstants.RecordingId, metadata.RecordingId },
                { AnalyticsParameterNamesConstants.ClientId, metadata.ClientId },
                { AnalyticsParameterNamesConstants.ItemId, metadata.ItemId },
                { AnalyticsParameterNamesConstants.RecordingTimestamp, metadata.RecordingTimestamp.ToString("o") },
                { AnalyticsParameterNamesConstants.ClientPlatformName, metadata.ClientPlatformName },
                { AnalyticsParameterNamesConstants.ClientPlatformVersion, metadata.ClientPlatformVersion },
                { AnalyticsParameterNamesConstants.RecordingDuration, metadata.RecordingDuration.ToString() },
                { AnalyticsParameterNamesConstants.RecordingSpecification,
                    $"{metadata.RecordingSampleRate.ToString()}/{metadata.RecordingBitDepth.ToString()}/{metadata.RecordingNumberOfChannels.ToString()}" },
                { AnalyticsParameterNamesConstants.BuildType, appConfiguration.BuildType },
            };
            var app = Application.Current as App;
            app.AnalyticsEventTracker.SendEvent(AnalyticsEventNamesConstants.RecordingCompleted, dict);

            // Update the total seconds recorded
            int durationInSeconds = (int) Math.Floor(metadata.RecordingDuration);
            app.UpdateTotalRecorded(durationInSeconds);
        }

        public void FinalizeAnswer(string itemId, string answer)
        {
            string recId = Guid.NewGuid().ToString();
            // Create a dummy recording so that we can save it to the database
            Recording rec = new Recording
            {
                RecordingId = recId,
                ItemId = itemId,
                FileName = $"{recId}-{Constants.MetadataWithoutRecording}",
                ClientId = Preferences.Get(Constants.ClientIdKey, "unknown"),
                Timestamp = DateTime.UtcNow,
                UploadStatus = UploadStatus.Pending
            };

            RecordingMetadata metadata = new RecordingMetadata(rec.RecordingId);

            metadata.ClientId = rec.ClientId;
            metadata.ItemId = rec.ItemId;
            metadata.RecordingTimestamp = rec.Timestamp;

            // Add user metadata based on the item view model
            var umd = new UserMetadata();
            var userAnswer = new UserAnswer();
            userAnswer.ItemId = itemId;
            userAnswer.Value = answer;
            umd.AddAnswer(userAnswer);

            metadata.User = umd;

            rec.Metadata = metadata.ToJsonString();

            App.Database.SaveRecordingAsync(rec);
            Debug.WriteLine($"Saved answer '{answer}', recording '{rec.RecordingId}', item '{itemId}'");
        }
    }
}
