using System;
using Autofac;
using MonoDevelop.VersionControl.TFS.Infrastructure.Settings;
using MonoDevelop.VersionControl.TFS.Infrastructure.Settings.Implementation;
using MonoDevelop.VersionControl.TFS.MonoDevelopWrappers;
using MonoDevelop.VersionControl.TFS.Tests.Core.MonoDevelopServices;

namespace MonoDevelop.VersionControl.TFS.Tests.Core
{
    internal sealed class TestServiceBuilder : ContainerBuilder
    {
        public TestServiceBuilder()
        {
            this.RegisterType<TestProjectService>().As<IProjectService>().SingleInstance();
            this.RegisterType<TestProgressService>().As<IProgressService>().SingleInstance();
            this.Register<ConfigurationService>(ctx =>
            {
                var service = new ConfigurationService();
                service.Init(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                return service;
            }).As<IConfigurationService>().SingleInstance();
            this.RegisterType<TestLoggingService>().As<ILoggingService>().SingleInstance();
        }
    }
}