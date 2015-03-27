//
// ProjectConfig.cs
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
using MonoDevelop.VersionControl.TFS.Core;

namespace MonoDevelop.VersionControl.TFS.Configuration
{
    public class ProjectConfig : IEquatable<ProjectConfig>, IComparable<ProjectConfig>
    {
        public string Name { get; set; }
        public ProjectState State { get; set; }
        public Uri Url { get; set; }

        public Guid Id
        {
            get
            {
                var url = Url.OriginalString;
                if (!string.IsNullOrEmpty(url) && url.Length > 36)
                    return Guid.Parse(url.Remove(0, 36)); //Remove vstfs:///Classification/TeamProject/
                return Guid.Empty;
            }
        }

        public ProjectCollectionConfig Collection { get; set; }

        public XElement ToConfigXml()
        {
            return new XElement("Project", 
                        new XAttribute("Name", this.Name),
                        new XAttribute("Status", this.State),
                        new XAttribute("Uri", this.Url));
        }

        public static ProjectConfig FromServerXml(XElement element)
        {
            var nameSpace = element.Name.Namespace;
            var projectConfig = new ProjectConfig();
            projectConfig.Name = element.Element(nameSpace + "Name").Value;
            projectConfig.Url = new Uri(element.Element(nameSpace + "Uri").Value);
            projectConfig.State = (ProjectState)Enum.Parse(typeof(ProjectState), element.Element(nameSpace + "Status").Value);
            return projectConfig;
        }

        public static ProjectConfig FromConfigXml(XElement element)
        {
            if (!string.Equals(element.Name.LocalName, "Project", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Invalid xml element");

            var projectConfig = new ProjectConfig();
            projectConfig.Name = element.Attribute("Name").Value;
            projectConfig.Url = new Uri(element.Attribute("Uri").Value);
            projectConfig.State = (ProjectState)Enum.Parse(typeof(ProjectState), element.Attribute("Status").Value);
            return projectConfig;
        }

        #region Equal

        #region IComparable<ProjectCollection> Members

        public int CompareTo(ProjectConfig other)
        {
            return Id.CompareTo(other.Id);
        }

        #endregion

        #region IEquatable<ProjectCollection> Members

        public bool Equals(ProjectConfig other)
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
            ProjectConfig cast = obj as ProjectConfig;
            if (cast == null)
                return false;
            return Equals(cast);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(ProjectConfig left, ProjectConfig right)
        {
            return ReferenceEquals(null, left) ? ReferenceEquals(null, right) : left.Equals(right);
        }

        public static bool operator !=(ProjectConfig left, ProjectConfig right)
        {
            return !(left == right);
        }

        #endregion Equal
    }
}

