using Autofac;
using MonoDevelop.VersionControl.TFS.MonoDevelopWrappers;
using MonoDevelop.VersionControl.TFS.Tests.Core.MonoDevelopServices;

namespace MonoDevelop.VersionControl.TFS.Tests.Core
{
    internal sealed class TestServiceBuilder : ContainerBuilder
    {
        public TestServiceBuilder()
        {
            this.RegisterType<TestProjectService>().As<IProjectService>().SingleInstance();
            this.RegisterType<TestLoggingService>().As<ILoggingService>().SingleInstance();
            this.RegisterType<TestProgressService>().As<IProgressService>().SingleInstance();
        }
    }
}