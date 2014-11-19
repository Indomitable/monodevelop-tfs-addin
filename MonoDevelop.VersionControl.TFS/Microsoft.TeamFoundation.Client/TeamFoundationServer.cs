//
// Microsoft.TeamFoundation.Client.TeamFoundationServer
//
// Authors:
//	Joel Reed (joelwreed@gmail.com)
//  Ventsislav Mladenov (ventsislav.mladenov@gmail.com)
//
// Copyright (C) 2013 Joel Reed, Ventsislav Mladenov
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Net;
using System.Xml.Linq;
using System.Linq;

namespace Microsoft.TeamFoundation.Client
{
    public class TeamFoundationServer : BaseTeamFoundationServer, INetworkServer
    {
        public TeamFoundationServer(Uri uri, string name, string domain, string userName, string password, bool isPasswordSavedInXml)
            : base(uri, name, userName, password, isPasswordSavedInXml)
        {
            this.Domain = domain;
            Credentials = new NetworkCredential(UserName, Password, Domain);
        }

        public static TeamFoundationServer FromLocalXml(XElement element, string password, bool isPasswordSavedInXml)
        {
            try
            {
                var server = new TeamFoundationServer(new Uri(element.Attribute("Url").Value),
                                                      element.Attribute("Name").Value,
                                                      element.Attribute("Domain").Value,
                                                      element.Attribute("UserName").Value,
                                                      password,
                                                      isPasswordSavedInXml);
                server.ProjectCollections = element.Elements("ProjectCollection").Select(x => ProjectCollection.FromLocalXml(server, x)).ToList();
                return server;
            }
            catch
            {
                return null;
            }
        }

        public override XElement ToLocalXml()
        {
            var serverElement = new XElement("Server", 
                                    new XAttribute("Type", (int)ServerType.TFS),
                                    new XAttribute("Name", this.Name),
                                    new XAttribute("Url", this.Uri),
                                    new XAttribute("Domain", this.Credentials.Domain),
                                    new XAttribute("UserName", this.Credentials.UserName),
                                    this.ProjectCollections.Select(p => p.ToLocalXml()));
            if (IsPasswordSavedInXml)
                serverElement.Add(new XAttribute("Password", this.Credentials.Password));
            return serverElement;
        }

        string Domain { get; set; }

        public NetworkCredential Credentials { get; private set; }
    }
}

