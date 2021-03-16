using System;
using System.IO;
using System.Net;

namespace Recorder
{
    public static class Constants
    {
        public const string ClientIdKey = "clientId";
        public const string RecordingIdKey = "recordingId";
        public const string TimestampKey = "recordingTimestamp"; // ISO 8601 in UTC
        public const string DurationKey = "recordingDuration";  // in seconds
        public const string ClientPlatformNameKey = "clientPlatformName";
        public const string ClientPlatformVersionKey = "clientPlatformVersion";
        public const string ItemIdKey = "itemId";
        public const string RecordingDurationKey = "recordingDuration";
        public const string RecordingBitDepthKey = "recordingBitDepth";
        public const string RecordingSampleRateKey = "recordingSampleRate";
        public const string RecordingNumberOfChannelsKey = "recordingNumberOfChannels";
        public const string ContentTypeKey = "contentType";

        public const string UserKey = "user";
        public const string UserLanguageKey = "userLanguage";
        public const string OnboardingCompletedKey = "onboardingCompleted";
        public const string TotalRecordedSecondsKey = "totalRecordedSeconds";
        public const string CompletedSchedulesKey = "completedSchedules";

        public const int PendingUploadsTimerIntervalSeconds = 20;

        public const string MetadataWithoutRecording = "MetadataWithoutRecording.wav"; // special "filename" for metadata items

        public const int ScheduleVersion = 2;  // used to identify schedule file formats in HTTP request

        //
        // Constants for local SQLite database (used with the sqlite-net-pcl NuGet package)
        //

        /// <value>The filename of the SQLite database file.</value>
        public const string DatabaseFilename = "Recorder.sqlitedb";

        /// <value>The platform-specific full pathname of the database file.</value>
        public static string DatabasePath
        {
            get
            {
                var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(basePath, DatabaseFilename);
            }
        }

        /// <value>Creation flags for the local SQLite database.</value>
        public const SQLite.SQLiteOpenFlags DatabaseFlags =            
            SQLite.SQLiteOpenFlags.ReadWrite |  // open the database in read/write mode
            SQLite.SQLiteOpenFlags.Create |     // create the database if it doesn't exist            
            SQLite.SQLiteOpenFlags.SharedCache; // enable multi-threaded database access
    }
}
