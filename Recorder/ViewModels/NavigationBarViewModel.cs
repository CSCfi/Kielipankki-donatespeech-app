using System;
using Xamarin.Essentials;

namespace Recorder.ViewModels
{
    public class NavigationBarViewModel : BaseViewmodel
    {
        private string _totalRecordedMinutes;
        public string TotalRecordedMinutes 
        {
            get => _totalRecordedMinutes;
            set => Set(ref _totalRecordedMinutes, value, nameof(TotalRecordedMinutes));
        }

        public NavigationBarViewModel()
        {
            Update();
        }

        public void Update()
        {
            int seconds = Preferences.Get(Constants.TotalRecordedSecondsKey, 0);
            int minutes = seconds / 60;
            if (minutes < 1)
            {
                TotalRecordedMinutes = $"<1 min";
            }
            else
            {
                TotalRecordedMinutes = $"{minutes} min";
            }
        }
    }
}
