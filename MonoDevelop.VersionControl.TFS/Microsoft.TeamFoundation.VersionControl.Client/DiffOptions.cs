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
using Microsoft.TeamFoundation.VersionControl.Client.Enums;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
    public class DiffOptions
    {
        public DiffOptions()
        {
        }

        public bool UseThirdPartyTool { get; set; }

        public StreamWriter StreamWriter { get; set; }

        public Encoding TargetEncoding { get; set; }

        public Encoding SourceEncoding { get; set; }

        public DiffOutputType OutputType { get; set; }

        public DiffOptionFlags Flags { get; set; }

        public string TargetLabel { get; set; }

        public string SourceLabel { get; set; }
    }
}
