//
// Microsoft.TeamFoundation.VersionControl.Client.WorkspaceVersionSpec
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
using System.Text;
using System.Xml;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
	public class WorkspaceVersionSpec : VersionSpec
	{
		private string name;
		private string ownerName;

		public WorkspaceVersionSpec(string name, string ownerName)
		{
			this.name = name;
			this.ownerName = ownerName;
		}

		public WorkspaceVersionSpec(Workspace workspace)
		{
			this.name = workspace.Name;
			this.ownerName = workspace.OwnerName;
		}

		public WorkspaceVersionSpec(WorkspaceInfo workspaceInfo)
		{
			this.name = workspaceInfo.Name;
			this.ownerName = workspaceInfo.OwnerName;
		}

		internal override void ToXml(XmlWriter writer, string element)
		{
			writer.WriteStartElement(element);
			writer.WriteAttributeString("xsi:type", "WorkspaceVersionSpec");
			writer.WriteAttributeString("name", Name);
			writer.WriteAttributeString("owner", OwnerName);
			writer.WriteEndElement();
		}

		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		public string OwnerName
		{
			get { return ownerName; }
		}

		public override string DisplayString
		{
			get { return String.Format("W{0};{1}", Name, OwnerName); }
		}
	}
}
