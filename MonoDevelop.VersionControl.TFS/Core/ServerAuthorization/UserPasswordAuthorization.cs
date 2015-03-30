using System;
using System.Net;
using System.Xml.Linq;
using MonoDevelop.VersionControl.TFS.Helpers;

namespace MonoDevelop.VersionControl.TFS.Core.ServerAuthorization
{
    class UserPasswordAuthorization
    {
        protected UserPasswordAuthorization()
        {
            
        }

        public string UserName { get; protected set; }
        public string Password { get; private set; }
        public bool SavePassword { get; private set; }

        protected void ReadConfig(XElement element, Uri serverUri)
        {
            this.UserName = element.GetAttributeValue("UserName");
            if (element.Attribute("Password") != null)
            {
                this.Password = element.Attribute("Password").Value;
                this.SavePassword = true;
            }
            else
            {
                this.Password = CredentialsManager.GetPassword(serverUri);
                this.SavePassword = false;
            }
        }
    }
}
