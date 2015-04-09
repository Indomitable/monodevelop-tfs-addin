using MonoDevelop.VersionControl.TFS.MonoDevelopWrappers;
using MonoDevelop.VersionControl.TFS.MonoDevelopWrappers.Implementation;
using MonoDevelop.VersionControl.TFS.VersionControl;
using SimpleInjector;

namespace MonoDevelop.VersionControl.TFS.Infrastructure
{
    class DependencyInjection
    {
        private static readonly Container container = new Container();

        public void Register()
        {
            container.RegisterSingle<IProjectService, ProjectService>();
            container.RegisterSingle<ILoggingService, LoggingService>();
            container.RegisterSingle<IProgressService, ProgressService>();
            container.RegisterSingle<IContextProvider, ContextProvider>();
            container.Register<IWorkspace, Workspace>();
        }

        public static Container Container { get { return container; } }
    }
}