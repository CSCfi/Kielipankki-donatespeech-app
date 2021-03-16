using System;
using System.Collections.Generic;

namespace Recorder.Models
{
    public class Schedule
    {
        public string ScheduleId;
        public Dictionary<string, string> Title;
        public Dictionary<string, string> Body1;
        public Dictionary<string, string> Body2;
        public ScheduleItemState Start;  // schedule has only start and finish
        public ScheduleItemState Finish;
        public List<ScheduleItem> Items;

        public Schedule()
        {
            Items = new List<ScheduleItem>();
        }
    }
}
