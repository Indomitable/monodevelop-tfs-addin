using System;
using System.Text;
using MonoDevelop.VersionControl.TFS.Configuration;
using MonoDevelop.VersionControl.TFS.Core.ServerAuthentication;

namespace MonoDevelop.VersionControl.TFS.Core.Structure
{
    sealed class VisualStudioOnlineTFS : BaseTeamFoundationServer, IBasicAuthenticatedServer
    {

        public VisualStudioOnlineTFS(ServerConfig config)
            : base(config)
        {
        }

        public override string UserName
        {
            get
            {
                return Config.UserName;
            }
        }

        public string AuthorizationHeader 
        {
            get
            {
                var credentialBuffer = Encoding.UTF8.GetBytes(Config.AuthUserName + ":" + this.Password);
                return "Basic " + Convert.ToBase64String(credentialBuffer);
            }
        }
    }
}

