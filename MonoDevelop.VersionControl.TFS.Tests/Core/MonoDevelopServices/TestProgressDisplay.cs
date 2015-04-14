using System;
using MonoDevelop.VersionControl.TFS.MonoDevelopWrappers;

namespace MonoDevelop.VersionControl.TFS.Tests.Core.MonoDevelopServices
{
    internal sealed class TestProgressDisplay : IProgressDisplay
    {
        public TestProgressDisplay()
        {
            IsCancelRequested = false;
        }

        public void Dispose()
        {
        }

        public void BeginTask(string message, int steps)
        {
            Console.WriteLine("Start {0} with {1} steps", message, steps);
        }

        public void EndTask()
        {
        }

        public bool IsCancelRequested { get; private set; }
    }
}