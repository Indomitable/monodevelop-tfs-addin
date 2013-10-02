using System;
using Microsoft.TeamFoundation.VersionControl.Client;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Client;
using MonoDevelop.VersionControl.TFS.Infrastructure.Objects;

namespace MonoDevelop.VersionControl.TFS.Helpers
{
    public static class WorkspaceHelper
    {
        public static List<Workspace> GetLocalWorkspaces(ServerEntry server)
        {
            var credentials = CredentialsManager.LoadCredential(server.Url);
            using (var tfsServer = TeamFoundationServerFactory.GetServer(server.Url, credentials))
            {
                tfsServer.Authenticate();
                var versionControl = tfsServer.GetService<VersionControlServer>();

                return  versionControl.QueryWorkspaces(credentials.UserName, Environment.MachineName);
            }
        }

        public static List<Workspace> GetRemoteWorkspaces(ServerEntry server)
        {
            var credentials = CredentialsManager.LoadCredential(server.Url);
            using (var tfsServer = TeamFoundationServerFactory.GetServer(server.Url, credentials))
            {
                tfsServer.Authenticate();
                var versionControl = tfsServer.GetService<VersionControlServer>();

                return  versionControl.QueryWorkspaces(credentials.UserName, string.Empty);
            }
        }

        public static Workspace GetWorkspace(ServerEntry server, string name)
        {
            var credentials = CredentialsManager.LoadCredential(server.Url);
            using (var tfsServer = TeamFoundationServerFactory.GetServer(server.Url, credentials))
            {
                tfsServer.Authenticate();
                var versionControl = tfsServer.GetService<VersionControlServer>();

                return  versionControl.GetWorkspace(name, credentials.UserName);
            }
        }
    }
}