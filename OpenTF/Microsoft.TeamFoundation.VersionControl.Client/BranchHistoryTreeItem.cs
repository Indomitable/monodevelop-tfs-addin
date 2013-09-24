//
// Microsoft.TeamFoundation.VersionControl.Client.BranchHistoryTreeItem
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Web.Services;
using Microsoft.TeamFoundation.VersionControl.Common;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
	public sealed class BranchHistoryTreeItem
	{
		private BranchHistoryTreeItem parent;
		private BranchRelative relative;
		private int level = 0;
		private ArrayList children = new ArrayList();

		internal BranchHistoryTreeItem(BranchRelative[] branches)
		{
			foreach (BranchRelative branch in branches)
				{
					if (branch.RelativeFromItemId == 0) relative = branch;
					else children.Add(branch);
				}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("BranchHistoryTreeItem instance ");
			sb.Append(GetHashCode());

			sb.Append("\n	 Parent: ");
			sb.Append(Parent);

			sb.Append("\n	 Relative: ");
			sb.Append(Relative);

			sb.Append("\n	 Level: ");
			sb.Append(Level);

			sb.Append("\n	 Children: ");
			sb.Append(Children);

			return sb.ToString();
		}

		public BranchHistoryTreeItem Parent
		{
			get { return parent; }
		}

		public BranchRelative Relative
		{
			get { return relative; }
		}

		public int Level
		{
			get { return level; }
		}

		public ICollection Children
		{
			get { return children;
			}
		}
	}
}