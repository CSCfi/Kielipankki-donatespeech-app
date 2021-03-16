using System;
using System.Collections.Generic;

namespace Recorder
{
    public static class AnalyticsEventNamesConstants
    {
        // Firebase standard events
        public static string SelectContent { get; } = "select_content";

        // App custom events
        public static string RecordingCompleted { get; } = "recording_completed";
        public static string ScheduleCompleted { get; } = "schedule_completed";
        public static string ScheduleItemCompleted { get; } = "schedule_item_completed";
    }

    public static class AnalyticsParameterNamesConstants
    {
        // Firebase standard parameters
        public static string ContentType { get; } = "content_type";
        public static string ItemId { get; } = "item_id";
        public static string ItemName { get; } = "item_name";

        // App custom parameters
        public static string RecordingId { get; } = "recording_id";
        public static string ClientId { get; } = "client_id";
        public static string RecordingTimestamp { get; } = "recording_timestamp";
        public static string ClientPlatformName { get; } = "client_platform_name";
        public static string ClientPlatformVersion { get; } = "client_platform_version";
        public static string RecordingDuration { get; } = "recording_duration";
        public static string RecordingSpecification { get; } = "recording_specification";
        public static string Answers { get; } = "answers";
        public static string BuildType { get; } = "build_type";
    }

    public interface AnalyticsEvent
    {
        string EventId { get; }
        IDictionary<string, string> EventParameters { get; }
    }

    public class ScheduleItemCompletedEvent : AnalyticsEvent
    {
        public string EventId { get; private set; }
        public IDictionary<string, string> EventParameters { get; private set; }

        public ScheduleItemCompletedEvent(string itemId, string itemName, string answer, string buildType)
        {
            EventId = AnalyticsEventNamesConstants.ScheduleItemCompleted;
            EventParameters = new Dictionary<string, string>
            {
                { AnalyticsParameterNamesConstants.ItemId, itemId },
                { AnalyticsParameterNamesConstants.ItemName, itemName },
                { AnalyticsParameterNamesConstants.Answers, answer },
                { AnalyticsParameterNamesConstants.ContentType, AnalyticsContentTypeConstants.ScheduleItem },
                { AnalyticsParameterNamesConstants.BuildType, buildType }
            };
        }
    }

    public static class AnalyticsContentTypeConstants
    {
        public static string Schedule = "schedule";
        public static string Theme = "theme";
        public static string ScheduleItem = "scheduleitem";
    }

    public interface IFirebaseAnalyticsEventTracker
    {
        void SendEvent(string eventId);
        void SendEvent(string eventId, string paramName, string value);
        void SendEvent(string eventId, IDictionary<string, string> parameters);
        void SendEvent(AnalyticsEvent analyticsEvent);
    }
}
