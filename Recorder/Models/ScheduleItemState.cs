using System;
using System.Collections.Generic;

namespace Recorder.Models
{
    public class ScheduleItemState
    {
        public Dictionary<string, string> Title;
        public Dictionary<string, string> Body1;
        public Dictionary<string, string> Body2;
        public string ImageUrl;
    }

    public enum ScheduleItemStateType
    {
        Start,
        Recording,
        Finish
    }

    public class ScheduleItemStateNames
    {
        public const string Start = "start";
        public const string Recording = "recording";
        public const string Finish = "finish";
    }
}
