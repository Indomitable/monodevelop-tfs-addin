using System;
using System.Reflection;
using Autofac;
using MonoDevelop.VersionControl.TFS.Core.Structure;
using MonoDevelop.VersionControl.TFS.MonoDevelopWrappers;
using MonoDevelop.VersionControl.TFS.MonoDevelopWrappers.Implementation;
using MonoDevelop.VersionControl.TFS.VersionControl;
using MonoDevelop.VersionControl.TFS.VersionControl.Structure;

namespace MonoDevelop.VersionControl.TFS.Infrastructure
{
    static class DependencyInjection
    {
        public static void Register(ContainerBuilder builder)
        {
            builder.RegisterType<Workspace>().As<IWorkspace>();
//                .FindConstructorsWith(t => 
//                new [] { t.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof() }, null) });
            Container = builder.Build();
        }

        public static IContainer Container { get; private set; }

        public static IWorkspace GetWorkspace(WorkspaceData workspaceData, ProjectCollection collection)
        {
            return Container.Resolve<IWorkspace>(new TypedParameter(typeof(WorkspaceData), workspaceData),
                                                 new TypedParameter(typeof(ProjectCollection), collection));
        }
    }


    public class ServiceBuilder : ContainerBuilder
    {
        public ServiceBuilder()
        {
            this.RegisterType<ProjectService>().As<IProjectService>().SingleInstance();
            this.RegisterType<LoggingService>().As<ILoggingService>().SingleInstance();
            this.RegisterType<ProgressService>().As<IProgressService>().SingleInstance();
        }
    }
}