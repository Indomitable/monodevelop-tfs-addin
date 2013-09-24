//
// Microsoft.TeamFoundation.VersionControl.Client.ItemSet
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
	public sealed class ItemSet
	{
		private Item[] items;
		private string pattern;
		private string queryPath;

		internal static ItemSet FromXml(Repository repository, XmlReader reader)
		{
			string elementName = reader.Name;
			ItemSet itemSet = new ItemSet();
 			List<Item> items = new List<Item>();

			while (reader.Read())
				{
					if (reader.NodeType == XmlNodeType.EndElement && reader.Name == elementName)
						break;

					if (reader.NodeType == XmlNodeType.Element)
						{
							switch (reader.Name)
								{
								case "Item":
									items.Add(Item.FromXml(repository, reader));
									break;
								case "Pattern":
									itemSet.pattern = reader.ReadElementContentAsString();
									break;
								case "QueryPath":
									itemSet.queryPath = reader.ReadElementContentAsString();
									break;
								}
						}
				}

			items.Sort(Item.GenericComparer);
			itemSet.items = items.ToArray();
			return itemSet;
		}

		public Item[] Items
		{
			get { return items; }
		}

    public string Pattern
		{
			get { return pattern; }
		}

		public string QueryPath
		{
			get { return queryPath; }
		}
	}
}

