using System.Collections.Generic;
using Xamarin.Forms;
using Recorder.iOS;
using Firebase.Analytics;
using Foundation;

[assembly: Dependency(typeof(FirebaseAnalyticsEventTrackeriOS))]
namespace Recorder.iOS
{
    public class FirebaseAnalyticsEventTrackeriOS : IFirebaseAnalyticsEventTracker
    {
        public FirebaseAnalyticsEventTrackeriOS()
        {
        }

        public void SendEvent(string eventId)
        {
            SendEvent(eventId, (IDictionary<string, string>)null);
        }

        public void SendEvent(string eventId, string paramName, string value)
        {
            SendEvent(eventId, new Dictionary<string, string>
            {
                { paramName, value }
            });
        }

        public void SendEvent(string eventId, IDictionary<string, string> parameters)
        {
            if (parameters == null)
            {
                Analytics.LogEvent(eventId, (Dictionary<object, object>)null);
                return;
            }

            var keys = new List<NSString>();
            var values = new List<NSString>();
            foreach (var item in parameters)
            {
                keys.Add(new NSString(item.Key));
                values.Add(new NSString(item.Value));
            }

            System.Diagnostics.Debug.WriteLine(string.Format("log event {0}", eventId), "FirebaseAnalyticsEventTrackeriOS");

            var parametersDictionary =
                NSDictionary<NSString, NSObject>.FromObjectsAndKeys(values.ToArray(), keys.ToArray(), keys.Count);
            Analytics.LogEvent(eventId, parametersDictionary);
        }

        public void SendEvent(AnalyticsEvent analyticsEvent)
            => SendEvent(analyticsEvent.EventId, analyticsEvent.EventParameters);
    }
}
