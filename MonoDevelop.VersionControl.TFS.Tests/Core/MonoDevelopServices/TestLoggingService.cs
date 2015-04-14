using System;
using MonoDevelop.VersionControl.TFS.MonoDevelopWrappers;

namespace MonoDevelop.VersionControl.TFS.Tests.Core.MonoDevelopServices
{
    internal sealed class TestLoggingService : ILoggingService
    {
        public bool IsDebugMode { get { return true; } }

        public void LogToDebug(string message)
        {
            Console.WriteLine(message);
        }

        public void LogToInfo(string message)
        {
            Console.WriteLine(message);
        }
    }
}