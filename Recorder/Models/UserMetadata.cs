using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Recorder.Models
{
    public class UserMetadata
    {
        public List<UserAnswer> Answers;

        public UserMetadata()
        {
            this.Answers = new List<UserAnswer>();
        }

        public void AddAnswer(UserAnswer answer)
        {
            this.Answers.Add(answer);
        }

        public void ClearAnswers()
        {
            this.Answers.Clear();
        }

        public JObject ToJsonObject()
        {
            List<JObject> answerObjects = new List<JObject>();
            foreach (UserAnswer answer in this.Answers)
            {
                answerObjects.Add(answer.ToJsonObject());
            }

            return new JObject(
                new JProperty("answers", JArray.FromObject(answerObjects))
            );
        }
    }
}
