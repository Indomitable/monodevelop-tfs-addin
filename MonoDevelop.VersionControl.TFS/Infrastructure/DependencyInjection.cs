// DependencyInjection.cs
// 
// Author:
//       Ventsislav Mladenov
// 
// The MIT License (MIT)
// 
// Copyright (c) 2013-2015 Ventsislav Mladenov
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using Autofac;
using MonoDevelop.Core;
using MonoDevelop.VersionControl.TFS.Core;
using MonoDevelop.VersionControl.TFS.Core.Services;
using MonoDevelop.VersionControl.TFS.Core.Structure;
using MonoDevelop.VersionControl.TFS.Infrastructure.Services;
using MonoDevelop.VersionControl.TFS.Infrastructure.Services.Implementation;
using MonoDevelop.VersionControl.TFS.Infrastructure.Settings;
using MonoDevelop.VersionControl.TFS.Infrastructure.Settings.Implementation;
using MonoDevelop.VersionControl.TFS.MonoDevelopWrappers;
using MonoDevelop.VersionControl.TFS.MonoDevelopWrappers.Implementation;
using MonoDevelop.VersionControl.TFS.VersionControl;
using MonoDevelop.VersionControl.TFS.VersionControl.Infrastructure;
using LoggingService = MonoDevelop.VersionControl.TFS.MonoDevelopWrappers.Implementation.LoggingService;

namespace MonoDevelop.VersionControl.TFS.Infrastructure
{
    static class DependencyInjection
    {
        public static void Register(ContainerBuilder builder)
        {
            builder.RegisterType<FileKeeperService>().As<IFileKeeperService>().SingleInstance();
            builder.RegisterType<TFSVersionControlService>().SingleInstance();
            builder.RegisterType<Workspace>().As<IWorkspace>();
            builder.RegisterType<SoapInvoker>().As<ISoapInvoker>();
            Container = builder.Build();
        }

        public static IContainer Container { get; private set; }

        public static IWorkspace GetWorkspace(WorkspaceData workspaceData, ProjectCollection collection)
        {
            return Container.Resolve<IWorkspace>(new TypedParameter(typeof(WorkspaceData), workspaceData),
                                                 new TypedParameter(typeof(ProjectCollection), collection));
        }

        public static TFSRepository GetTFSRepository(string path, WorkspaceData workspaceData, ProjectCollection collection)
        {
            using (var scope = Container.BeginLifetimeScope())
            {
                var workspace = GetWorkspace(workspaceData, collection);
                return scope.Resolve<TFSRepository>(new NamedParameter("rootPath", path), new TypedParameter(typeof (IWorkspace), workspace));
            }
        }

        public static ISoapInvoker GetSoapInvoker(TFSService service)
        {
            return Container.Resolve<ISoapInvoker>(new TypedParameter(typeof(TFSService), service));
        }
    }


    public class ServiceBuilder : ContainerBuilder
    {
        public ServiceBuilder()
        {
            this.RegisterType<ProjectService>().As<IProjectService>().SingleInstance();
            this.RegisterType<ProgressService>().As<IProgressService>().SingleInstance();
            this.Register<ConfigurationService>(ctx =>
            {
                var service = new ConfigurationService();
                service.Init(UserProfile.Current.ConfigDir);
                return service;
            }).As<IConfigurationService>().SingleInstance();
            this.RegisterType<LoggingService>().As<ILoggingService>().SingleInstance();
            this.RegisterType<NotificationService>().As<INotificationService>();
            this.RegisterType<TFSRepository>().InstancePerLifetimeScope().OnActivated(a => a.Instance.NotificationService = a.Context.Resolve<INotificationService>() );
            
        }
    }
}