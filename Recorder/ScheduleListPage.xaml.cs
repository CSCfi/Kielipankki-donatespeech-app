using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xamarin.Forms;

using Recorder.Models;
using Recorder.ViewModels;
using Recorder.Services;
using Xamarin.Essentials;
using static Xamarin.Essentials.Permissions;
using System.Threading.Tasks;

namespace Recorder
{
    // this is not in use
    public partial class ScheduleListPage : ContentPage
    {
        public List<string> scheduleIds;
        public List<Schedule> schedules;

        public ScheduleListPage(string title, List<string> scheduleIds)
        {
            Title = title;

            this.scheduleIds = new List<string>();
            this.scheduleIds.AddRange(scheduleIds);

            /*
            Debug.WriteLine("This theme references the following schedule IDs:");
            foreach (string scheduleId in this.scheduleIds)
            {
                Debug.WriteLine(scheduleId);
            }
            */

            this.schedules = new List<Schedule>();

            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // prevents reloading schedules when navigating back to this page
            if (this.schedules.Count == 0)
            {
                //Debug.WriteLine("Load schedules from API");
                List<ScheduleViewModel> viewModels = new List<ScheduleViewModel>();

                var app = Application.Current as App;

                this.schedules.Clear();
                foreach (string scheduleId in this.scheduleIds)
                {
                    //Debug.WriteLine($"About to download schedule {scheduleId}");
                    Result<Schedule> result = await app.AppRepository.GetScheduleAsync(scheduleId);
                    if (result.Succeeded)
                    {
                        Debug.WriteLine($"Loaded schedule '{result.Data.ScheduleId}'");
                        this.schedules.Add(result.Data);
                    }
                    else
                    {
                        Debug.WriteLine($"Failed to load schedule '{scheduleId}'");
                    }
                }

                foreach (Schedule schedule in this.schedules)
                {
                    viewModels.Add(new ScheduleViewModel(schedule));
                }

                this.scheduleListView.ItemsSource = viewModels;
            }
        }

        async void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItemIndex < 0 || e.SelectedItem == null)
            {
                return;
            }
            ((ListView)sender).SelectedItem = null;

            Schedule selectedSchedule = this.schedules[e.SelectedItemIndex];

            var title = new LanguageString
            {
                Strings = selectedSchedule.Title
            };

            var app = Application.Current as App;
            var dict = new Dictionary<string, string>
            {
                { AnalyticsParameterNamesConstants.ItemId, selectedSchedule.ScheduleId },
                { AnalyticsParameterNamesConstants.ItemName, title.Localized },
                { AnalyticsParameterNamesConstants.ContentType, AnalyticsContentTypeConstants.Schedule },
                { AnalyticsParameterNamesConstants.BuildType, app.Config.BuildType }
            };

            app.AnalyticsEventTracker.SendEvent(AnalyticsEventNamesConstants.SelectContent, dict);

            var result = await CheckAndRequestPermissionAsync(new Microphone());
            if (result == PermissionStatus.Granted)
            {
                Debug.WriteLine("permissions ok");

                // refactor back if schedule list page is used
                ////await Navigation.PushAsync(new ScheduleStartPage(selectedSchedule));
            }
            else
            {
                // show rationale dialog
                Debug.WriteLine("permissions denied");
            }            
        }

        private async Task<PermissionStatus> CheckAndRequestPermissionAsync<T>(T permission)
            where T : BasePermission
        {
            var status = await permission.CheckStatusAsync();
            if (status != PermissionStatus.Granted)
            {
                status = await permission.RequestAsync();
            }

            return status;
        }
    }
}

