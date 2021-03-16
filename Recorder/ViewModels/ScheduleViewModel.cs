using System;

using Recorder.Models;


namespace Recorder
{
    public class ScheduleViewModel
    {
        private Schedule schedule;

        private LanguageString _title;
        private LanguageString _body1;
        private LanguageString _body2;

        public string Title => _title.Localized;
        public string Body1 => _body1.Localized;
        public string Body2 => _body2.Localized;

        public string TestId => schedule.ScheduleId;

        public ScheduleViewModel(Schedule schedule)
        {
            this.schedule = schedule;

            this._title = new LanguageString()
            {
                Strings = schedule.Title
            };
            this._body1 = new LanguageString()
            {
                Strings = schedule.Body1
            };
            this._body2 = new LanguageString()
            {
                Strings = schedule.Body2
            };
        }
    }
}
