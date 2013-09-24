//
// Microsoft.TeamFoundation.VersionControl.Client.InternalServerInfo
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

namespace Microsoft.TeamFoundation.VersionControl.Client
{
	internal sealed class InternalServerInfo
	{
		private Uri uri;
		private Guid repositoryGuid;
		public WorkspaceInfo[] Workspaces;

		public InternalServerInfo()
		{
		}

		public InternalServerInfo(string uri, Guid repositoryGuid, Workspace workspace)
		{
			this.uri = new Uri(uri);
			this.repositoryGuid = repositoryGuid;

			List<WorkspaceInfo> infos = new List<WorkspaceInfo>();
			infos.Add(new WorkspaceInfo(this, workspace));
			Workspaces = infos.ToArray();
		}

		public InternalServerInfo(string uri, Guid repositoryGuid, Workspace[] workspaces)
		{
			this.uri = new Uri(uri);
			this.repositoryGuid = repositoryGuid;

 			List<WorkspaceInfo> infos = new List<WorkspaceInfo>();
			foreach (Workspace workspace in workspaces)
				{
					infos.Add(new WorkspaceInfo(this, workspace));
				}

			Workspaces = infos.ToArray();
		}

		public XmlElement ToXml(XmlDocument doc)
		{
			XmlElement xmlElement = doc.CreateElement("ServerInfo");
			xmlElement.SetAttributeNode("uri", "").Value = Uri.ToString();
			//xmlElement.SetAttributeNode("repositoryGuid", "").Value = ServerGuid.ToString();

			foreach (WorkspaceInfo workspace in Workspaces)
				{
					xmlElement.AppendChild(workspace.ToXml(doc));
				}

			return xmlElement;
		}

		public static InternalServerInfo FromXml(XmlReader reader)
		{
			string elementName = reader.Name;
			InternalServerInfo info = new InternalServerInfo();

			info.uri = new Uri(reader.GetAttribute("uri"));
 			List<WorkspaceInfo> workspaces = new List<WorkspaceInfo>();

			while (reader.Read())
				{
					if (reader.NodeType == XmlNodeType.EndElement && reader.Name == elementName)
						break;

					if (reader.NodeType == XmlNodeType.Element && reader.Name == "WorkspaceInfo")
						workspaces.Add(WorkspaceInfo.FromXml(info, reader));
				}

			info.Workspaces = workspaces.ToArray();
			return info;
		}

		public Uri Uri
		{
			get { return uri; }
		}

		public Guid RepositoryGuid
		{
			get { return repositoryGuid; }
		}
	}
}
