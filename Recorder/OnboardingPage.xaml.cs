using System;
using Recorder.Views;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Recorder
{
    public partial class OnboardingPage : ContentPage
    {
        public OnboardingPage()
        {
            InitializeComponent();
        }

        async void ContinueButton_Clicked(object sender, EventArgs e)
        {
            // onboarding is complete only when terms accepted
            await Navigation.PushAsync(new TermsConditionsPage());
        }
    }
}
