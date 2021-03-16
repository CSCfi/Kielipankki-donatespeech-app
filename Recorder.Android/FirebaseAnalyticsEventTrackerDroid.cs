using System.Collections.Generic;
using Android.OS;
using Firebase.Analytics;
using Recorder.Droid;
using Xamarin.Forms;

[assembly: Dependency(typeof(FirebaseAnalyticsEventTrackerDroid))]
namespace Recorder.Droid
{
    public class FirebaseAnalyticsEventTrackerDroid : IFirebaseAnalyticsEventTracker
    {
        public FirebaseAnalyticsEventTrackerDroid()
        {
        }

        public void SendEvent(string eventId)
        {
            SendEvent(eventId, null);
        }

        public void SendEvent(string eventId, string paramName, string value)
        {
            SendEvent(eventId, new Dictionary<string, string>
            {
                {paramName, value}
            });
        }

        public void SendEvent(string eventId, IDictionary<string, string> parameters)
        {
            var firebaseAnalytics = FirebaseAnalytics.GetInstance(Android.App.Application.Context);

            if (parameters == null)
            {
                firebaseAnalytics.LogEvent(eventId, null);
                return;
            }

            var bundle = new Bundle();
            foreach (var param in parameters)
            {
                bundle.PutString(param.Key, param.Value);
            }

            System.Diagnostics.Debug.WriteLine(string.Format("log event {0}", eventId), "FirebaseAnalyticsEventTrackerDroid");
            firebaseAnalytics.LogEvent(eventId, bundle);
        }

        public void SendEvent(AnalyticsEvent analyticsEvent)
            => SendEvent(analyticsEvent.EventId, analyticsEvent.EventParameters);
    }
}
