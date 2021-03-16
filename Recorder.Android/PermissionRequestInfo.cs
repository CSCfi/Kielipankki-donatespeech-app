using System;
using Recorder.Services;
using AndroidX.Core.App;
using Android;
using Xamarin.Forms;

[assembly: Dependency(typeof(Recorder.Droid.PermissionRequestInfo))]

namespace Recorder.Droid
{
    public class PermissionRequestInfo : IPermissionRequestInfo
    {
        public PermissionRequestInfo()
        {
        }

        // this method is only valid if request status = Denied
        public bool IsRetryAllowedForDeniedMicrophone()
            => ActivityCompat.ShouldShowRequestPermissionRationale(MainActivity.Instance, Manifest.Permission.RecordAudio);

        // ShouldShowRequestPermissionRationale returns false if a user has denied a permission and
        // selected the Don't ask again option in the permission request dialog, or if a device policy prohibits the permission.
    }
}
