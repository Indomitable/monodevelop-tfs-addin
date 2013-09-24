//
// Microsoft.TeamFoundation.VersionControl.Client.ItemSpec
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
using Microsoft.TeamFoundation.VersionControl.Common;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
	public class ItemSpec 
	{
		private int deletionId = 0;
		private string item;
		private RecursionType recursionType;

		public ItemSpec(string item, RecursionType recursionType)
		{
			if (String.IsNullOrEmpty(item))
				throw new ArgumentException("Value cannot be null or empty.");

			this.item = item;
			this.recursionType = recursionType;
		}

		public ItemSpec(string item, RecursionType recursionType, int deletionId)
		{
			if (String.IsNullOrEmpty(item))
				throw new ArgumentException("Value cannot be null or empty.");

			this.item = item;
			this.recursionType = recursionType;
			this.deletionId = deletionId;
		}

		internal void ToXml(XmlWriter writer, string element)
		{
			if (String.IsNullOrEmpty(item)) return;

			writer.WriteStartElement(element);

			if (this.RecursionType != RecursionType.None)
				writer.WriteAttributeString("recurse", RecursionType.ToString());
			if (this.DeletionId != 0) 
				writer.WriteAttributeString("did", DeletionId.ToString());

			// only convert local path specs from platform paths to tfs paths 
			if (VersionControlPath.IsServerItem(Item)) writer.WriteAttributeString("item", Item);
			else writer.WriteAttributeString("item", TfsPath.FromPlatformPath(Item));

			writer.WriteEndElement();
		}

		public int DeletionId
		{
			get { return deletionId; }
			set { deletionId = value; }
		}

		public string Item
		{
			get { return item; }
			set { item = value; }
		}

		public RecursionType RecursionType
		{
			get { return recursionType; }
			set { recursionType = value; }
		}
	}
}
