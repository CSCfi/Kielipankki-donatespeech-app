using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Recorder.Models;

namespace Recorder.Services
{
    public interface IAppRepository
    {
        Task<Result<List<Theme>>> GetAllThemesAsync();
        Task<Result<Schedule>> GetScheduleAsync(string scheduleId);
        Task UploadPendingRecordings();
        Task ListRecordingsInDatabase();
        Task ListUploadedRecordings();

        List<string> GetCompletedScheduleIds();
        bool AddCompletedScheduleId(string id);

        void SaveAnswer(string id, string value);
        string GetAnswer(string id);
    }
}
