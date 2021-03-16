using System;
namespace Recorder.Services
{
    public interface IPermissionRequestInfo
    {
        // when permissions request returns Denied, use this to check if app can ask again
        bool IsRetryAllowedForDeniedMicrophone();
    }
}
