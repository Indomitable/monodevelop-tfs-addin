//
// Microsoft.TeamFoundation.VersionControl.Client.Changeset
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
	public sealed class Changeset
	{
		private static readonly string[] DateTimeFormats = { "yyyy-MM-ddTHH:mm:ss.fZ", "yyyy-MM-ddTHH:mm:ss.ffZ", "yyyy-MM-ddTHH:mm:ss.fffZ", "yyyy-MM-ddTHH:mm:ss.ffffZ", "yyyy-MM-ddTHH:mm:ss.fffffZ", "yyyy-MM-ddTHH:mm:ss.ffffffZ", "yyyy-MM-ddTHH:mm:ssZ" };
		private Uri artifactUri;
		private Change[] changes;
		private int changesetId;
		private string comment;
		private string committer;
		private string owner;
		private DateTime creationDate;
		private VersionControlServer versionControlServer;

		internal static Changeset FromXml(Repository repository, XmlReader reader)
		{
			string elementName = reader.Name;

			Changeset changeset = new Changeset();
			changeset.versionControlServer = repository.VersionControlServer;

			changeset.committer = reader.GetAttribute("cmtr");
			changeset.changesetId = Convert.ToInt32(reader.GetAttribute("cset"));
			string date = reader.GetAttribute("date");
			changeset.creationDate = DateTime.ParseExact(date, DateTimeFormats, null, DateTimeStyles.None);
			changeset.owner = reader.GetAttribute("owner");

 			List<Change> changes = new List<Change>();

			while (reader.Read())
				{
					if (reader.NodeType == XmlNodeType.EndElement && reader.Name == elementName)
						break;

					if (reader.NodeType == XmlNodeType.Element)
						{
							switch (reader.Name)
								{
								case "Change":
									changes.Add(Change.FromXml(repository, reader));
									break;
								case "Comment":
									changeset.comment = reader.ReadString();
									break;
								}
						}
				}

			changeset.changes = changes.ToArray();
			return changeset;
		}

		internal void ToXml(XmlWriter writer, string element)
		{
			writer.WriteStartElement(element);
			writer.WriteAttributeString("cmtr", Committer);
			writer.WriteAttributeString("date", CreationDate.ToString());
			writer.WriteAttributeString("cset", ChangesetId.ToString());
			writer.WriteElementString("owner", Owner);

			if (changes != null)
				{
					writer.WriteStartElement("Changes");

					foreach (Change change in changes)
						{
							change.ToXml(writer, "Change");
						}
					
					writer.WriteEndElement();
				}

			writer.WriteEndElement();
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("Changeset instance ");
			sb.Append(GetHashCode());

			sb.Append("\n	 ChangesetId: ");
			sb.Append(ChangesetId);

			sb.Append("\n	 ArtifactUri: ");
			sb.Append(ArtifactUri);

			sb.Append("\n	 Comment: ");
			sb.Append(Comment);

			sb.Append("\n	 Committer: ");
			sb.Append(Committer);

			sb.Append("\n	 Owner: ");
			sb.Append(Owner);

			foreach (Change change in Changes)
				{
					sb.Append("\n	 Change: ");
					sb.Append(change.ToString());
				}

			return sb.ToString();
		}

		public Uri ArtifactUri
		{
			get { return artifactUri; }
		}

		public Change[] Changes
		{
			get { return changes; }
		}

		public int ChangesetId
		{
			get { return changesetId; }
		}

		public string Comment
		{
			get { return comment; }
		}

		public string Committer
		{
			get { return committer; }
		}

		public string Owner
		{
			get { return owner; }
		}

		public DateTime CreationDate
		{
			get { return creationDate; }
		}

		public VersionControlServer VersionControlServer
		{
			get { return versionControlServer; }
		}

	}
}
