using System;
using Newtonsoft.Json.Linq;

namespace Recorder.Models
{
    public class UserAnswer
    {
        public string ItemId;    // the schedule item
        public string Value;     // the actual answer

        public JObject ToJsonObject()
        {
            return new JObject(
                new JProperty("itemId", this.ItemId),
                new JProperty("value", this.Value)
            );
        }
    }
}
