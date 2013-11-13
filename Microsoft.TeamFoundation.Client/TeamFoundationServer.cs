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
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using Microsoft.TeamFoundation.Client.Services;

namespace Microsoft.TeamFoundation.Client
{
    public sealed class TeamFoundationServer : IEquatable<TeamFoundationServer>, IComparable<TeamFoundationServer>
    {
        private TeamFoundationServer()
        {
            
        }

        public TeamFoundationServer(Uri uri, string name, string domain, string userName, string password)
        {
            this.Uri = uri;
            this.Name = name;
            this.Domain = domain;
            this.UserName = userName;
            IsPasswordSavedSecurely = password == null;
            if (!IsPasswordSavedSecurely)
            {
                Password = password;
            }
        }

        public void LoadProjectConnections()
        {
            var projectCollectionsService = new ProjectCollectionService(this);
            this.ProjectCollections = projectCollectionsService.GetProjectCollections();
        }

        public static TeamFoundationServer FromLocalXml(XElement element, string password)
        {
            try
            {
                TeamFoundationServer server = new TeamFoundationServer();
                server.Name = element.Attribute("Name").Value;
                server.Uri = new Uri(element.Attribute("Url").Value);
                server.IsPasswordSavedSecurely = password != null;
                if (server.IsPasswordSavedSecurely)
                {
                    server.Password = password;
                }
                else
                {
                    if (element.Attribute("Password") != null)
                    {
                        server.Password = element.Attribute("Password").Value;
                    }
                    else
                    {
                        throw new Exception("Could not load password!");
                    }
                }
                server.Domain = element.Attribute("Domain").Value;
                server.UserName = element.Attribute("UserName").Value;
                server.ProjectCollections = element.Elements("ProjectCollection").Select(x => ProjectCollection.FromLocalXml(server, x)).ToList();
                return server;
            }
            catch
            {
                return null;
            }
        }

        public XElement ToLocalXml()
        {
            var serverElement = new XElement("Server", 
                                    new XAttribute("Name", this.Name),
                                    new XAttribute("Url", this.Uri),
                                    new XAttribute("Domain", this.Credentials.Domain),
                                    new XAttribute("UserName", this.Credentials.UserName),
                                    this.ProjectCollections.Select(p => p.ToLocalXml()));
            if (!IsPasswordSavedSecurely)
                serverElement.Add(new XAttribute("Password", this.Credentials.Password));
            return serverElement;
        }

        public NetworkCredential Credentials
        { 
            get
            {
                return new NetworkCredential(UserName, Password, Domain);
            }
        }

        public bool IsPasswordSavedSecurely { get; set; }

        public string Domain { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public List<ProjectCollection> ProjectCollections { get; set; }

        public string Name { get; private set; }

        public Uri Uri { get; private set; }

        #region Equal

        #region IComparable<TeamFoundationServer> Members

        public int CompareTo(TeamFoundationServer other)
        {
            return string.Compare(Uri.ToString(), other.Uri.ToString(), StringComparison.Ordinal);
        }

        #endregion

        #region IEquatable<TeamFoundationServer> Members

        public bool Equals(TeamFoundationServer other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return other.Uri == Uri;
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            TeamFoundationServer cast = obj as TeamFoundationServer;
            if (cast == null)
                return false;
            return Equals(cast);
        }

        public override int GetHashCode()
        {
            return Uri.GetHashCode();
        }

        public static bool operator ==(TeamFoundationServer left, TeamFoundationServer right)
        {
            return ReferenceEquals(null, left) ? ReferenceEquals(null, right) : left.Equals(right);
        }

        public static bool operator !=(TeamFoundationServer left, TeamFoundationServer right)
        {
            return !(left == right);
        }

        #endregion Equal

    }
}

