//
// Microsoft.TeamFoundation.Server.ServiceInterface
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

namespace Microsoft.TeamFoundation.Server
{
	public class ServiceInterface
	{
		private string name;
		private string url;

		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		public string Url
		{
			get { return url; }
			set { url = value; }
		}

		public ServiceInterface ()
		{
		}

		public ServiceInterface (string name, string url)
		{
			this.name = name;
			this.url = url;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("ServiceInterface instance ");
			sb.Append(GetHashCode());

			sb.Append("\n	 Name: ");
			sb.Append(Name);

			sb.Append("\n	 Url: ");
			sb.Append(Url);

			return sb.ToString();
		}
	}
}

