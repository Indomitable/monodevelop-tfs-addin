using System;
using System.Xml.Linq;

namespace MonoDevelop.VersionControl.TFS.Core.ServerAuthorization
{
    static class ServerAuthorizationFactory
    {
        public static IServerAuthorization GetServerAuthorization(XElement element, Uri serverUri)
        {
            switch (element.Name.LocalName)
            {
                case "Ntlm":
                    return NtlmAuthorization.FromConfigXml(element, serverUri);
                case "Basic":
                    return BasicAuthorization.FromConfigXml(element, serverUri);
                default:
                    return new NoAuthorization();
            }
        }
    }
}