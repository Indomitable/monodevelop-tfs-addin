using System;
using System.Xml.Linq;
using MonoDevelop.VersionControl.TFS.Core.ServerAuthorization.Config;
using MonoDevelop.VersionControl.TFS.GUI.Server.Authorization;

namespace MonoDevelop.VersionControl.TFS.Core.ServerAuthorization
{
    static class ServerAuthorizationFactory
    {
        public static IServerAuthorization GetServerAuthorization(XElement element, Uri serverUri)
        {
            ServerAuthorizationType authorizationType;
            Enum.TryParse(element.Name.LocalName, out authorizationType);
            switch (authorizationType)
            {
                case ServerAuthorizationType.Ntlm:
                    return NtlmAuthorization.FromConfigXml(element, serverUri);
                case ServerAuthorizationType.Basic:
                    return BasicAuthorization.FromConfigXml(element, serverUri);
                default:
                    return new NoAuthorization();
            }
        }

        public static IServerAuthorizationConfig GetServerAuthorizationConfig(ServerAuthorizationType authorizationType, Uri serverUri)
        {
            switch (authorizationType)
            {
                case ServerAuthorizationType.Ntlm:
                    return new NtlmAuthorizationConfig(serverUri);
                case ServerAuthorizationType.Basic:
                    return new UserPasswordAuthorizationConfig(serverUri);
                default:
                    return new NoAuthorizationConfig();
            }
        }

        public static IServerAuthorization GetServerAuthorization(ServerAuthorizationType authorizationType, IServerAuthorizationConfig serverAuthorizationConfig)
        {
            switch (authorizationType)
            {
                case ServerAuthorizationType.Ntlm:
                    return NtlmAuthorization.FromConfigWidget((INtlmAuthorizationConfig) serverAuthorizationConfig);
                case ServerAuthorizationType.Basic:
                    return BasicAuthorization.FromConfigWidget((IUserPasswordAuthorizationConfig)serverAuthorizationConfig);
                default:
                    return new NoAuthorization();
            }
        }
    }
}