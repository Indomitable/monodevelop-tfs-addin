//
// Microsoft.TeamFoundation.VersionControl.Client.GetRequest
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
	public class GetRequest {
		
		private ItemSpec itemSpec;
		private VersionSpec versionSpec;

		public GetRequest(string item, RecursionType recursionType, VersionSpec versionSpec)
		{
			this.itemSpec = new ItemSpec(item, recursionType);
			this.versionSpec = versionSpec;
		}

		public GetRequest(ItemSpec itemSpec, VersionSpec versionSpec)
		{
			this.itemSpec = itemSpec;
			this.versionSpec = versionSpec;
		}

		public GetRequest (ItemSpec itemSpec, int changesetId)
		{
			this.itemSpec = itemSpec;
			this.versionSpec = new ChangesetVersionSpec(changesetId);
		}

		internal GetRequest(VersionSpec versionSpec)
		{
			this.versionSpec = versionSpec;
		}

		public ItemSpec ItemSpec 
		{
			get { return itemSpec; }
		}
		
		public VersionSpec VersionSpec
		{
			get { return versionSpec; }
		}

		internal void ToXml(XmlWriter writer, string element)
		{
			writer.WriteStartElement("GetRequest");
			if (itemSpec != null) itemSpec.ToXml(writer, "ItemSpec");
			versionSpec.ToXml(writer, "VersionSpec");
			writer.WriteEndElement();
		}

	}
}
