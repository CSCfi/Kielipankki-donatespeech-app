using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Essentials;
using System.Windows.Input;
using System.Diagnostics;

namespace Recorder
{
    public partial class DetailsPage : ContentPage
    {
        public string ClientId
        {
            get { return Preferences.Get(Constants.ClientIdKey, "unknown"); }
        }

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

        public DetailsPage()
        {
            BindingContext = this;
            InitializeComponent();
        }

        async void CopyClientId_Clicked(System.Object sender, System.EventArgs e)
        {
            await Clipboard.SetTextAsync(ClientId);
            CopyClientIdButton.Text = ResX.AppResources.DetailsRemoveButtonClickedTitle;
        }
    }
}
