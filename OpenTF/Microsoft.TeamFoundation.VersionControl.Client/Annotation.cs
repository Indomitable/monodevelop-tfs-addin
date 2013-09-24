//
// Microsoft.TeamFoundation.VersionControl.Client.Annotation
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
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Xml;
using System.Web.Services;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
	internal class Annotation 
	{
		private string item;
		private int version;
    private string name;
    private string value;
    private System.DateTime date;
    private string comment;

		internal static Annotation FromXml(Repository repository, XmlReader reader)
		{
			string elementName = reader.Name;
			Annotation annotation = new Annotation();

			annotation.item = reader.GetAttribute("item");
			annotation.version = Convert.ToInt32(reader.GetAttribute("v"));
			annotation.name = reader.GetAttribute("name");
			annotation.value = reader.GetAttribute("value");

			string date = reader.GetAttribute("date");
			if (!String.IsNullOrEmpty(date))
				annotation.date = DateTime.Parse(date);

			while (reader.Read())
				{
					if (reader.NodeType == XmlNodeType.EndElement && reader.Name == elementName)
						break;

					if (reader.NodeType == XmlNodeType.Element)
						{
							switch (reader.Name)
								{
								case "Comment":
									annotation.comment = reader.ReadString();
									break;
								}
						}
				}

			return annotation;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("Annotation instance ");
			sb.Append(GetHashCode());

			sb.Append("\n	 Comment: ");
			sb.Append(Comment);

			sb.Append("\n	 Version: ");
			sb.Append(Version);

			sb.Append("\n	 Name: ");
			sb.Append(Name);

			sb.Append("\n	 Value: ");
			sb.Append(Value);

			sb.Append("\n	 Date: ");
			sb.Append(Date);

			return sb.ToString();
		}

		public string Comment
		{
			get { return comment; }
		}

		public string Item
		{
			get { return item; }
		}

		public string Name
		{
			get { return name; }
		}

		public string Value
		{
			get { return value; }
		}

		public int Version
		{
			get { return version; }
		}

		public DateTime Date
		{
			get { return date; }
		}
	}
}

