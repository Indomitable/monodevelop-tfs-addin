//
// Microsoft.TeamFoundation.VersionControl.Client.VersionControlLabel
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
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Linq;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
    //    public sealed class VersionControlLabel
    //    {
    //        private string comment;
    //        private Item[] items;
    //        private int labelId;
    //        private System.DateTime lastModifiedDate;
    //        private string name;
    //        private string scope;
    //        private string ownerName;
    //
    //        internal VersionControlLabel()
    //        {
    //        }
    //
    //        public VersionControlLabel(VersionControlServer versionControlServer,
    //                                   string name, string ownerName, string scope,
    //                                   string comment)
    //        {
    //            this.name = name;
    //            this.ownerName = ownerName;
    //            this.scope = scope;
    //            this.comment = comment;
    //            this.lastModifiedDate = new DateTime(1);
    //        }
    //        //        <VersionControlLabel date="dateTime" name="string" owner="string" ownerdisp="string" owneruniq="string" scope="string" lid="int">
    //        //          <Comment>string</Comment>
    //        //          <Items>
    //        //            <Item xsi:nil="true" />
    //        //            <Item xsi:nil="true" />
    //        //          </Items>
    //        //        </VersionControlLabel>
    //        internal static VersionControlLabel FromXml(XElement element)
    //        {
    //            VersionControlLabel label = new VersionControlLabel();
    //            label.lastModifiedDate = DateTime.Parse(element.Attribute("date").Value);
    //            label.name = element.Attribute("name").Value;
    //            label.ownerName = element.Attribute("owner").Value;
    //            label.scope = element.Attribute("scope").Value;
    //            label.labelId = Convert.ToInt32(element.Attribute("lid").Value);
    //            label.comment = element.Element(XmlNamespaces.GetMessageElementName("Comment")).Value;
    //            label.items = element.Element(XmlNamespaces.GetMessageElementName("Items"))
    //                .Elements(XmlNamespaces.GetMessageElementName("Item"))
    //                .Select(el => Item.FromXml(el)).ToArray();
    //            return label;
    //        }
    //
    //        internal void ToXml(XmlWriter writer, string element)
    //        {
    //            writer.WriteStartElement(element);
    //            writer.WriteAttributeString("date", LastModifiedDate.ToString("s"));
    //            writer.WriteAttributeString("name", Name);
    //
    //            if (!String.IsNullOrEmpty(OwnerName))
    //                writer.WriteAttributeString("owner", OwnerName);
    //            if (!String.IsNullOrEmpty(Scope))
    //                writer.WriteAttributeString("scope", Scope);
    //
    //            writer.WriteAttributeString("lid", LabelId.ToString());
    //            writer.WriteEndElement();
    //        }
    //
    //        public override string ToString()
    //        {
    //            StringBuilder sb = new StringBuilder();
    //
    //            sb.Append("VersionControlLabel instance ");
    //            sb.Append(GetHashCode());
    //
    //            sb.Append("\n	 Comment: ");
    //            sb.Append(Comment);
    //
    //            sb.Append("\n	 LastModifiedDate: ");
    //            sb.Append(LastModifiedDate);
    //
    //            foreach (Item item in items)
    //            {
    //                sb.Append("\n  Item: ");
    //                sb.Append(item.ToString());
    //            }
    //
    //            sb.Append("\n	 Name: ");
    //            sb.Append(Name);
    //
    //            sb.Append("\n	 OwnerName: ");
    //            sb.Append(OwnerName);
    //
    //            sb.Append("\n	 Scope: ");
    //            sb.Append(Scope);
    //
    //            sb.Append("\n	 LabelId: ");
    //            sb.Append(LabelId);
    //
    //            return sb.ToString();
    //        }
    //
    //        public Uri ArtifactUri { get { return new Uri("vstfs:///VersionControl/Label/" + LabelId); } }
    //
    //        public string Comment { get { return comment; } }
    //
    //        public Item[] Items { get { return items; } }
    //
    //        public int LabelId { get { return labelId; } }
    //
    //        public DateTime LastModifiedDate { get { return lastModifiedDate; } }
    //
    //        public string Name { get { return name; } }
    //
    //        public string OwnerName { get { return ownerName; } }
    //
    //        public string Scope { get { return scope; } }
    //    }
}

