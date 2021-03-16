using System;
namespace Recorder.Services
{
    public interface IAppConfiguration
    {
        string RecorderApiUrl { get; }
        string RecorderApiKey { get; }
        bool AlwaysShowOnboarding { get; }
        uint MaxRecordingMinutes { get; }
        string BuildType { get; }
    }
}
