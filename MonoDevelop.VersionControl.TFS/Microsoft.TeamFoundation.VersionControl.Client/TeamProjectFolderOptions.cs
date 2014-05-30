//
// Microsoft.TeamFoundation.VersionControl.Client.TeamProjectFolderOptions
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
using System.Xml;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
    public class TeamProjectFolderOptions
    {
        private string comment = "";
        private bool exclusiveCheckout = false;
        private string sourceProject;
        private string teamProject;

        public TeamProjectFolderOptions(string teamProject)
        {
            this.teamProject = teamProject;
            this.sourceProject = teamProject;
        }

        public TeamProjectFolderOptions(string teamProject, string sourceProject)
        {
            this.teamProject = teamProject;
            this.sourceProject = sourceProject;
        }

        internal void ToXml(XmlWriter writer, string element)
        {
            writer.WriteStartElement("teamProjectOptions");
            writer.WriteAttributeString("exc", ExclusiveCheckout.ToString().ToLower());
            writer.WriteElementString("TeamProject", TeamProject);
            writer.WriteElementString("SourceProject", SourceProject);
            writer.WriteElementString("Comment", Comment);
            writer.WriteEndElement();
        }

        public string Comment
        {
            get { return comment; }
        }

        public bool ExclusiveCheckout
        {
            get { return exclusiveCheckout; }
        }

        public string SourceProject
        {
            get { return sourceProject; }
        }

        public string TeamProject
        {
            get { return teamProject; }
        }
    }
}

