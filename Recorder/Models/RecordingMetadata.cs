using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Essentials;

namespace Recorder.Models
{
    public class RecordingMetadata
    {
        public string ClientId;
        public string RecordingId;
        public string ClientPlatformName;
        public string ClientPlatformVersion;
        public DateTime RecordingTimestamp;
        public double RecordingDuration;
        public int RecordingBitDepth;
        public int RecordingSampleRate;
        public int RecordingNumberOfChannels;
        public string ContentType;
        public string ItemId;

        public UserMetadata User;

        public RecordingMetadata(string recordingId)
        {
            this.RecordingId = recordingId;

            if (Preferences.ContainsKey(Constants.ClientIdKey))
            {
                this.ClientId = Preferences.Get(Constants.ClientIdKey, "unknown");
            }

            this.ClientPlatformName = DeviceInfo.Platform.ToString();
            this.ClientPlatformVersion = DeviceInfo.VersionString;

            this.User = new UserMetadata();
        }

        public JObject ToJsonObject()
        {
            return new JObject(
                new JObject(
                    new JProperty(Constants.ClientIdKey, this.ClientId),
                    new JProperty(Constants.RecordingIdKey, this.RecordingId),
                    new JProperty(Constants.ClientPlatformNameKey, this.ClientPlatformName),
                    new JProperty(Constants.ClientPlatformVersionKey, DeviceInfo.VersionString),
                    new JProperty(Constants.TimestampKey, this.RecordingTimestamp),
                    new JProperty(Constants.RecordingDurationKey, this.RecordingDuration),
                    new JProperty(Constants.RecordingBitDepthKey, this.RecordingBitDepth),
                    new JProperty(Constants.RecordingSampleRateKey, this.RecordingSampleRate),
                    new JProperty(Constants.RecordingNumberOfChannelsKey, this.RecordingNumberOfChannels),
                    new JProperty(Constants.ContentTypeKey, this.ContentType),
                    new JProperty(Constants.ItemIdKey, this.ItemId),
                    new JProperty(Constants.UserKey, this.User != null ? this.User.ToJsonObject() : null)
                )
            );
        }

        public string ToJsonString()
        {
            JObject jsonObject = this.ToJsonObject();
            return JsonConvert.SerializeObject(jsonObject);
        }
    }
}
