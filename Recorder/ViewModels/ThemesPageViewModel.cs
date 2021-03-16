using System;
using System.Collections.Generic;
using System.Diagnostics;
using Recorder.Models;
using Recorder.Services;

namespace Recorder.ViewModels
{
    public class ThemesPageViewModel : BaseViewmodel
    {
        private readonly IAppRepository appRepository;

        public event EventHandler<EventArgs> ThemeLoadFailed;

        public ThemesPageViewModel(IAppRepository appRepository)
        {
            this.appRepository = appRepository;
        }

        public async void ReloadIfNeeded()
        {
            // Read completed schedules from preferences
            List<string> completedScheduleIds = appRepository.GetCompletedScheduleIds();

            // TEST BEGIN 
            Debug.WriteLine("Completed schedule IDs from user preferences:");
            if (completedScheduleIds?.Count > 0)
            {
                foreach (string scheduleId in completedScheduleIds)
                {
                    Debug.WriteLine(scheduleId);
                }
            }
            else
            {
                Debug.WriteLine("None.");
            }
            // TEST END

            if (!IsLoading)
            {
                if (ThemeModels == null)
                {
                    IsLoading = true;

                    Result<List<Theme>> result = await appRepository.GetAllThemesAsync();
                    if (result.Succeeded)
                    {
                        ThemeModels = result.Data
                            .FindAll(t => t?.Content?.ScheduleIds?.Count > 0)
                            .ConvertAll(t => new ThemeViewModel(t));
                    }
                    else
                    {
                        Debug.WriteLine("Failed to load themes");
                        ThemeLoadFailed?.Invoke(this, EventArgs.Empty);
                    }
                    IsLoading = false;
                }

                // update completed flags on every call.. data binding will update list data template
                ThemeModels?.ForEach(t =>
                    t.IsCompleted = completedScheduleIds?.Contains(t.FirstScheduleId) == true);
            }
        }

        private bool _loading;
        public bool IsLoading
        {
            get => _loading;
            set => Set(ref _loading, value, nameof(IsLoading));
        }

        private List<ThemeViewModel> _themeModels;
        public List<ThemeViewModel> ThemeModels
        {
            get => _themeModels;
            set => Set(ref _themeModels, value, nameof(ThemeModels));
        }
    }
}
