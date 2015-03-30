using System;
using System.Net;
using System.Xml.Linq;

namespace MonoDevelop.VersionControl.TFS.Core.ServerAuthorization
{
    interface IServerAuthorization
    {
        void Authorize(HttpWebRequest request);
        void Authorize(WebClient client);
        XElement ToConfigXml();
    }
}
