using System;
using Xunit;
using Moq;

using Recorder;
using System.Threading.Tasks;
using System.Collections.Generic;

using Recorder.Services;
using Recorder.Models;

namespace RecorderTests
{
    public class AppRepositoryTests
    {
        Mock<IRecorderApi> MockRecorderApi;
        Mock<IAppPreferences> MockPreferences;

        public AppRepositoryTests()
        {
            MockRecorderApi = new Mock<IRecorderApi>();
            MockPreferences = new Mock<IAppPreferences>();
        }

        [Theory]
        [InlineData("1234 5678 9012", 3)]
        [InlineData(" 1234   5678 9012  ", 3)]
        [InlineData("1234", 1)]
        [InlineData("", 0)]
        public void GetCompletedSchedules(string prefValue, int expectedCount)
        {
            MockPreferences.Setup(p => p.Get(Constants.CompletedSchedulesKey, string.Empty))
                .Returns(prefValue);

            AppRepository repo = new AppRepository(MockRecorderApi.Object, MockPreferences.Object);

            Assert.Equal(expectedCount, repo.GetCompletedScheduleIds().Count);
        }

        [Fact]
        public async void TestGetAllThemesSuccess()
        {
            List<Theme> expected = new List<Theme>();

            MockRecorderApi.Setup(api => api.GetAllThemesAsync()).Returns(Task.FromResult(expected));

            AppRepository repo = new AppRepository(MockRecorderApi.Object, MockPreferences.Object);
            Result<List<Theme>> actual = await repo.GetAllThemesAsync();

            Assert.Equal(expected.Count, actual.Data.Count);
        }
    }
}
