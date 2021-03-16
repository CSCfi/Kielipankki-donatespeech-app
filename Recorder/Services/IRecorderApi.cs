using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Recorder.Models;

namespace Recorder.Services
{
    public struct UploadDescription
    {
        public string PreSignedUrl;
    }

    public interface IRecorderApi
    {
        Task<UploadDescription> InitUploadAsync(Recording recording, RecordingMetadata metadata);
        Task<List<Theme>> GetAllThemesAsync();
        Task<Schedule> GetScheduleAsync(string scheduleId);
        Task<List<Schedule>> GetAllSchedulesAsync();
        Task<bool> UploadRecordingAsync(string filePath, string url, string contentType);
        Task<bool> DeleteRecordingAsync(string recId);
    }
}
