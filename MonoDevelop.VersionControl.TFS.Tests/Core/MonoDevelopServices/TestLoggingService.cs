using System;
using MonoDevelop.VersionControl.TFS.MonoDevelopWrappers;

namespace MonoDevelop.VersionControl.TFS.Tests.Core.MonoDevelopServices
{
    internal sealed class TestLoggingService : ILoggingService
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}