//
// ProjectCollectionConfig.cs
//
// Author:
//       Ventsislav Mladenov <ventsislav.mladenov@gmail.com>
//
// Copyright (c) 2015 Ventsislav Mladenov License MIT/X11
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
using System.Collections.Generic;
using System.Linq;

namespace MonoDevelop.VersionControl.TFS.Configuration
{
    public class ProjectCollectionConfig : IEquatable<ProjectCollectionConfig>, IComparable<ProjectCollectionConfig>
    {
        public ProjectCollectionConfig()
        {
            Projects = new List<ProjectConfig>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string LocationServicePath { get; set; }

        public List<ProjectConfig> Projects { get; set; }
        public ServerConfig Server { get; set; }

        public XElement ToConfigXml()
        {
            var element = new XElement("ProjectCollection", 
                                new XAttribute("Id", this.Id), 
                                new XAttribute("Name", this.Name),
                                new XAttribute("LocationServicePath", this.LocationServicePath));

            element.Add(Projects.Select(p => p.ToConfigXml()));
            return element;
        }

        public static ProjectCollectionConfig FromConfigXml(XElement element)
        {
            if (!string.Equals(element.Name.LocalName, "ProjectCollection", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Invalid xml element");

            var projectCollection = new ProjectCollectionConfig();
            projectCollection.Id = Guid.Parse(element.Attribute("Id").Value);
            projectCollection.Name = element.Attribute("Name").Value;
            projectCollection.LocationServicePath = element.Attribute("LocationServicePath").Value;

            projectCollection.Projects.AddRange(element.Elements("Project").Select(ProjectConfig.FromConfigXml));
            projectCollection.Projects.ForEach(p => p.Collection = projectCollection);

            return projectCollection;
        }

        public ProjectCollectionConfig Copy()
        {
            var collectionConfig = new ProjectCollectionConfig();
            collectionConfig.Id = this.Id;
            collectionConfig.Name = this.Name;
            collectionConfig.LocationServicePath = this.LocationServicePath;
            return collectionConfig;
        }

        #region Equal

        #region IComparable<ProjectCollection> Members

        public int CompareTo(ProjectCollectionConfig other)
        {
            return Id.CompareTo(other.Id);
        }

        #endregion

        #region IEquatable<ProjectCollection> Members

        public bool Equals(ProjectCollectionConfig other)
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
            ProjectCollectionConfig cast = obj as ProjectCollectionConfig;
            if (cast == null)
                return false;
            return Equals(cast);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(ProjectCollectionConfig left, ProjectCollectionConfig right)
        {
            return ReferenceEquals(null, left) ? ReferenceEquals(null, right) : left.Equals(right);
        }

        public static bool operator !=(ProjectCollectionConfig left, ProjectCollectionConfig right)
        {
            return !(left == right);
        }

        #endregion Equal
    }
}

