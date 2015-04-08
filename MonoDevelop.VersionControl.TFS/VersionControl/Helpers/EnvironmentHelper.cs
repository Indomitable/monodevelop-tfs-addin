using System;

namespace MonoDevelop.VersionControl.TFS.VersionControl.Helpers
{
    internal static class EnvironmentHelper
    {
        internal static bool IsRunningOnUnix
        {
            get
            {
                var p = Environment.OSVersion.Platform;
                return (p == PlatformID.Unix) || (p == PlatformID.MacOSX);
            }
        }
    }
}
