using System;
using Moq;
using Recorder.Services;
using Recorder.ViewModels;
using Xunit;
using Recorder.Models;
using System.Threading.Tasks;
using Recorder;

namespace RecorderTests.ViewModels
{
    public class ScheduleStartPageViewModelTests
    {
        Mock<IAppRepository> mockRepo;
        Mock<IFirebaseAnalyticsEventTracker> mockTracker;
        Mock<IAppConfiguration> mockConfiguration;

        public ScheduleStartPageViewModelTests()
        {
            mockRepo = new Mock<IAppRepository>();
            mockTracker = new Mock<IFirebaseAnalyticsEventTracker>();
            mockConfiguration = new Mock<IAppConfiguration>();
        }

        [Fact]
        public void FailedScheduleLoad()
        {
            string scheduleId = "test_id";
            mockRepo.Setup(r => r.GetScheduleAsync(scheduleId))
                .Returns(Task.FromResult(Result<Schedule>.Failure()));

            ScheduleStartPageViewModel model = new ScheduleStartPageViewModel(scheduleId,
                mockRepo.Object, mockTracker.Object, mockConfiguration.Object);

            Assert.Raises<EventArgs>(
                attach: handler => model.ScheduleLoadFailed += handler,
                detach: handler => model.ScheduleLoadFailed -= handler,
                testCode: () => model.LoadScheduleAsync());
        }
    }
}
