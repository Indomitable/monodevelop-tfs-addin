using System;
using System.Net;
using System.Xml.Linq;

namespace MonoDevelop.VersionControl.TFS.Core.ServerAuthorization
{
    class NoAuthorization : IServerAuthorization
    {
        public void Authorize(HttpWebRequest request)
        {
            //Pass request with no modifications
        }

        public void Authorize(WebClient client)
        {
            //Pass request with no modifications
        }

        public XElement ToConfigXml()
        {
            return new XElement("None");
        }
    }
}
