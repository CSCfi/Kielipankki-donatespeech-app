using System;
using System.Collections.Generic;

using Xunit;
using Moq;

using Recorder.Models;
using Recorder.ViewModels;
using Recorder.Services;
using Recorder;
using System.Diagnostics.CodeAnalysis;
using System.Collections;

namespace RecorderTests.ViewModels
{
    public class SchedulePageViewModelTests
    {
        Mock<IAppRepository> mockRepo;
        Mock<IRecordingManager> mockRecordingManager;
        Mock<IAppConfiguration> mockConfiguration;

        public SchedulePageViewModelTests()
        {
            mockRepo = new Mock<IAppRepository>();
            mockRecordingManager = new Mock<IRecordingManager>();
            mockConfiguration = new Mock<IAppConfiguration>();
        }

        [Fact]
        public void AnswerSavedOnNext()
        {
            string modifiedItemId = "1234";
            string answerText = "test answer";

            Schedule schedule = new ScheduleBuilder()
                .WithItems(
                    TextPromptItem.WithId(modifiedItemId).Build(),
                    TextPromptItem.Build())
                .Build();

            SchedulePageViewModel model = new SchedulePageViewModel(schedule,
                mockRecordingManager.Object, new DummyEventTracker(), new DummyDictionary(), mockRepo.Object,
                mockConfiguration.Object);

            // simulate text answer entered
            model.ItemViewModel.Answer = answerText;
            model.ItemViewModel.AnswerModified = true;

            model.NextCommand.Execute(null);

            mockRecordingManager.Verify(f => f.FinalizeAnswer(modifiedItemId, answerText));
        }

        [Fact]
        public void NextFromLastItemFinishes()
        {
            Schedule schedule = BaseSchedule.Build();
            SchedulePageViewModel model = CreateViewModel(schedule);

            model.NextCommand.Execute(null);

            Assert.Raises<EventArgs>(
                attach: handler => model.ScheduleFinished += handler,
                detach: handler => model.ScheduleFinished -= handler,
                testCode: () => model.NextCommand.Execute(null)
                );
        }

        [Fact]
        public void PrevFromFirstItemNotAllowed()
        {
            Schedule schedule = BaseSchedule.Build();
            SchedulePageViewModel model = CreateViewModel(schedule);
            Assert.False(model.PreviousCommand.CanExecute(null));
        }

        [Fact]
        public void PrevNextFromMiddleItemAllowed()
        {
            Schedule schedule = new ScheduleBuilder()
                .WithItems(ImageItem.Build(), VideoItem.Build(), ImageItem.Build())
                .Build();
            SchedulePageViewModel model = CreateViewModel(schedule);

            // move to middle
            model.NextCommand.Execute(null);

            Assert.True(model.PreviousCommand.CanExecute(null));
            Assert.True(model.NextCommand.CanExecute(null));
        }

        [Fact]
        public void FirstScheduleItemSelected()
        {
            Schedule schedule = BaseSchedule.Build();            
            SchedulePageViewModel model = CreateViewModel(schedule);
            Assert.NotNull(model.ItemViewModel);
        }

        [Fact]
        public void RetryRecording()
        {
            Schedule schedule = BaseSchedule.Build();
            SchedulePageViewModel model = CreateViewModel(schedule);

            model.RecordCommand.Execute(null); // start
            Assert.Equal(ScheduleItemStateType.Recording, model.DisplayState);

            model.RecordCommand.Execute(null); // stop
            Assert.Equal(ScheduleItemStateType.Finish, model.DisplayState);

            model.RetryCommand.Execute(null);
            Assert.Equal(ScheduleItemStateType.Start, model.DisplayState);
            Assert.True(model.ShowRecordButton, "Record button should show");            
        }

        [Fact]
        public void CounterIndexAndTotal()
        {
            var nonRecordingVideo = VideoItem.WithRecordingEnabled(false).Build();
            var recordingVideo = VideoItem.Build();
            var recordingVideo2 = VideoItem.Build();
            var nonRecordingImage = ImageItem.WithRecordingEnabled(false).Build();
            var recordingImage = ImageItem.Build();

            Schedule schedule = new ScheduleBuilder()
                .WithItems(
                    recordingVideo,
                    nonRecordingImage,
                    recordingImage,
                    recordingVideo2,
                    nonRecordingVideo
                    )
                .Build();

            SchedulePageViewModel model = CreateViewModel(schedule);

            Assert.Equal(1, model.ItemViewModel.CounterIndex);
            Assert.Equal(3, model.ItemViewModel.CounterTotal);

            // move to third
            model.NextCommand.Execute(null);
            model.NextCommand.Execute(null);

            Assert.Equal(2, model.ItemViewModel.CounterIndex);
            Assert.Equal(3, model.ItemViewModel.CounterTotal);
        }

        private SchedulePageViewModel CreateViewModel(Schedule schedule)
            => new SchedulePageViewModel(schedule, new DummyRecordingManager(), new DummyEventTracker(),
                                        new DummyDictionary(), mockRepo.Object, mockConfiguration.Object);

        private ScheduleItemBuilder TextPromptItem => new ScheduleItemBuilder()
            .WithKind(ItemKindValue.Prompt)
            .WithType(ItemTypeValue.Text);

        private ScheduleItemBuilder VideoItem => new ScheduleItemBuilder()
            .WithType(ItemTypeValue.Video);

        private ScheduleItemBuilder ImageItem => new ScheduleItemBuilder()
            .WithType(ItemTypeValue.Image);

        private ScheduleBuilder BaseSchedule => new ScheduleBuilder()
            .WithItems(VideoItem.Build(), ImageItem.Build());
    }

    public class DummyEventTracker : IFirebaseAnalyticsEventTracker
    {
        public void SendEvent(string eventId) {}
        public void SendEvent(string eventId, string paramName, string value) {}
        public void SendEvent(string eventId, IDictionary<string, string> parameters) {}
        public void SendEvent(AnalyticsEvent analyticsEvent) {}
    }

    // this just returns null for all lookups
    public class DummyDictionary : IDictionary<string, object>
    {
        public object this[string key] { get => null; set => throw new NotImplementedException(); }
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            value = null;
            return false;
        }

        public ICollection<string> Keys => throw new NotImplementedException();
        public ICollection<object> Values => throw new NotImplementedException();
        public int Count => throw new NotImplementedException();
        public bool IsReadOnly => throw new NotImplementedException();
        public void Add(string key, object value)
        {
            throw new NotImplementedException();
        }

        public void Add(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool Remove(string key)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public class DummyRecordingManager : IRecordingManager
    {
        public bool IsRecording { get; private set; } = false;

        public void FinalizeAnswer(string itemId, string answer)
        {
            throw new NotImplementedException();
        }

        public void FinalizeRecording(string answer)
        {
            IsRecording = false;
        }

        public void StartRecording(string itemId)
        {
            IsRecording = true;
        }
    }
}
