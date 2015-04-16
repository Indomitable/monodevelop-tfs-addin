// TeamFoundationServer.cs
// 
// Author:
//       Ventsislav Mladenov
// 
// The MIT License (MIT)
// 
// Copyright (c) 2013-2015 Ventsislav Mladenov
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using MonoDevelop.VersionControl.TFS.Core.ServerAuthorization;
using MonoDevelop.VersionControl.TFS.Core.Services;
using MonoDevelop.VersionControl.TFS.GUI.Server;
using MonoDevelop.VersionControl.TFS.Helpers;

namespace MonoDevelop.VersionControl.TFS.Core.Structure
{
    sealed class TeamFoundationServer: IEquatable<TeamFoundationServer>, IComparable<TeamFoundationServer>
    {
        TeamFoundationServer()
        {
            ProjectCollections = new List<ProjectCollection>();
        }

        public void LoadStructure()
        {
            var projectCollectionsService = new ProjectCollectionService(Uri) {Server = this};
            var projectCollections = projectCollectionsService.GetProjectCollections(this);
            this.ProjectCollections.Clear();
            foreach (var projectCollection in projectCollections)
            {
                projectCollection.Projects.Clear();
                projectCollection.LoadProjects();
                this.ProjectCollections.Add(projectCollection);
            }
        }

        public string Name { get; private set; }

        public Uri Uri { get; private set; }

        public string UserName { get; set; }

        public IServerAuthorization Authorization { get; private set; }

        public List<ProjectCollection> ProjectCollections { get; set; }

        public XElement ToConfigXml()
        {
            var element = new XElement("Server",
                                new XAttribute("Name", this.Name),
                                new XAttribute("Uri", this.Uri),
                                new XAttribute("UserName", this.UserName));
            element.Add(new XElement("Auth", Authorization.ToConfigXml()));
            element.Add(new XElement("ProjectCollections", this.ProjectCollections.Select(pc => pc.ToConfigXml())));
            return element;
        }

        public static TeamFoundationServer FromConfigXml(XElement element)
        {
            if (!string.Equals(element.Name.LocalName, "Server", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Invalid xml element");

            var server = new TeamFoundationServer();
            server.Name = element.GetAttributeValue("Name");
            server.Uri = element.GetUriAttribute("Uri");
            server.UserName = element.GetAttributeValue("UserName");

            var authElement = (XElement)element.Element("Auth").FirstNode;
            server.Authorization = ServerAuthorizationFactory.GetServerAuthorization(authElement, server.Uri);

            server.ProjectCollections.AddRange(element.GetDescendants("ProjectCollection").Select(pc => ProjectCollection.FromConfigXml(pc, server)));
            return server;
        }

        public static TeamFoundationServer FromAddServerDialog(AddServerResult addServerResult,
            IServerAuthorization authorization)
        {
            var server = new TeamFoundationServer
            {
                Uri = addServerResult.Url,
                Name = addServerResult.Name,
                UserName = addServerResult.UserName,
                Authorization = authorization
            };
            return server;
        }

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
            var cast = obj as TeamFoundationServer;
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

