using System;
using System.Net;
using System.Xml.Linq;

namespace Microsoft.TeamFoundation.Client
{
    public static class TeamFoundationServerFactory
    {
        public static BaseTeamFoundationServer Create(ServerType serverType, BaseServerInfo serverInfo, ServerAuthentication auth, bool isPasswordSavedInXml)
        {
            if (serverType == ServerType.TFS)
            {
                return new TeamFoundationServer(serverInfo.Uri, serverInfo.Name, auth.Domain, auth.UserName, auth.Password, isPasswordSavedInXml);
            }
            else
            {
                var vsServerInfo = (VisualStudioServerInfo)serverInfo;
                return new VisualStudioOnlineTFS(vsServerInfo.Uri, vsServerInfo.Name, vsServerInfo.TFSUserName, auth.AuthUser, auth.Password, isPasswordSavedInXml);
            }
        }

        public static BaseTeamFoundationServer Create(XElement element, string password, bool isPasswordSavedInXml)
        {
            var type = element.Attribute("Type");
            if (type == null || (ServerType)Convert.ToInt32(type.Value) == ServerType.TFS)
            {
                return TeamFoundationServer.FromLocalXml(element, password, isPasswordSavedInXml);
            }
            else
            {
                return VisualStudioOnlineTFS.FromLocalXml(element, password, isPasswordSavedInXml);
            }
        }
    }
}

