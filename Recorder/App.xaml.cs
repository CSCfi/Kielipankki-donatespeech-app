using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Essentials;

using Recorder.Models;
using Recorder.Services;

namespace Recorder
{
    public partial class App : Application
    {
        public IRecorderApi RecorderApi { get; private set; }
        public IAppRepository AppRepository { get; private set; }
        public IAppPreferences AppPreferences { get; private set; }
        public RecordingManager RecMan { get; private set; }

        private static IAppDatabase _database;
        public static IAppDatabase Database => GetDatabase();

        public IFirebaseAnalyticsEventTracker AnalyticsEventTracker;
        public IAppConfiguration Config { get; private set; }

        private long schedulerLockCounter = 0;

        public event EventHandler AppSleep;
        public event EventHandler AppResume;

        public int TotalRecordedSeconds;  // total recorded time from preferences

        public App()
        {
            InitializeComponent();
            InitializeServices();

            // Generate and save an instance ID if one does not already exist
            if (!Preferences.ContainsKey(Constants.ClientIdKey))
            {
                string guidString = Guid.NewGuid().ToString();
                Preferences.Set(Constants.ClientIdKey, guidString);
                Debug.WriteLine($"Created new client ID: {guidString}");
            }
            Debug.WriteLine(string.Format("From preferences, clientID = {0}", Preferences.Get(Constants.ClientIdKey, "unknown")));

            // Force the user language for testing:
            Preferences.Set(Constants.UserLanguageKey, "fi");

            // Initialize the total number of seconds recorded from preferences.
            // If this preference is not found, it is initialized to zero.
            // The number should be updated after each completed recording.
            // Use the UpdateTotalRecorded function in this class for that.
            TotalRecordedSeconds = Preferences.Get(Constants.TotalRecordedSecondsKey, 0);
            Debug.WriteLine($"Total time recorded: {TotalRecordedSeconds} seconds");
            // Use NavigationBarViewModel to get a formatted representation.

            //App.Database.DeleteAllRecordings();  // or maybe not
            //this.AppRepository.ListRecordingsInDatabase();

            this.AppRepository.ListUploadedRecordings();

            StartUploadScheduler(schedulerLockCounter);

            MainPage = new NavigationPage(GetInitialPage());
        }

        private Page GetInitialPage()
        {
            bool onboardingCompleted = Preferences.Get(Constants.OnboardingCompletedKey, false);

            if (Config.AlwaysShowOnboarding || !onboardingCompleted)
            {               
                return new OnboardingPage();
            }
            else
            {
                return new ThemesPage();
            }
        }

        private void InitializeServices()
        {
            Config = AppConfiguration.Load();

            RecorderApi = new RecorderApi(Config);
            AppPreferences = new AppPreferences();
            AppRepository = new AppRepository(RecorderApi, AppPreferences);

            RecMan = new RecordingManager(Config);
            AnalyticsEventTracker = DependencyService.Get<IFirebaseAnalyticsEventTracker>();

        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
            AppSleep?.Invoke(this, new EventArgs());
            schedulerLockCounter = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        protected override void OnResume()
        {
            AppResume?.Invoke(this, new EventArgs());
            StartUploadScheduler(schedulerLockCounter);
        }

        private void StartUploadScheduler(long timerLock)
        {
            Debug.WriteLine("Starting timer for lock value {0}", timerLock);
            Xamarin.Forms.Device.StartTimer(TimeSpan.FromSeconds(Constants.PendingUploadsTimerIntervalSeconds), () =>
            {
                Debug.WriteLine("Running upload task");
                if (timerLock != schedulerLockCounter)
                {
                    Debug.WriteLine("Initialized lock {0} doesn't match the current {1}", timerLock, schedulerLockCounter);
                    return false;
                }

                if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    Task.Run(async () =>
                    {
                        try
                        {
                            await this.AppRepository.UploadPendingRecordings();
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine("Failed to upload pending recordings");
                            Debug.WriteLine(e);
                        }
                    });
                }
                else
                {
                    Debug.WriteLine("Skipping upload because no network");
                }

                return true;
            });
        }

        private static IAppDatabase GetDatabase()
        {
            if (_database == null)
            {
                _database = new AppDatabase();
            }

            return _database;
        }

        public void UpdateTotalRecorded(int secondsToAdd)
        {
            TotalRecordedSeconds += Math.Abs(secondsToAdd);  // should never be negative, but hey...
            Preferences.Set(Constants.TotalRecordedSecondsKey, TotalRecordedSeconds);
            Debug.WriteLine($"Total recorded seconds updated to {TotalRecordedSeconds}");
        }
    }
}
