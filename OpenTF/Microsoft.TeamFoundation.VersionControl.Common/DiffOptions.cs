//
// Microsoft.TeamFoundation.VersionControl.Common.DiffOptions
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
using System.IO;
using System.Text;

namespace Microsoft.TeamFoundation.VersionControl.Common
{
	public class DiffOptions
	{
		public DiffOptions ()
		{
		}

		private bool useThirdPartyTool;
		private StreamWriter streamWriter;
		private Encoding targetEncoding;
		private Encoding sourceEncoding;
		private DiffOutputType outputType;
		private DiffOptionFlags flags;
		private string targetLabel;
		private string sourceLabel;

		public bool UseThirdPartyTool
		{
			get { return useThirdPartyTool; }
			set { useThirdPartyTool = value; }
		}

		public StreamWriter StreamWriter
		{
			get { return streamWriter; }
			set { streamWriter = value; }
		}

		public Encoding TargetEncoding
		{
			get { return targetEncoding; }
			set { targetEncoding = value; }
		}

		public Encoding SourceEncoding
		{
			get { return sourceEncoding; }
			set { sourceEncoding = value; }
		}

		public DiffOutputType OutputType
		{
			get { return outputType; }
			set { outputType = value; }
		}

		public DiffOptionFlags Flags
		{
			get { return flags; }
			set { flags = value; }
		}

		public string TargetLabel
		{
			get { return targetLabel; }
			set { targetLabel = value; }
		}

		public string SourceLabel
		{
			get { return sourceLabel; }
			set { sourceLabel = value; }
		}
	}
}
