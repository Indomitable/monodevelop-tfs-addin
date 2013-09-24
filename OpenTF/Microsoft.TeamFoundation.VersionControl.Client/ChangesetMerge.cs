//
// Microsoft.TeamFoundation.VersionControl.Client.ChangesetMerge
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
using System.Globalization;
using System.Text;
using System.Xml;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
	public sealed class ChangesetMerge
	{
		private bool partialFlag = false;
		private int sourceVersion;
		private int targetVersion;
		private Changeset targetChangeset;

		internal static ChangesetMerge FromXml(Repository repository, XmlReader reader)
		{
			ChangesetMerge merge = new ChangesetMerge();
			merge.sourceVersion = Convert.ToInt32(reader.GetAttribute("srcver"));
			merge.targetVersion = Convert.ToInt32(reader.GetAttribute("tgtver"));
			merge.partialFlag = Convert.ToBoolean(reader.GetAttribute("part"));

			return merge;
		}

		public bool Partial
		{
			get { return partialFlag; }
		}

		public int SourceVersion
		{
			get { return sourceVersion; }
		}

		public int TargetVersion
		{
			get { return targetVersion; }
		}

		public Changeset TargetChangeset
		{
			get { return targetChangeset; }
			internal set { targetChangeset = value; }
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("ChangesetMerge instance ");
			sb.Append(GetHashCode());

			sb.Append("\n	 SourceVersion: ");
			sb.Append(SourceVersion);

			sb.Append("\n	 TargetVersion: ");
			sb.Append(TargetVersion);

			sb.Append("\n	 Partial: ");
			sb.Append(Partial);

			return sb.ToString();
		}
	}
}