//
// Microsoft.TeamFoundation.VersionControl.Client.Changeset
//
// Authors:
//	Joel Reed (joelwreed@gmail.com)
//  Ventsislav Mladenov (ventsislav.mladenov@gmail.com)
//
// Copyright (C) 2013 Joel Reed, Ventsislav Mladenov
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
using System.Globalization;
using System.Text;
using System.Xml.Linq;
using System.Linq;
using Microsoft.TeamFoundation.VersionControl.Client.Helpers;

namespace Microsoft.TeamFoundation.VersionControl.Client.Objects
{
    public sealed class Changeset
    {
        private static readonly string[] DateTimeFormats = { "yyyy-MM-ddTHH:mm:ss.fZ", "yyyy-MM-ddTHH:mm:ss.ffZ", "yyyy-MM-ddTHH:mm:ss.fffZ", "yyyy-MM-ddTHH:mm:ss.ffffZ", "yyyy-MM-ddTHH:mm:ss.fffffZ", "yyyy-MM-ddTHH:mm:ss.ffffffZ", "yyyy-MM-ddTHH:mm:ssZ" };
        //      <QueryChangesetResult cmtr="string" cmtrdisp="string" date="dateTime" cset="int" owner="string" ownerdisp="string">
        //        <Comment>string</Comment>
        //        <CheckinNote>
        //          <Values>
        //            <CheckinNoteFieldValue xsi:nil="true" />
        //            <CheckinNoteFieldValue xsi:nil="true" />
        //          </Values>
        //        </CheckinNote>
        //        <PolicyOverride>
        //          <Comment>string</Comment>
        //          <PolicyFailures>
        //            <PolicyFailureInfo xsi:nil="true" />
        //            <PolicyFailureInfo xsi:nil="true" />
        //          </PolicyFailures>
        //        </PolicyOverride>
        //        <Properties>
        //          <PropertyValue pname="string">
        //            <val />
        //          </PropertyValue>
        //          <PropertyValue pname="string">
        //            <val />
        //          </PropertyValue>
        //        </Properties>
        //        <Changes>
        //          <Change type="None or Add or Edit or Encoding or Rename or Delete or Undelete or Branch or Merge or Lock or Rollback or SourceRename or Property" typeEx="int">
        //            <Item xsi:nil="true" />
        //            <MergeSources xsi:nil="true" />
        //          </Change>
        //          <Change type="None or Add or Edit or Encoding or Rename or Delete or Undelete or Branch or Merge or Lock or Rollback or SourceRename or Property" typeEx="int">
        //            <Item xsi:nil="true" />
        //            <MergeSources xsi:nil="true" />
        //          </Change>
        //        </Changes>
        //      </QueryChangesetResult>
        internal static Changeset FromXml(XElement element)
        {
            Changeset changeset = new Changeset();
            changeset.Committer = element.Attribute("cmtr").Value;
            changeset.ChangesetId = GeneralHelper.XmlAttributeToInt(element.Attribute("cset").Value);
            string date = element.Attribute("date").Value;
            changeset.CreationDate = DateTime.ParseExact(date, DateTimeFormats, null, DateTimeStyles.None);
            changeset.Owner = element.Attribute("owner").Value;

            changeset.Comment = element.Element(element.Name.Namespace + "Comment").Value;

            changeset.Changes = element.Element(element.Name.Namespace + "Changes")
                .Elements(element.Name.Namespace + "Change")
                .Select(Change.FromXml).ToArray();
            return changeset;
        }
        //        internal void ToXml(XmlWriter writer, string element)
        //        {
        //            writer.WriteStartElement(element);
        //            writer.WriteAttributeString("cmtr", Committer);
        //            writer.WriteAttributeString("date", CreationDate.ToString());
        //            writer.WriteAttributeString("cset", ChangesetId.ToString());
        //            writer.WriteElementString("owner", Owner);
        //
        //            if (Changes != null)
        //            {
        //                writer.WriteStartElement("Changes");
        //
        //                foreach (Change change in Changes)
        //                {
        //                    change.ToXml(writer, "Change");
        //                }
        //
        //                writer.WriteEndElement();
        //            }
        //
        //            writer.WriteEndElement();
        //        }
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

        public Uri ArtifactUri { get; private set; }

        public Change[] Changes { get; private set; }

        public int ChangesetId { get; private set; }

        public string Comment { get; private set; }

        public string Committer { get; private set; }

        public string Owner { get; private set; }

        public DateTime CreationDate { get; private set; }
    }
}
