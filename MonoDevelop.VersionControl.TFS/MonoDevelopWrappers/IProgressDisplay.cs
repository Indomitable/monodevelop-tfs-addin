using System;

namespace MonoDevelop.VersionControl.TFS.MonoDevelopWrappers
{
    interface IProgressDisplay : IDisposable
    {
        void BeginTask(string message, int steps);
        void EndTask();
        bool IsCancelRequested { get; }
    }
}