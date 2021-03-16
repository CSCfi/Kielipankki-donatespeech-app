using System;
using System.Linq;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Recorder.UITests
{
    [TestFixture(Platform.Android)]
    [TestFixture(Platform.iOS)]
    public class Tests
    {
        IApp app;
        Platform platform;

        static readonly Func<AppQuery, AppQuery> SummerTheme = c => c.Marked("b5a0e3a2-4065-48d6-8761-ba60a5740b9d");
         
        public Tests(Platform platform)
        {
            this.platform = platform;
        }

        [SetUp]
        public void BeforeEachTest()
        {
            app = AppInitializer.StartApp(platform);
        }

        [Test]
        public void StartSummerTheme()
        {
            WaitAndTap("OnboardingContinueButton");
            WaitAndTap(SummerTheme);
            WaitAndTap("ScheduleStartButton");

            var e = app.WaitForElement(c => c.Marked("SchedulePageNextButton"));
            Assert.IsTrue(e.Any());
        }

        private void WaitAndTap(Func<AppQuery, AppQuery> query)
        {
            var e = app.WaitForElement(query);
            Assert.IsTrue(e.Any());

            app.Tap(query);
        }

        private void WaitAndTap(string id) => WaitAndTap(c => c.Marked(id));
    }
}
