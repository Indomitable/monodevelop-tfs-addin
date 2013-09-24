//
// Microsoft.TeamFoundation.VersionControl.Client.LabelItemSpec
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
	public sealed class LabelItemSpec 
	{
		private ItemSpec itemSpec;
		private VersionSpec version;
		private bool exclude;

		public LabelItemSpec (ItemSpec itemSpec, VersionSpec version, bool exclude)
		{
			this.itemSpec = itemSpec;
			this.version = version;
			this.exclude = exclude;
		}

		internal void ToXml(XmlWriter writer, string element)
		{
			writer.WriteStartElement(element);
			writer.WriteAttributeString("ex", exclude.ToString().ToLower());
			ItemSpec.ToXml(writer, "ItemSpec");
			Version.ToXml(writer, "Version");
			writer.WriteEndElement();
		}

		public bool Exclude
		{
			get { return exclude; }
			set { exclude = value; }
		}
		
		public ItemSpec ItemSpec
		{
			get { return itemSpec; }
			set { itemSpec = value; }
		}

		public VersionSpec Version
		{
			get { return version; }
			set { version = value; }
		}
	}
}