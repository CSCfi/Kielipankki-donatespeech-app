using Recorder.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(Recorder.iOS.PermissionRequestInfo))]

namespace Recorder.iOS
{
    public class PermissionRequestInfo : IPermissionRequestInfo
    {
        public PermissionRequestInfo()
        {
        }

        // ios permission can only be requested once
        public bool IsRetryAllowedForDeniedMicrophone() => false;
    }
}
