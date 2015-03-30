using System;
using System.Net;
using System.Xml.Linq;
using MonoDevelop.VersionControl.TFS.Helpers;

namespace MonoDevelop.VersionControl.TFS.Core.ServerAuthorization
{
    sealed class NtlmAuthorization : UserPasswordAuthorization, IServerAuthorization
    {
        private NtlmAuthorization()
        {
            
        }

        public void Authorize(HttpWebRequest request)
        {
            request.Credentials = new NetworkCredential(UserName, Password, Domain);
        }

        public void Authorize(WebClient client)
        {
            client.Credentials = new NetworkCredential(UserName, Password, Domain);
        }

        public string Domain { get; private set; }

        public static NtlmAuthorization FromConfigXml(XElement element, Uri serverUri)
        {
            var auth = new NtlmAuthorization();
            auth.ReadConfig(element, serverUri);
            auth.Domain = element.GetAttributeValue("Domain");
            return auth;
        }

        public XElement ToConfigXml()
        {
            var element = new XElement("Ntlm", 
                            new XAttribute("UserName", UserName),
                            new XAttribute("Domain", Domain));
            if (SavePassword)
            {
                element.Add(new XAttribute("Password", Password));
            }
            return element;
        }
    }
}