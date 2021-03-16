using System;
using System.Collections.Generic;

namespace Recorder.Models
{
    public class ThemeContent
    {
        public Dictionary<string, string> Title;
        public Dictionary<string, string> Body1;
        public Dictionary<string, string> Body2;
        public string Image;  // image URL
        public List<string> ScheduleIds;
    }
}
