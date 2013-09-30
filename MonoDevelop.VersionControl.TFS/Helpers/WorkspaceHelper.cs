using System;
using Microsoft.TeamFoundation.VersionControl.Client;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Client;

namespace MonoDevelop.VersionControl.TFS.Helpers
{
    public static class WorkspaceHelper
    {
        public static List<Workspace> GetLocalWorkspaces(string serverName)
        {
            var server = TFSVersionControlService.Instance.GetServer(serverName);
            var credentials = CredentialsManager.LoadCredential(server.Url);
            using (var tfsServer = TeamFoundationServerFactory.GetServer(server.Url, credentials))
            {
                tfsServer.Authenticate();
                var versionControl = tfsServer.GetService<VersionControlServer>();

                return  versionControl.QueryWorkspaces(string.Empty, credentials.UserName, Environment.MachineName);
            }
        }

        public static List<Workspace> GetRemoteWorkspaces(string serverName)
        {
            var server = TFSVersionControlService.Instance.GetServer(serverName);
            var credentials = CredentialsManager.LoadCredential(server.Url);
            using (var tfsServer = TeamFoundationServerFactory.GetServer(server.Url, credentials))
            {
                tfsServer.Authenticate();
                var versionControl = tfsServer.GetService<VersionControlServer>();

                return  versionControl.QueryWorkspaces(string.Empty, credentials.UserName, string.Empty);
            }
        }
    }
}