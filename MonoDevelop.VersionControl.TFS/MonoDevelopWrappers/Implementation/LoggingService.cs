using System;
using System.IO;
using System.Text;
using MonoDevelop.Core.Logging;
using MonoDevelop.VersionControl.TFS.Infrastructure.Settings;

namespace MonoDevelop.VersionControl.TFS.MonoDevelopWrappers.Implementation
{
    sealed class LoggingService : ILoggingService
    {
        private readonly IConfigurationService _configurationService;
        readonly static object locker = new object();
        private Configuration _configuration;

        public LoggingService(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
            _configuration = _configurationService.Load();
        }

        public void LogToDebug(string message)
        {
            if (_configuration.IsDebugMode)
            {
                lock (locker)
                {
                    var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "TFS.VersionControl.Debug.log");
                    File.AppendAllText(path, message, Encoding.UTF8);
                }
            }
        }

        public void LogToInfo(string message)
        {
            MonoDevelop.Core.LoggingService.Log(LogLevel.Info, message);
        }
    }
}
