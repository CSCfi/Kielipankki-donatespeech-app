using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using Recorder.ResX;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Recorder.Views
{
    public partial class TermsConditionsPage : ContentPage
    {
        public ICommand TapLinkCommand => new Command<string>(
            async (url) => await Launcher.OpenAsync(url));

        public ICommand TapEmailCommand => new Command<string>(
            async (recipient) =>
            {
                try
                {
                    await Email.ComposeAsync(string.Empty, string.Empty, new string[] { recipient });
                }
                catch (FeatureNotSupportedException e)
                {
                    // email is not supported on ios simulator and will throw
                    Debug.WriteLine(e);
                }
            });

        public TermsConditionsPage()
        {
            InitializeComponent();
            BindingContext = this;
            acceptButton.Clicked += AcceptButton_Clicked;
        }

        private void AcceptButton_Clicked(object sender, EventArgs e)
        {
            Preferences.Set(Constants.OnboardingCompletedKey, true);
            Navigation.PushAsyncThenClearHistory(new ThemesPage());
        }
    }
}
