using MonoDevelop.VersionControl.TFS.Core.Structure;
using MonoDevelop.VersionControl.TFS.VersionControl.Structure;

namespace MonoDevelop.VersionControl.TFS
{
    internal sealed class TFSContext
    {
        private static TFSContext _instance;

        public static TFSContext Current { get { return _instance ?? (_instance = new TFSContext()); } }

        public void Set(ProjectCollection projectCollection, WorkspaceData workspaceData)
        {
            this.ProjectCollection = projectCollection;
            this.WorkspaceData = workspaceData;
        }

        public ProjectCollection ProjectCollection { get; private set; }

        public WorkspaceData WorkspaceData { get; private set; }
    }

    interface IContextProvider
    {
        TFSContext GetContext();
    }

    internal sealed class ContextProvider : IContextProvider
    {
        public TFSContext GetContext()
        {
            return TFSContext.Current;
        }
    }
}