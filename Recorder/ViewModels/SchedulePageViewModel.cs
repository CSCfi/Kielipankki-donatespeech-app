using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using Recorder.Models;
using Recorder.ResX;
using Recorder.Services;
using Xamarin.Forms;

namespace Recorder.ViewModels
{
    public class SchedulePageViewModel : BaseViewmodel
    {
        private Schedule schedule;
        private List<ScheduleItemViewModel> itemModels;
        private int currentItemIndex;
        private ElapsedTimeModel elapsedTimeModel;

        private readonly IRecordingManager recordingManager;
        private readonly IFirebaseAnalyticsEventTracker eventTracker;
        private readonly IDictionary<string, object> resourceDictionary;
        private readonly IAppRepository appRepository;
        private readonly IAppConfiguration appConfiguration;

        public event EventHandler<EventArgs> ScheduleFinished;
        public event EventHandler<EventArgs> MaxRecordingTimeReached;

        public ICommand PreviousCommand { get; private set; }
        public ICommand NextCommand { get; private set; }
        public ICommand RecordCommand { get; private set; } // both start and stop recording
        public ICommand ContinueCommand { get; private set; }
        public ICommand RetryCommand { get; private set; }

        private ScheduleItemViewModel _itemViewModel;
        public ScheduleItemViewModel ItemViewModel
        {
            get => _itemViewModel;
            set
            {
                if (!ReferenceEquals(_itemViewModel, value))
                {
                    _itemViewModel = value;
                    RaisePropertyChanged(nameof(ItemViewModel));

                    UpdateContinueButton();
                    UpdateRecordButton();

                    (NextCommand as Command)?.ChangeCanExecute();
                    (PreviousCommand as Command)?.ChangeCanExecute();

                    SendSelectItemEvent(schedule.Items[currentItemIndex]);
                }
            }
        }

        private volatile ScheduleItemStateType _displayState;
        public ScheduleItemStateType DisplayState
        {
            get => _displayState;
            set
            {
                if (!ReferenceEquals(_displayState, value))
                {
                    _displayState = value;
                    RaisePropertyChanged(nameof(DisplayState));

                    // forward state change to currently shown item
                    ItemViewModel.ItemDisplayState = _displayState;

                    RaisePropertyChanged(nameof(ShowRetryButton));
                    UpdateRecordButton();
                    UpdateRecordingTimeText();

                    (NextCommand as Command)?.ChangeCanExecute();
                    (PreviousCommand as Command)?.ChangeCanExecute();
                }
            }
        }

        private string _continueButtonText;
        public string ContinueButtonText
        {
            get => _continueButtonText;
            set => Set(ref _continueButtonText, value, nameof(ContinueButtonText));
        }

        private Style _continueButtonStyle;
        public Style ContinueButtonStyle
        {
            get => _continueButtonStyle;
            set => Set(ref _continueButtonStyle, value, nameof(ContinueButtonStyle));
        }

        private string _recordButtonText;
        public string RecordButtonText
        {
            get => _recordButtonText;
            set => Set(ref _recordButtonText, value, nameof(RecordButtonText));
        }

        private Style _recordButtonStyle;
        public Style RecordButtonStyle
        {
            get => _recordButtonStyle;
            set => Set(ref _recordButtonStyle, value, nameof(RecordButtonStyle));
        }

        private string _recordingTimeText;
        public string RecordingTimeText
        {
            get => _recordingTimeText;
            set => Set(ref _recordingTimeText, value, nameof(RecordingTimeText));
        }

        private Style _recordingTimeLabelStyle;
        public Style RecordingTimeLabelStyle
        {
            get => _recordingTimeLabelStyle;
            set => Set(ref _recordingTimeLabelStyle, value, nameof(RecordingTimeLabelStyle));
        }

        private bool _showRecordButton;
        public bool ShowRecordButton
        {
            get => _showRecordButton;
            set => Set(ref _showRecordButton, value, nameof(ShowRecordButton), nameof(ShowContinueButton));
        }

        public bool ShowContinueButton => !ShowRecordButton;

        public bool ShowRetryButton => DisplayState == ScheduleItemStateType.Finish;

        public SchedulePageViewModel(Schedule schedule, IRecordingManager recordingManager,
            IFirebaseAnalyticsEventTracker eventTracker, IDictionary<string, object> resourceDictionary,
            IAppRepository appRepository, IAppConfiguration appConfiguration)
        {
            this.schedule = schedule;
            this.recordingManager = recordingManager;
            this.eventTracker = eventTracker;
            this.resourceDictionary = resourceDictionary;
            this.appRepository = appRepository;
            this.appConfiguration = appConfiguration;

            CreateCommands();
            CreateElapsedTimeModel();

            var totalRecordingItems = schedule.Items
                .FindAll(it => it.IsRecording && !it.IsPrompt)
                .Count;

            itemModels = new List<ScheduleItemViewModel>();
            int recordingItemIndex = 1;
            foreach (ScheduleItem item in schedule.Items)
            {
                var model = new ScheduleItemViewModel(item, appRepository);
                model.PropertyChanged += ItemViewModelPropertyChanged;

                if (item.IsRecording && !item.IsPrompt)
                {
                    model.CounterIndex = recordingItemIndex;
                    model.CounterTotal = totalRecordingItems;
                    recordingItemIndex++;
                }

                itemModels.Add(model);
            }

            if (schedule.Items.Count > 0)
            {
                currentItemIndex = 0;
                MoveBy(0);
            }
        }

        private void ItemViewModelPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(ScheduleItemViewModel.Answer))
            {
                Debug.WriteLine(string.Format("got answer change: {0}", ItemViewModel.Answer));
                UpdateContinueButton();
            }
        }

        private void CreateElapsedTimeModel()
        {
            elapsedTimeModel = new ElapsedTimeModel();
            elapsedTimeModel.SecondElapsed += OnSecondElapsed;
        }

        private void OnSecondElapsed(object sender, ElapsedTimeModel.SecondElapsedEventArgs e)
        {
            RecordingTimeText = e.ElapsedTime.ToString(@"m\:ss");

            if (e.ElapsedTime.Minutes >= appConfiguration.MaxRecordingMinutes
                && DisplayState == ScheduleItemStateType.Recording)
            {
                Debug.WriteLine("limit reached while recording");
                StopRecording();
                MaxRecordingTimeReached?.Invoke(this, EventArgs.Empty);
            }
        }

        private void UpdateRecordingTimeText()
        {
            switch (DisplayState)
            {
                case ScheduleItemStateType.Start:
                    {
                        RecordingTimeText = "0:00";
                        if (resourceDictionary.TryGetValue("InfoLabelStyle", out object resource))
                        {
                            RecordingTimeLabelStyle = resource as Style;
                        }
                        break;
                    }
                case ScheduleItemStateType.Recording:
                    {
                        if (resourceDictionary.TryGetValue("AccentLabelStyle", out object resource))
                        {
                            RecordingTimeLabelStyle = resource as Style;
                        }
                        break;
                    }
            }
        }

        private void CreateCommands()
        {
            NextCommand = new Command(
                execute: () => ShowNextItem(),
                canExecute: () => DisplayState == ScheduleItemStateType.Start);

            PreviousCommand = new Command(
                execute: () => ShowPreviousItem(),
                canExecute: () => DisplayState == ScheduleItemStateType.Start && currentItemIndex > 0);

            RecordCommand = new Command(
                execute: () => ToggleRecordingState());

            ContinueCommand = new Command(
                execute: () => ShowNextItem());

            RetryCommand = new Command(
                execute: () => DisplayState = ScheduleItemStateType.Start);
        }

        private void ToggleRecordingState()
        {
            if (recordingManager.IsRecording)
            {
                StopRecording();
            }
            else
            {
                StartRecording();
            }
        }

        private void UpdateItemViewModel()
        {
            if (currentItemIndex >= 0 && currentItemIndex < itemModels.Count)
            {
                // clean up, such as remove event handlers
                ItemViewModel?.ClearAfterDisplay();

                // clear the state of target view model before using it
                itemModels[currentItemIndex].PrepareForDisplay();

                ItemViewModel = itemModels[currentItemIndex];
            }
        }

        private void UpdateContinueButton()
        {
            if (ItemViewModel != null && ItemViewModel.IsPrompt && !ItemViewModel.HasAnswer)
            {
                // Skip only for a prompt that does not have an answer yet
                ContinueButtonText = AppResources.SkipScheduleItem;
                if (resourceDictionary.TryGetValue("textButtonStyle", out object resource))
                {
                    ContinueButtonStyle = resource as Style;
                }
            }
            else
            {
                ContinueButtonText = AppResources.ContinueSchedule;
                ContinueButtonStyle = null; // use default style
            }
        }

        private void UpdateRecordButton()
        {
            ShowRecordButton = ItemViewModel.IsRecordingEnabled
                       && !ItemViewModel.IsPrompt    
                       && DisplayState != ScheduleItemStateType.Finish;

            var buttonStyle = "recordButtonStyle";

            if (recordingManager.IsRecording)
            {
                buttonStyle = "recordButtonRecordingStyle";
                RecordButtonText = AppResources.StopRecording;
            }
            else
            {
                RecordButtonText = AppResources.StartRecording;
            }

            if (resourceDictionary.TryGetValue(buttonStyle, out object resource))
            {
                RecordButtonStyle = resource as Style;
            }
        }

        private void StartRecording()
        {
            recordingManager.StartRecording(ItemViewModel.Item.ItemId);
            DisplayState = ScheduleItemStateType.Recording;
            elapsedTimeModel.Start();
        }

        private void StopRecording()
        {
            recordingManager.FinalizeRecording(ItemViewModel.Answer);
            DisplayState = ScheduleItemStateType.Finish;
            elapsedTimeModel.Stop();
        }

        private void MoveBy(int indexOffset)
        {
            var newIndex = currentItemIndex + indexOffset;
            if (newIndex == itemModels.Count)
            {
                appRepository.AddCompletedScheduleId(schedule.ScheduleId);
                ScheduleFinished?.Invoke(this, EventArgs.Empty);
            }
            else if (newIndex >= 0)
            {
                currentItemIndex = newIndex;
                UpdateItemViewModel();
                DisplayState = ScheduleItemStateType.Start;
            }
        }

        private void SendSelectItemEvent(ScheduleItem item)
        {
            var dict = new Dictionary<string, string>
            {
                { AnalyticsParameterNamesConstants.ItemId, item.ItemId },
                { AnalyticsParameterNamesConstants.ItemName, item.Title.ToLocalString() },
                { AnalyticsParameterNamesConstants.ContentType, AnalyticsContentTypeConstants.ScheduleItem },
                { AnalyticsParameterNamesConstants.BuildType, appConfiguration.BuildType }
            };
            eventTracker.SendEvent(AnalyticsEventNamesConstants.SelectContent, dict);
        }

        private void SendUserAnswerCompletedEvent(ScheduleItem item, string answer)
        {
            eventTracker.SendEvent(new ScheduleItemCompletedEvent(
                item.ItemId, item.Title.ToLocalString(), answer, appConfiguration.BuildType));
        }

        private void ShowPreviousItem()
        {
            SaveAnswer();
            MoveBy(indexOffset: -1);
        }

        private void ShowNextItem()
        {
            SaveAnswer();
            MoveBy(indexOffset: 1);
        }

        private void SaveAnswer()
        {
            if (ItemViewModel.AnswerModified && ItemViewModel.HasAnswer)
            {
                string id = ItemViewModel.Item.ItemId;
                string answer = ItemViewModel.NoChoiceSelected ? string.Empty : ItemViewModel.Answer;

                Debug.WriteLine($"Saving a modified answer: {answer}");

                recordingManager.FinalizeAnswer(id, answer);
                appRepository.SaveAnswer(id, answer);
                SendUserAnswerCompletedEvent(ItemViewModel.Item, answer);
            }
        }

        // stops and saves current recording, and later the schedule can be continued with NextCommand
        // this intentionally does not collect metadata answer, since that is done in NextCommand
        public void PauseSchedule()
        {
            if (recordingManager.IsRecording)
            {
                StopRecording();
            }

            ItemViewModel.PauseSchedule();
            elapsedTimeModel.Stop();
        }
    }
}
