using MonoDevelop.Ide;
using MonoDevelop.Ide.ProgressMonitoring;

namespace MonoDevelop.VersionControl.TFS.MonoDevelopWrappers.Implementation
{
    sealed class ProgressDisplay : IProgressDisplay
    {
        private readonly MessageDialogProgressMonitor progressMonitor;

        public ProgressDisplay()
        {
            progressMonitor = new MessageDialogProgressMonitor(DispatchService.IsGuiThread, false, false);
        }

        public void Dispose()
        {
            progressMonitor.Dispose();
        }

        public void BeginTask(string message, int steps)
        {
            progressMonitor.BeginTask(message, steps);
        }

        public void EndTask()
        {
            progressMonitor.EndTask();
        }

        public bool IsCancelRequested { get { return progressMonitor.IsCancelRequested; } }
    }
}
