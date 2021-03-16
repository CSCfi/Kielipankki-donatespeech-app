using System;
using SQLite;

namespace Recorder.Models
{
    public class Recording
    {
        [PrimaryKey]  // annotation for sqlite-net-pcl
        public string RecordingId { get; set; }
        public string ItemId { get; set; }  // identifier of the schedule item linked to this recording
        public string FileName { get; set; }  // filename of recording (last path component)
        public string ClientId { get; set; }
        public DateTime Timestamp { get; set; }
        public string UploadStatus { get; set; }  // see UploadStatus class for constants
        public string Metadata { get; set; }  // JSON payload of recording metadata

        public string LocalTimestamp
        {
            get
            {
                return this.Timestamp.ToLocalTime().ToString("g", System.Globalization.CultureInfo.CurrentCulture);
            }
        }

        public override string ToString()
        {
            return RecordingId;
        }
    }
}
