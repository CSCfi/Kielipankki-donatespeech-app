using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace Recorder.Services
{
    public class AppConfiguration : IAppConfiguration
    {
        // namespace and filename for embedded resource
        private const string resourceName = "Recorder.BuildConfig.appconfiguration.json";

        private AppConfiguration()
        {
            MaxRecordingMinutes = 10;
        }

        public string RecorderApiUrl { get; set; }
        public string RecorderApiKey { get; set; }
        public bool AlwaysShowOnboarding { get; set; }
        public uint MaxRecordingMinutes { get; set; }
        public string BuildType { get; set; }

        public static AppConfiguration Load()
        {
            var assembly = Assembly.GetAssembly(typeof(AppConfiguration));
            var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            return JsonConvert.DeserializeObject<AppConfiguration>(json);
        }
    }
}
