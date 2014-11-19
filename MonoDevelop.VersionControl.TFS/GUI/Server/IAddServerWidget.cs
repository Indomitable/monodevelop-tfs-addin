using Microsoft.TeamFoundation.Client;

namespace MonoDevelop.VersionControl.TFS.GUI.Server
{
    public interface IAddServerWidget
    {
        BaseServerInfo ServerInfo { get; }
    }
}

