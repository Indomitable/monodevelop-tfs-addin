//
// Microsoft.TeamFoundation.VersionControl.Client.ItemSecurity
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
using System.Text;
using System.Xml;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
	public class ItemSecurity 
	{
		private string serverItem;
		private bool writable;
		private AccessEntry[] entries;

		internal static ItemSecurity FromXml(Repository repository, XmlReader reader)
		{
			ItemSecurity itemSecurity = new ItemSecurity();
			string elementName = reader.Name;

			itemSecurity.serverItem = reader.GetAttribute("item");
			itemSecurity.writable = Convert.ToBoolean(reader.GetAttribute("writable"));

 			List<AccessEntry> entries = new List<AccessEntry>();
			while (reader.Read())
				{
					if (reader.NodeType == XmlNodeType.EndElement && reader.Name == elementName)
						break;

					if (reader.NodeType == XmlNodeType.Element)
						{
							switch (reader.Name)
								{
								case "AccessEntry":
									entries.Add(AccessEntry.FromXml(repository, reader));
									break;
								}
						}
				}

			itemSecurity.entries = entries.ToArray();
			return itemSecurity;
		}

		public string ServerItem
		{
			get { return serverItem; }
		}

		public bool Writable
		{
			get { return writable; }
		}

		public AccessEntry[] Entries
		{
			get { return entries; }
		}
	}
}
