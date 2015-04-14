using System;
using System.IO;
using System.Text;
using MonoDevelop.Core.Logging;

namespace MonoDevelop.VersionControl.TFS.MonoDevelopWrappers.Implementation
{
    sealed class LoggingService : ILoggingService
    {
        readonly static object locker = new object();

        public bool IsDebugMode
        {
            get { return TFSVersionControlService.Instance.IsDebugMode; }
        }


        public void LogToDebug(string message)
        {
            if (IsDebugMode)
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
