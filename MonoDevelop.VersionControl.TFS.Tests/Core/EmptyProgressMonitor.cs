using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoDevelop.Core;

namespace MonoDevelop.VersionControl.TFS.Tests.Core
{
    sealed class EmptyProgressMonitor : IProgressMonitor
    {
        readonly TextWriter log = new StringWriter(new StringBuilder());

        public void Dispose()
        {
            log.Dispose();
        }

        public void BeginTask(string name, int totalWork)
        {
        }

        public void BeginStepTask(string name, int totalWork, int stepSize)
        {
        }

        public void EndTask()
        {
        }

        public void Step(int work)
        {
        }

        public void ReportWarning(string message)
        {
        }

        public void ReportSuccess(string message)
        {
        }

        public void ReportError(string message, Exception exception)
        {
        }

        public TextWriter Log
        {
            get { return log; }
        }

        public bool IsCancelRequested
        {
            get { return false; }
        }

        public IAsyncOperation AsyncOperation
        {
            get { return null;  }
        }

        public object SyncRoot
        {
            get { return null; }
        }

        public event MonitorHandler CancelRequested;
    }
}
