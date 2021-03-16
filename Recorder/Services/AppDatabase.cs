using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SQLite;

using Recorder.Models;

namespace Recorder.Services
{
    public class AppDatabase : IAppDatabase
    {
        static readonly Lazy<SQLiteAsyncConnection> lazyInitializer = new Lazy<SQLiteAsyncConnection>(() =>
        {
            return new SQLiteAsyncConnection(Constants.DatabasePath, Constants.DatabaseFlags);
        });

        public static SQLiteAsyncConnection Database => lazyInitializer.Value;
        static bool initialized = false;

        public AppDatabase()
        {
            // Uses task extension as instructed by Microsoft in their SQLite tutorial:
            // https://docs.microsoft.com/en-us/xamarin/xamarin-forms/data-cloud/data/databases
            InitializeAsync().SafeFireAndForget(false);
        }

        async Task InitializeAsync()
        {
            if (!initialized)
            {
                if (!Database.TableMappings.Any(m => m.MappedType.Name == typeof(Recording).Name))
                {
                    //Debug.WriteLine("Creating database tables");
                    await Database.CreateTablesAsync(CreateFlags.None, typeof(Recording)).ConfigureAwait(false);
                    initialized = true;
                }
                else
                {
                    Debug.WriteLine("Database is initialized");
                }
            }
            else
            {
                Debug.WriteLine(String.Format("Connected to database at '{0}'", Database.DatabasePath));
            }
        }

        public Task<int> GetRecordingCountAsync()
        {
            return Database.Table<Recording>().CountAsync();
        }

        public Task<List<Recording>> GetRecordingsAsync()
        {
            var query = Database.Table<Recording>();
            return query.ToListAsync();
        }

        public Task<List<Recording>> GetRecordingsByUploadStatusAsync(string uploadStatus)
        {
            var query = Database.Table<Recording>().Where(r => r.UploadStatus.Equals(uploadStatus));
            return query.ToListAsync();
        }

        public Task<Recording> GetRecordingAsync(string recordingId)
        {
            return Database.Table<Recording>().Where(i => i.RecordingId == recordingId).FirstOrDefaultAsync();
        }

        public Task<int> SaveRecordingAsync(Recording item)
        {
            Debug.WriteLine(String.Format("Saving item with RecordingID = {0}", item.RecordingId));
            return Database.InsertAsync(item);
        }

        public Task<int> DeleteRecordingAsync(Recording item)
        {
            return Database.DeleteAsync(item);
        }

        public Task<int> UpdateRecordingUploadStatusAsync(Recording item)
        {
            return Database.UpdateAsync(item);
        }

        public void DeleteAllRecordings()
        {
            Debug.WriteLine("About to delete all recordings from the database");
            Database.ExecuteScalarAsync<int>("DELETE FROM Recording");
        }
    }

    public class UploadStatus
    {
        public static readonly string Unknown = "unknown";
        public static readonly string Pending = "pending";
        public static readonly string Uploaded = "uploaded";
        public static readonly string Deleted = "deleted";

        private UploadStatus() { }
    }
}
