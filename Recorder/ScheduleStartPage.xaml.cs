using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Recorder.Models;
using Recorder.ResX;
using Recorder.Services;
using Recorder.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;
using static Xamarin.Essentials.Permissions;

namespace Recorder
{
    public partial class ScheduleStartPage : ContentPage
    {
        private readonly ScheduleStartPageViewModel viewModel;
        private Task<bool> alertPopupTask;
        private readonly IPermissionRequestInfo permissionRequestInfo;
        private bool scheduleStarting;

        public ScheduleStartPage(string scheduleId)
        {
            InitializeComponent();

            var app = Application.Current as App;
            viewModel = new ScheduleStartPageViewModel(scheduleId, app.AppRepository,
                app.AnalyticsEventTracker, app.Config);

            viewModel.ScheduleLoadFailed += OnScheduleLoadFailed;
            BindingContext = viewModel;

            startImage.HeightRequest = DeviceDisplay.MainDisplayInfo.GetHeightInSixteenNine();

            // note: this can be replaced with a method from Xamarin Essentials 1.6 when available
            permissionRequestInfo = DependencyService.Get<IPermissionRequestInfo>();
        }

        async void StartButtonClickedAsync(object sender, EventArgs e)
        {
            if (scheduleStarting)
            {
                Debug.WriteLine("Ignoring schedule start since already starting");
                return;
            }
            scheduleStarting = true;

            if (viewModel.Schedule != null)
            {
                if (await CheckAndRequestPermissionAsync())
                {
                    // one way navigation
                    await Navigation.PushAsyncThenRemove(new SchedulePage(viewModel.Schedule), this);
                }
                else
                {
                    Debug.WriteLine("permission denied");
                }
            }
            scheduleStarting = false;
        }

        private async Task<bool> CheckAndRequestPermissionAsync()
        {
            Microphone mic = new Microphone();

            // ios status=unknown at first, and denied after has been denied
            // android status=denied at first

            if (await mic.CheckStatusAsync() == PermissionStatus.Granted)
            {
                return true;
            }

            // first make a request
            var status = await mic.RequestAsync();
            if (status == PermissionStatus.Granted)
            {
                return true;
            }

            // android allows retry, ios does not
            if (permissionRequestInfo.IsRetryAllowedForDeniedMicrophone())
            {
                await DisplayAlert(AppResources.PermissionRationaleTitle,
                    AppResources.PermissionRationaleMessage, AppResources.PermissionRationaleDismiss);
            }
            else
            {
                // ios: denied, android: denied and do not ask again -> user needs to fix manually
                await DisplayAlert(AppResources.PermissionDeniedTitle,
                    AppResources.PermissionDeniedMessage, AppResources.PermissionDeniedDismiss);
            }

            return false;
        }

        private async void OnScheduleLoadFailed(object sender, System.EventArgs e)
        {
            if (!IsAlertShowing)
            {
                alertPopupTask = DisplayAlert(AppResources.LoadFailedAlertTitle, AppResources.LoadFailedAlertMessage,
                    AppResources.LoadFailedAlertContinue, AppResources.LoadFailedAlertCancel);

                await alertPopupTask;

                if (alertPopupTask.Result)
                {
                    viewModel.LoadScheduleAsync();
                }
                else
                {
                    await Navigation.PopAsync();
                }
            }
        }

        private bool IsAlertShowing => alertPopupTask?.Status == TaskStatus.Running ||
                                       alertPopupTask?.Status == TaskStatus.WaitingForActivation;
    }
}
