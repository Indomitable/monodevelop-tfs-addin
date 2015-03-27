using MonoDevelop.VersionControl.TFS.Configuration;

namespace MonoDevelop.VersionControl.TFS.Core.Structure
{
    internal static class TeamFoundationServerFactory
    {
        public static BaseTeamFoundationServer Create(ServerConfig config)
        {
            if (config.Type == ServerType.OnPremise)
            {
                return new TeamFoundationServer(config);
            }
            else
            {
                return new VisualStudioOnlineTFS(config);
            }
        }
    }
}

