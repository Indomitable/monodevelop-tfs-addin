//
// Microsoft.TeamFoundation.VersionControl.Client.WorkspaceInfo
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
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Xml;
using System.Web.Services;
using Microsoft.TeamFoundation.Client;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
//    public sealed class WorkspaceInfo
//    {
//        private string comment;
//        private string computer;
//        private string displayName;
//        private string name;
//        private string ownerName;
//        private string[] mappedPaths;
//        private InternalServerInfo server;
//
//        internal WorkspaceInfo(InternalServerInfo server)
//        {
//            this.server = server;
//        }
//
//        internal WorkspaceInfo(InternalServerInfo server, Workspace workspace)
//        {
//            this.server = server;
//            comment = workspace.Comment;			
//            computer = workspace.Computer;
//            name = workspace.Name;
//            ownerName = workspace.OwnerName;
//			
//            List<string> paths = new List<string>();
//            foreach (WorkingFolder folder in workspace.Folders)
//            {
//                paths.Add(folder.LocalItem);
//            }
//
//            mappedPaths = paths.ToArray(); 
//        }
//
//        internal XmlElement ToXml(XmlDocument doc)
//        {
//            XmlElement xmlElement = doc.CreateElement("WorkspaceInfo");
//
//            xmlElement.SetAttributeNode("name", "").Value = Name;
//            xmlElement.SetAttributeNode("ownerName", "").Value = OwnerName;
//            xmlElement.SetAttributeNode("computer", "").Value = Computer;
//            xmlElement.SetAttributeNode("comment", "").Value = Comment;
//            xmlElement.SetAttributeNode("LastSavedCheckinTimeStamp", "").Value = DateTime.Now.ToString();
//
//            XmlElement mpaths = doc.CreateElement("MappedPaths");
//            xmlElement.AppendChild(mpaths);
//
//            foreach (string path in mappedPaths)
//            {
//                XmlElement pathElement = doc.CreateElement("MappedPath");
//                mpaths.AppendChild(pathElement);
//                pathElement.SetAttributeNode("path", "").Value = path;
//            }
//
//            return xmlElement;
//        }
//
//        internal static WorkspaceInfo FromXml(InternalServerInfo server, XmlReader reader)
//        {
//            string elementName = reader.Name;
//            WorkspaceInfo info = new WorkspaceInfo(server);
//
//            info.name = reader.GetAttribute("name");
//            info.ownerName = reader.GetAttribute("ownerName");
//            info.computer = reader.GetAttribute("computer");
//            info.comment = reader.GetAttribute("comment");
//
//            List<string> mappedPaths = new List<string>();
//            while (reader.Read())
//            {
//                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == elementName)
//                    break;
//
//                if (reader.NodeType == XmlNodeType.Element && reader.Name == "MappedPath")
//                    mappedPaths.Add(reader.GetAttribute("path"));
//            }
//
//            info.mappedPaths = mappedPaths.ToArray();
//            return info;
//        }
//
//        public Workspace GetWorkspace(ProjectCollection collection)
//        {
//            VersionControlServer vcs = collection.GetService<VersionControlServer>(new VersionControlServiceResolver());
//            return vcs.GetWorkspace(Name, OwnerName);
//        }
//
//        internal void SaveAsXml(XmlNode parent)
//        {	
//        }
//
//        public override string ToString()
//        {
//            StringBuilder sb = new StringBuilder();
//
//            sb.Append("WorkspaceInfo instance ");
//            sb.Append(GetHashCode());
//
//            sb.Append("\n	 Name: ");
//            sb.Append(Name);
//
//            sb.Append("\n	 OwnerName: ");
//            sb.Append(OwnerName);
//
//            sb.Append("\n	 Comment: ");
//            sb.Append(Comment);
//
//            sb.Append("\n	 Computer: ");
//            sb.Append(Computer);
//
//            sb.Append("\n	 ServerUri: ");
//            sb.Append(ServerUri);
//
//            return sb.ToString();
//        }
//
//        public string Comment
//        {
//            get { return comment; }
//        }
//
//        public string Computer
//        {
//            get { return computer; }
//        }
//
//        public string[] MappedPaths
//        {
//            get { return mappedPaths; }
//            internal set { mappedPaths = value; }
//        }
//
//        public string DisplayName
//        {
//            get { return displayName; }
//        }
//
//        public string Name
//        {
//            get { return name; }
//        }
//
//        public string OwnerName
//        {
//            get { return ownerName; }
//        }
//
//        public Uri ServerUri
//        {
//            get { return server.Uri; }
//        }
//
//        internal InternalServerInfo Server
//        {
//            get { return server; }
//        }
//    }
}
