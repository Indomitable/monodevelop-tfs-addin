//
// Microsoft.TeamFoundation.VersionControl.Client.Shelveset
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
using System.Globalization;
using System.Text;
using System.Xml;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
	internal class ShelvesetGenericComparer : IComparer<Shelveset>
	{
		public static ShelvesetGenericComparer Instance = new ShelvesetGenericComparer();

		public int Compare(Shelveset x, Shelveset y)
		{
			int cmp = String.Compare(x.Name, y.Name);
			if (cmp != 0) return cmp;
			return String.Compare(x.Name, y.OwnerName);
		}
	}

	public sealed class Shelveset
	{
		private static readonly string[] DateTimeFormats = { "yyyy-MM-ddTHH:mm:ss.FFFFFFFZ", "yyyy-MM-ddTHH:mm:ssZ" };
		private string name;
		private string comment = String.Empty;
		private string ownerName;
		private DateTime creationDate = new DateTime(0);
		private VersionControlServer versionControlServer;

		public Shelveset (VersionControlServer versionControlServer,
											string name, string ownerName)
		{
			this.versionControlServer = versionControlServer;
			this.name = name;
			this.ownerName = ownerName;
		}

		internal static Shelveset FromXml(Repository repository, XmlReader reader)
		{
			string elementName = reader.Name;
			string ownerName = reader.GetAttribute("owner");
			string name = reader.GetAttribute("name");

			Shelveset shelveset = new Shelveset(repository.VersionControlServer, name, ownerName);
			shelveset.creationDate =  DateTime.ParseExact(reader.GetAttribute("date"), DateTimeFormats, null, DateTimeStyles.None);

			while (reader.Read())
				{
					if (reader.NodeType == XmlNodeType.EndElement && reader.Name == elementName)
						break;

					if (reader.NodeType == XmlNodeType.Element)
						{
							switch (reader.Name)
								{
								case "Comment":
									shelveset.comment = reader.ReadString();
									break;
								}
						}
				}

			return shelveset;
		}

		internal void ToXml(XmlWriter writer, string element)
		{
			writer.WriteStartElement(element);
			writer.WriteAttributeString("date", CreationDate.ToString("s"));
			writer.WriteAttributeString("name", Name);
			writer.WriteAttributeString("owner", OwnerName);
			writer.WriteEndElement();
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("Shelveset instance ");
			sb.Append(GetHashCode());

			sb.Append("\n	 Name: ");
			sb.Append(Name);

			sb.Append("\n	 Comment: ");
			sb.Append(Comment);

			sb.Append("\n	 OwnerName: ");
			sb.Append(OwnerName);

			return sb.ToString();
		}

		public string Name
		{
			get { return name; }
		}

		public string Comment
		{
			get { return comment; }
		}

		public string OwnerName
		{
			get { return ownerName; }
		}

		public DateTime CreationDate
		{
			get { return creationDate; }
		}

		public VersionControlServer VersionControlServer
		{
			get { return versionControlServer; }
		}

		internal class ShelvesetComparer : IComparer
		{
			public static ShelvesetComparer Instance = new ShelvesetComparer();

			public int Compare(object xo, object yo)
			{
				Shelveset x = xo as Shelveset;
				Shelveset y = yo as Shelveset;
				return ShelvesetGenericComparer.Instance.Compare(x, y);
			}
		}

		public static IComparer NameComparer {
			get { 
				return ShelvesetComparer.Instance; 
			}
		}

	}
}
