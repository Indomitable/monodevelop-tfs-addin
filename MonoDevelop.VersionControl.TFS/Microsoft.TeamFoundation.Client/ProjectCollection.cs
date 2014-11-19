//
// TeamFoundationProjectCollection.cs
//
// Author:
//       Ventsislav Mladenov <vmladenov.mladenov@gmail.com>
//
// Copyright (c) 2013 Ventsislav Mladenov
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
using System.Xml.Linq;
using System.Xml.XPath;
using System.Linq;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Client.Services;

namespace Microsoft.TeamFoundation.Client
{
    public class ProjectCollection : IEquatable<ProjectCollection>, IComparable<ProjectCollection>
    {
        private LocationService locationService;

        private ProjectCollection()
        {
            
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public Uri Url { get; set; }

        public Uri LocationServiceUrl { get; set; }

        public static ProjectCollection FromServerXml(BaseTeamFoundationServer server, XElement element)
        {
            ProjectCollection collection = new ProjectCollection();
            collection.Server = server;
            collection.Name = element.Attribute("DisplayName").Value;
            collection.Id = element.Attribute("Identifier").Value;
            var locationServiceElement = element.XPathSelectElement("./msg:CatalogServiceReferences/msg:CatalogServiceReference/msg:ServiceDefinition[@serviceType='LocationService']",
                                             TeamFoundationServerServiceMessage.NsResolver);
            string locationService = locationServiceElement.Attribute("relativePath").Value;
            collection.Url = UrlHelper.AddPathToUri(server.Uri, UrlHelper.GetFirstItemOfPath(locationService));
            collection.LocationServiceUrl = UrlHelper.AddPathToUri(server.Uri, locationService);
            collection.locationService = new LocationService(collection);
            return collection;
        }

        public static ProjectCollection FromLocalXml(BaseTeamFoundationServer server, XElement element)
        {
            ProjectCollection collection = new ProjectCollection();
            collection.Server = server;
            collection.Id = element.Attribute("Id").Value;
            collection.Name = element.Attribute("Name").Value;
            collection.Url = new Uri(element.Attribute("Url").Value);
            collection.LocationServiceUrl = new Uri(element.Attribute("LocationServiceUrl").Value);
            collection.locationService = new LocationService(collection);
            collection.Projects = element.Elements("Project").Select(x => ProjectInfo.FromLocalXml(collection, x)).ToList();
            return collection;
        }

        public XElement ToLocalXml()
        {
            return new XElement("ProjectCollection", 
                new XAttribute("Id", this.Id), 
                new XAttribute("Url", this.Url),
                new XAttribute("Name", this.Name),
                new XAttribute("LocationServiceUrl", this.LocationServiceUrl),
                this.Projects.Select(x => x.ToLocalXml()));
        }

        public void LoadProjects()
        {
            /*s.agostini (2014-01-14) Catch "401 unauthorized" exception, returning an empty list*/
            try
            {
            var commonStructureService = this.GetService<CommonStructureService>();
            this.Projects = commonStructureService.ListAllProjects();
            this.Projects.Sort();
            }
            catch
            {
                this.Projects = new List<ProjectInfo>();
            }
            /*s.agostini end*/
        }

        public void LoadProjects(List<string> names)
        {
            var commonStructureService = this.GetService<CommonStructureService>();
            this.Projects = commonStructureService.ListAllProjects().Where(pi => names.Any(n => string.Equals(pi.Name, n))).ToList();
            this.Projects.Sort();
        }

        public List<ProjectInfo> Projects { get; set; }

        public BaseTeamFoundationServer Server { get; set; }

        public T GetService<T>()
            where T : TFSCollectionService
        {
            return locationService.LoadService<T>();
        }

        #region Equal

        #region IComparable<ProjectCollection> Members

        public int CompareTo(ProjectCollection other)
        {
            return string.Compare(Id, other.Id, StringComparison.Ordinal);
        }

        #endregion

        #region IEquatable<ProjectCollection> Members

        public bool Equals(ProjectCollection other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return other.Id == Id;
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            ProjectCollection cast = obj as ProjectCollection;
            if (cast == null)
                return false;
            return Equals(cast);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(ProjectCollection left, ProjectCollection right)
        {
            return ReferenceEquals(null, left) ? ReferenceEquals(null, right) : left.Equals(right);
        }

        public static bool operator !=(ProjectCollection left, ProjectCollection right)
        {
            return !(left == right);
        }

        #endregion Equal

    }
}
