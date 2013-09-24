//
// Microsoft.TeamFoundation.VersionControl.Client.WorkingFolder
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
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.TeamFoundation.VersionControl.Common;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
	[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://schemas.microsoft.com/TeamFoundation/2005/06/VersionControl/ClientServices/03")]
	public sealed class WorkingFolder 
	{
		private bool isCloaked;
		private string localItem;
		private string serverItem;
    private WorkingFolderType type = WorkingFolderType.Map;

		public WorkingFolder(string serverItem, string localItem)
		{
			CheckServerPathStartsWithDollarSlash(serverItem);
			this.serverItem = serverItem;
			this.localItem = Path.GetFullPath(localItem);
		}

		internal static WorkingFolder FromXml(Repository repository, XmlReader reader)
		{
			string local = TfsPath.ToPlatformPath(reader.GetAttribute("local"));
			string serverItem = reader.GetAttribute("item");
			return new WorkingFolder(serverItem, local);
		}

		internal void ToXml(XmlWriter writer, string element)
		{
			writer.WriteStartElement("WorkingFolder");
			writer.WriteAttributeString("local", TfsPath.FromPlatformPath(LocalItem));
			writer.WriteAttributeString("item", ServerItem);
			//			writer.WriteAttributeString("type", Type.ToString());
			writer.WriteEndElement();
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("WorkingFolder instance ");
			sb.Append(GetHashCode());

			sb.Append("\n	 LocalItem: ");
			sb.Append(LocalItem);

			sb.Append("\n	 ServerItem: ");
			sb.Append(ServerItem);

			return sb.ToString();
		}

		internal void CheckServerPathStartsWithDollarSlash(string serverItem)
		{
			if (VersionControlPath.IsServerItem(serverItem)) return;
			string msg = String.Format("TF10125: The path '{0}' must start with {1}", serverItem, VersionControlPath.RootFolder);
			throw new InvalidPathException(msg);
		}

		public bool IsCloaked
		{
			get { return isCloaked; }
		}

		public string LocalItem
		{
			get { return localItem; }
		}

		public WorkingFolderType Type
		{
			get { return type; }
		}

		public string ServerItem
		{
			get { return serverItem; }
		}

	}
}
