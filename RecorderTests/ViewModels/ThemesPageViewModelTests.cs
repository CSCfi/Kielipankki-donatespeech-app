using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Moq;
using Recorder.Services;
using Recorder.ViewModels;
using Xunit;
using Recorder.Models;

namespace RecorderTests.ViewModels
{
    public class ThemesPageViewModelTests
    {
        Mock<IAppRepository> mockRepo;

        public ThemesPageViewModelTests()
        {
            mockRepo = new Mock<IAppRepository>();
        }

        [Fact]
        public void FailedThemeLoad()
        {
            mockRepo.Setup(r => r.GetAllThemesAsync())
                .Returns(Task.FromResult(Result<List<Theme>>.Failure()));

            ThemesPageViewModel model = new ThemesPageViewModel(mockRepo.Object);
            
            Assert.Raises<EventArgs>(
                attach: handler => model.ThemeLoadFailed += handler,
                detach: handler => model.ThemeLoadFailed -= handler,
                testCode: () => model.ReloadIfNeeded());
        }

        [Fact]
        public void CompletedSchedulesUpdated()
        {
            string idComplete = "schedule1";
            string idNotComplete = "schedule2";

            List<Theme> expected = new List<Theme>()
            {
                new ThemeBuilder().WithScheduleIds(idComplete).Build(),
                new ThemeBuilder().WithScheduleIds(idNotComplete).Build()
            };

            mockRepo.Setup(r => r.GetAllThemesAsync())
                .Returns(Task.FromResult(Result<List<Theme>>.Success(expected)));

            mockRepo.Setup(r => r.GetCompletedScheduleIds())
                .Returns(new List<string>() { idComplete });

            ThemesPageViewModel model = new ThemesPageViewModel(mockRepo.Object);

            model.ReloadIfNeeded();

            Assert.True(model.ThemeModels.Find(t => t.FirstScheduleId == idComplete)?.IsCompleted);
            Assert.False(model.ThemeModels.Find(t => t.FirstScheduleId == idNotComplete)?.IsCompleted);
        }
    }
}
