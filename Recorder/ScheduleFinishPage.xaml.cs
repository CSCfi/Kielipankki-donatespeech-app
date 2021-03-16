using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Recorder.Models;
using Recorder.ResX;
using Recorder.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Recorder
{
    public partial class ScheduleFinishPage : ContentPage
    {
        public ScheduleFinishPage(Schedule schedule)
        {
            BindingContext = new ScheduleFinishPageViewModel(schedule);
            InitializeComponent();

            rewardImage.HeightRequest = DeviceDisplay.MainDisplayInfo.GetHeightInSixteenNine();
        }

        async void InviteButtonClicked(object sender, EventArgs e)
        {
            int seconds = Preferences.Get(Constants.TotalRecordedSecondsKey, 0);
            int minutes = seconds / 60;

            string shareTemplate = AppResources.InviteFriendTemplate;
            if (minutes < 2)
            {
                shareTemplate = AppResources.InviteFriendNewbieTemplate;
            }
            string shareText = string.Format(shareTemplate, minutes);
            Debug.WriteLine($"Share text = '{shareText}'");

            await Share.RequestAsync(new ShareTextRequest
            {
                Text = shareText,
                Title = AppResources.InviteFriendTitle
            });
        }

        void ContinueButtonClicked(object sender, EventArgs e) => Navigation.PopAsync();
    }
}
