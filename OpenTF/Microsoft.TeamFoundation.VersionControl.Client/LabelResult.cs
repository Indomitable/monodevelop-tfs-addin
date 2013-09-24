//
// Microsoft.TeamFoundation.VersionControl.Client.LabelResult
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
	public sealed class LabelResult
	{
    private string label;
    private string scope;
    private LabelResultStatus status;

		internal static LabelResult FromXml(Repository repository, XmlReader reader)
		{
			LabelResult labelResult = new LabelResult();

			labelResult.label = reader.GetAttribute("label");
			labelResult.scope = reader.GetAttribute("scope");

			string status = reader.GetAttribute("status");
			labelResult.status = (LabelResultStatus) Enum.Parse(typeof(LabelResultStatus), status, true);

			return labelResult;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("LabelResult instance ");
			sb.Append(GetHashCode());
			sb.Append("\n	 Label: ");
			sb.Append(Label);

			sb.Append("\n	 Scope: ");
			sb.Append(Scope);

			sb.Append("\n	 Status: ");
			sb.Append(Status);

			return sb.ToString();
		}

		public string Label
		{
			get { return label; }
		}

		public string Scope
		{
			get { return scope; }
		}

		public LabelResultStatus Status
		{
			get { return status; }
		}
	}
}

