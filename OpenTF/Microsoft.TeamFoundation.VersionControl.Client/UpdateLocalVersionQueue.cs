//
// Microsoft.TeamFoundation.VersionControl.Client.UpdateLocalVersionQueue
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
	internal class Update
	{
		private int itemId;
		private string targetLocalItem;
		private int localVersion;

		public Update(int itemId, string targetLocalItem, int localVersion)
		{
			this.itemId = itemId;
			this.targetLocalItem = targetLocalItem;
			this.localVersion = localVersion;
		}

		public int ItemId 
		{
			get { return itemId; }
		}

		public string TargetLocalItem
		{
			get { return targetLocalItem; }
		}

		public int LocalVersion
		{
			get { return localVersion; }
		}

		public void ToXml(XmlWriter writer, string element)
		{
			writer.WriteStartElement("LocalVersionUpdate");

			writer.WriteAttributeString("itemid", Convert.ToString(itemId));
			if (!String.IsNullOrEmpty(targetLocalItem))
				writer.WriteAttributeString("tlocal", TfsPath.FromPlatformPath(targetLocalItem));
			writer.WriteAttributeString("lver", Convert.ToString(localVersion));

			writer.WriteEndElement();
		}
	}

	public sealed class UpdateLocalVersionQueue
	{
		private List<Update> updates;
		private Workspace workspace;

		public UpdateLocalVersionQueue(Workspace workspace)
		{
			this.workspace = workspace;
			updates = new List<Update>();
		}

		public int Count 
		{ 
			get { return updates.Count; }
		}

		public void Flush()
		{
			if (updates.Count > 0)
				workspace.Repository.UpdateLocalVersion(this);
			updates.Clear();
		}

		public void QueueUpdate(int itemId, string targetLocalItem, int localVersion)
		{
			updates.Add(new Update(itemId, targetLocalItem, localVersion));
		}

		internal void ToXml(XmlWriter writer, string element)
		{
			writer.WriteElementString("workspaceName", workspace.Name);
			writer.WriteElementString("ownerName", workspace.OwnerName);

			writer.WriteStartElement("updates");
			foreach (Update update in updates)
				{
					update.ToXml(writer, "LocalVersionUpdate");
				}

			writer.WriteEndElement();
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("UpdateLocalVersionQueue instance ");
			sb.Append(GetHashCode());

			return sb.ToString();
		}
	}
}