using MonoDevelop.Core.Logging;

namespace MonoDevelop.VersionControl.TFS.MonoDevelopWrappers.Implementation
{
    sealed class LoggingService : ILoggingService
    {
        public void Log(string message)
        {
            MonoDevelop.Core.LoggingService.Log(LogLevel.Info, message);
        }
    }
}
