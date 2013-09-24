//
// Microsoft.TeamFoundation.Server.Identity
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
	[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://schemas.microsoft.com/TeamFoundation/2005/06/Services/Authorization/03")]
	public class Identity
	{
		private string accountName;
		private string description;
		private string displayName;
		private string distinguishedName;
		private string mailAddress;
		private string domain;
		private string sid;
		private IdentityType type;

		public string AccountName
		{
			get { return accountName; }
			set { accountName = value; }
		}

		public string Description
		{
			get { return description; }
			set { description = value; }
		}

		public string DisplayName
		{
			get { return displayName; }
			set { displayName = value; }
		}

		public string DistinguishedName
		{
			get { return distinguishedName; }
			set { distinguishedName = value; }
		}

		public string Domain
		{
			get { return domain; }
			set { domain = value; }
		}

		public IdentityType Type
		{
			get { return type; }
			set { type = value; }
		}

		public string MailAddress
		{
			get { return mailAddress; }
			set { mailAddress = value; }
		}

		public string Sid
		{
			get { return sid; }
			set { sid = value; }
		}

		public Identity()
		{
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("Identity instance ");
			sb.Append(GetHashCode());

			sb.Append("\n	 DisplayName: ");
			sb.Append(DisplayName);

			return sb.ToString();
		}
	}
}

