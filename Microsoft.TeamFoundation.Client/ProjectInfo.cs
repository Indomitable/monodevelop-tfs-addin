//
// Microsoft.TeamFoundation.Server.ProjectInfo
//
// Authors:
//	Joel Reed (joelwreed@gmail.com)
//
// Copyright (C) 2007 Joel Reed
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
using System.Xml.Linq;

namespace Microsoft.TeamFoundation.Client
{
    public sealed class ProjectInfo: IEquatable<ProjectInfo>, IComparable<ProjectInfo>
    {
        public string Name { get; private set; }

        public ProjectState Status { get; private set; }

        public string Uri { get; private set; }

        public string Guid
        {
            get
            {
                if (!string.IsNullOrEmpty(Uri) && Uri.Length > 36)
                    return Uri.Remove(0, 36); //Remove vstfs:///Classification/TeamProject/
                return string.Empty;
            }
        }

        public ProjectCollection Collection { get; private set; }

        public static ProjectInfo FromXml(ProjectCollection collection, XElement element)
        {
            var nameSpace = element.Name.Namespace;
            ProjectInfo projectInfo = new ProjectInfo();
            projectInfo.Name = element.Element(nameSpace + "Name").Value;
            projectInfo.Uri = element.Element(nameSpace + "Uri").Value;
            projectInfo.Status = (ProjectState)Enum.Parse(typeof(ProjectState), element.Element(nameSpace + "Status").Value);
            projectInfo.Collection = collection;
            return projectInfo;
        }

        public static ProjectInfo FromLocalXml(ProjectCollection collection, XElement element)
        {
            ProjectInfo projectInfo = new ProjectInfo();
            projectInfo.Name = element.Attribute("Name").Value;
            projectInfo.Uri = element.Attribute("Uri").Value;
            projectInfo.Status = (ProjectState)Enum.Parse(typeof(ProjectState), element.Attribute("Status").Value);
            projectInfo.Collection = collection;
            return projectInfo;
        }

        public XElement ToLocalXml()
        {
            return new XElement("Project", 
                new XAttribute("Name", this.Name),
                new XAttribute("Status", this.Status),
                new XAttribute("Uri", this.Uri));
        }

        #region Equal

        #region IComparable<ProjectInfo> Members

        public int CompareTo(ProjectInfo other)
        {
            return Name.CompareTo(other.Name);
        }

        #endregion

        #region IEquatable<ProjectInfo> Members

        public bool Equals(ProjectInfo other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return other.Name == Name;
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            ProjectInfo cast = obj as ProjectInfo;
            if (cast == null)
                return false;
            return Equals(cast);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public static bool operator ==(ProjectInfo left, ProjectInfo right)
        {
            return ReferenceEquals(null, left) ? ReferenceEquals(null, right) : left.Equals(right);
        }

        public static bool operator !=(ProjectInfo left, ProjectInfo right)
        {
            return !(left == right);
        }

        #endregion Equal

    }
}

