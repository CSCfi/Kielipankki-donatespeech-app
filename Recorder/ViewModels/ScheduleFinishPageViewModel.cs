using Recorder.Models;

namespace Recorder.ViewModels
{
    public class ScheduleFinishPageViewModel
    {
        public string Title { get; private set; }
        public string RewardImageUrl { get; private set; }
        public string Body1 { get; private set; }
        public string Body2 { get; private set; }

        public ScheduleFinishPageViewModel(Schedule schedule)
        {
            // Use schedule defaults if nothing else specified
            var titleDict = schedule.Finish?.Title ?? schedule.Title;
            var body1Dict = schedule.Finish?.Body1 ?? schedule.Body1;
            var body2Dict = schedule.Finish?.Body2 ?? schedule.Body2;

            Title = titleDict.ToLocalString();
            Body1 = body1Dict.ToLocalString();
            Body2 = body2Dict.ToLocalString();
            RewardImageUrl = schedule.Finish?.ImageUrl;
        }
    }
}
