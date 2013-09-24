//
// Microsoft.TeamFoundation.VersionControl.Client.BranchRelative
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
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Web.Services;
using Microsoft.TeamFoundation.VersionControl.Common;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
	public sealed class BranchRelative
	{
		private Item branchFromItem;

		public Item BranchFromItem
		{
			get { return branchFromItem; }
		}

		private Item branchToItem;
		public Item BranchToItem
		{
			get { return branchToItem; }
		}

		private bool isRequestedItem;
		public bool IsRequestedItem
		{
			get { return isRequestedItem; }
		}

		private int relativeFromItemId;
		public int RelativeFromItemId
		{
			get { return relativeFromItemId; }
			set { relativeFromItemId = value; }
		}

		private int relativeToItemId;
		public int RelativeToItemId
		{
			get { return relativeToItemId; }
			set { relativeToItemId = value; }
		}

		internal static BranchRelative FromXml(Repository repository, XmlReader reader)
		{
			string elementName = reader.Name;
			BranchRelative branch = new BranchRelative();

			branch.relativeToItemId = Convert.ToInt32(reader.GetAttribute("reltoid"));
			branch.relativeFromItemId = Convert.ToInt32(reader.GetAttribute("relfromid"));
			branch.isRequestedItem = Convert.ToBoolean(reader.GetAttribute("reqstd"));

			while (reader.Read())
				{
					if (reader.NodeType == XmlNodeType.EndElement && reader.Name == elementName)
						break;

					if (reader.NodeType == XmlNodeType.Element)
						{
							switch (reader.Name)
								{
								case "BranchFromItem":
									branch.branchFromItem = Item.FromXml(repository, reader);
									break;
								case "BranchToItem":
									branch.branchToItem = Item.FromXml(repository, reader);
									break;
								}
						}
				}

			return branch;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("BranchRelative instance ");
			sb.Append(GetHashCode());

			sb.Append("\n	 BranchFromItem: ");
			sb.Append(BranchFromItem);

			sb.Append("\n	 BranchToItem: ");
			sb.Append(BranchToItem);

			sb.Append("\n	 RelativeFromItemId: ");
			sb.Append(RelativeFromItemId);

			sb.Append("\n	 RelativeToItemId: ");
			sb.Append(RelativeToItemId);

			sb.Append("\n	 IsRequestedItem: ");
			sb.Append(IsRequestedItem);

			return sb.ToString();
		}
	}
}
