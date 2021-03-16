using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Recorder.Models;

namespace Recorder.Services
{
    public interface IAppDatabase
    {
        Task<int> GetRecordingCountAsync();
        Task<List<Recording>> GetRecordingsAsync();
        Task<List<Recording>> GetRecordingsByUploadStatusAsync(string uploadStatus);
        Task<Recording> GetRecordingAsync(string recordingId);
        Task<int> SaveRecordingAsync(Recording item);
        Task<int> DeleteRecordingAsync(Recording item);
        Task<int> UpdateRecordingUploadStatusAsync(Recording item);
        void DeleteAllRecordings();
    }
}
