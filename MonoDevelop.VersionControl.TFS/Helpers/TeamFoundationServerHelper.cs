using System;
using Microsoft.TeamFoundation.Client;
using MonoDevelop.VersionControl.TFS.Infrastructure.Objects;

namespace MonoDevelop.VersionControl.TFS.Helpers
{
    public static class TeamFoundationServerHelper
    {
        public static TeamFoundationServer GetServer(ServerEntry server)
        {
            var credentials = CredentialsManager.LoadCredential(server.Url);
            return TeamFoundationServerFactory.GetServer(server.Url, credentials);
        }
    }
}

