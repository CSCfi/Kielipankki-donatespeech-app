using System;

using Recorder.Models;
using Xamarin.Forms;

namespace Recorder.ViewModels
{
    public class ThemeViewModel : BaseViewmodel
    {
        private Theme theme;

        private LanguageString _title;
        private LanguageString _body1;
        private LanguageString _body2;

        public string Title => _title.Localized;
        public string Body1 => _body1.Localized;
        public string Body2 => _body2.Localized;

        public string ImageUrl => theme.Content.Image;
        public string TestId => theme.Id;

        public string ThemeId => theme.Id;

        public string FirstScheduleId
        {
            get 
            {
                if (theme.Content?.ScheduleIds?.Count > 0)
                {
                    return theme.Content.ScheduleIds[0];
                }
                else
                {
                    return null;
                }
            }
        }

        public Color ButtonBackgroundColor
        {
            get
            {
                return this.IsCompleted ?
                    (Color)Application.Current.Resources["ThirdColor"] :
                    (Color)Application.Current.Resources["FirstColor"];
            }
        }

        private bool _isCompleted = false;
        public bool IsCompleted
        {
            get => _isCompleted;
            set => Set(ref _isCompleted, value, nameof(IsCompleted), nameof(ButtonBackgroundColor));
        }

        public ThemeViewModel(Theme theme)
        {
            this.theme = theme;

            this._title = new LanguageString()
            {
                Strings = theme.Content.Title
            };
            this._body1 = new LanguageString()
            {
                Strings = theme.Content.Body1
            };
            this._body2 = new LanguageString()
            {
                Strings = theme.Content.Body2
            };
        }
    }
}

