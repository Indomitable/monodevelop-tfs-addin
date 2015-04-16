// ProjectInfo.cs
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
using System.Xml.Linq;
using MonoDevelop.VersionControl.TFS.Helpers;

namespace MonoDevelop.VersionControl.TFS.Core.Structure
{
    sealed class ProjectInfo: IEquatable<ProjectInfo>, IComparable<ProjectInfo>
    {
        private ProjectInfo(ProjectCollection collection)
        {
            this.Collection = collection;
        }

        public string Name { get; private set;  }

        public ProjectState State { get; private set; }

        public Uri Uri { get; private set; }

        public Guid Id { get; private set; }

        public ProjectCollection Collection { get; private set; }

        #region Serialization

        public XElement ToConfigXml()
        {
            return new XElement("Project",
                        new XAttribute("Name", this.Name),
                        new XAttribute("Status", this.State),
                        new XAttribute("Uri", this.Uri));
        }

        public static ProjectInfo FromServerXml(XElement element, ProjectCollection collection)
        {
            var projectInfo = new ProjectInfo(collection);
            projectInfo.Name = element.GetElement("Name").Value;
            projectInfo.Uri = new Uri(element.GetElement("Uri").Value);
            projectInfo.Id = Guid.Parse(projectInfo.Uri.OriginalString.Remove(0, 36));
            projectInfo.State = (ProjectState)Enum.Parse(typeof(ProjectState), element.GetElement("Status").Value);
            return projectInfo;
        }

        public static ProjectInfo FromConfigXml(XElement element, ProjectCollection collection)
        {
            if (!string.Equals(element.Name.LocalName, "Project", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Invalid xml element");

            var projectInfo = new ProjectInfo(collection);
            projectInfo.Name = element.Attribute("Name").Value;
            projectInfo.Uri = new Uri(element.Attribute("Uri").Value);
            projectInfo.Id = Guid.Parse(projectInfo.Uri.OriginalString.Remove(0, 36));
            projectInfo.State = (ProjectState)Enum.Parse(typeof(ProjectState), element.Attribute("Status").Value);
            return projectInfo;
        }


        #endregion

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

