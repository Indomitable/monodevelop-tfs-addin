//
// Microsoft.TeamFoundation.VersionControl.Client.AccessEntry
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
    //	public class AccessEntry
    //	{
    //		private string[] allow;
    //		private string[] deny;
    //		private string[] allowInherited;
    //		private string[] denyInherited;
    //		private string ident;
    //
    //		internal static string[] ReadPermissions(XmlReader reader)
    //		{
    //			if (reader.IsEmptyElement) return new string[0];
    //
    //			string elementName = reader.Name;
    //
    // 			List<string> perms = new List<string>();
    //			while (reader.Read())
    //				{
    //					if (reader.NodeType == XmlNodeType.EndElement && reader.Name == elementName)
    //						break;
    //
    //					if (reader.NodeType == XmlNodeType.Element && reader.Name == "string")
    //						{
    //							string perm = reader.ReadString();
    //							perms.Add(perm);
    //						}
    //				}
    //
    //			return perms.ToArray();
    //		}
    //
    //		internal static AccessEntry FromXml(Repository repository, XmlReader reader)
    //		{
    //			AccessEntry entry = new AccessEntry();
    //			string elementName = reader.Name;
    //
    //			entry.ident = reader.GetAttribute("ident");
    //
    //			while (reader.Read())
    //				{
    //					if (reader.NodeType == XmlNodeType.EndElement && reader.Name == elementName)
    //						break;
    //
    //					if (reader.NodeType == XmlNodeType.Element)
    //						{
    //							switch (reader.Name)
    //								{
    //								case "Allow":
    //									entry.allow = ReadPermissions(reader);
    //									break;
    //								case "Deny":
    //									entry.deny = ReadPermissions(reader);
    //									break;
    //								case "AllowInherited":
    //									entry.allowInherited = ReadPermissions(reader);
    //									break;
    //								case "DenyInherited":
    //									entry.denyInherited = ReadPermissions(reader);
    //									break;
    //								}
    //						}
    //				}
    //
    //			return entry;
    //		}
    //
    //		public string[] Allow
    //		{
    //			get { return allow; }
    //		}
    //
    //		public string[] Deny
    //		{
    //			get { return deny; }
    //		}
    //
    //		public string[] AllowInherited
    //		{
    //			get { return allowInherited; }
    //		}
    //
    //		public string[] DenyInherited
    //		{
    //			get { return denyInherited; }
    //		}
    //
    //		public string IdentityName
    //		{
    //			get { return ident; }
    //		}
    //	}
}
