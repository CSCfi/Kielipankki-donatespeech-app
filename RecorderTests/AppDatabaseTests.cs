using System;
using Xunit;

using Moq;

using Recorder;
using System.Threading.Tasks;
using Recorder.Models;
using Recorder.Services;

namespace RecorderTests
{
    public class AppDatabaseTests
    {
        Mock<IAppDatabase> mockDb;

        public AppDatabaseTests()
        {
            this.mockDb = new Mock<IAppDatabase>();
        }

        [Fact]
        public void TestRecordingCount()
        {
            var count = 0;

            // For this async Moq technique see https://medium.com/@tiagocesar/mocking-async-methods-with-moq-d222bf4b565b
            mockDb.Setup(db => db.GetRecordingCountAsync()).Returns(Task.FromResult(count));
            Assert.Equal(0, count);
        }

        [Fact]
        public void TestInsertRecord()
        {
            var item = new Recording()
            {
                RecordingId = Guid.NewGuid().ToString(),
                ClientId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                UploadStatus = "unknown",
                Metadata = "{}"  // empty JSON object
            };

            var result = 0;
            mockDb.Setup(db => db.SaveRecordingAsync(item)).Returns(Task.FromResult(result));
            Assert.NotEqual(-1, result);
        }
    }
}
