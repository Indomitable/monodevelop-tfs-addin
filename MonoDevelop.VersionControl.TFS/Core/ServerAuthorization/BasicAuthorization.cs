using System;
using System.Net;
using System.Text;
using System.Xml.Linq;
using MonoDevelop.VersionControl.TFS.Core.ServerAuthorization.Config;

namespace MonoDevelop.VersionControl.TFS.Core.ServerAuthorization
{
    sealed class BasicAuthorization : UserPasswordAuthorization, IServerAuthorization
    {
        private BasicAuthorization()
        {
            
        }

        public void Authorize(HttpWebRequest request)
        {
            request.Headers.Add(HttpRequestHeader.Authorization, this.AuthorizationHeader);
        }

        public void Authorize(WebClient client)
        {
            client.Headers.Add(HttpRequestHeader.Authorization, this.AuthorizationHeader);
        }

        public static BasicAuthorization FromConfigXml(XElement element, Uri serverUri)
        {
            var auth = new BasicAuthorization();
            auth.ReadConfig(element, serverUri);
            return auth;
        }

        public XElement ToConfigXml()
        {
            var element = new XElement("Basic",
                            new XAttribute("UserName", UserName));
            if (ClearSavePassword)
            {
                element.Add(new XAttribute("Password", Password));
            }
            return element;
        }

        private string AuthorizationHeader
        {
            get
            {
                var credentialBuffer = Encoding.UTF8.GetBytes(this.UserName + ":" + this.Password);
                return "Basic " + Convert.ToBase64String(credentialBuffer);
            }
        }

        public static IServerAuthorization FromConfigWidget(IUserPasswordAuthorizationConfig serverAuthorizationConfig)
        {
            var auth = new BasicAuthorization();
            auth.ReadConfig(serverAuthorizationConfig);
            return auth;
        }
    }
}