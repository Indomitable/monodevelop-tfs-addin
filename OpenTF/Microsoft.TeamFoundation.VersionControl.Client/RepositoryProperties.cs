//
// Microsoft.TeamFoundation.VersionControl.Client.RepositoryProperties
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
using System.Xml;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
    //    internal sealed class RepositoryProperties
    //    {
    //        private string id;
    //        private string name;
    //        private string ver;
    //        private int latestChangesetId;
    //
    //        internal static RepositoryProperties FromXml(XmlReader reader)
    //        {
    //            RepositoryProperties repositoryProperties = new RepositoryProperties();
    //            repositoryProperties.id = reader.GetAttribute("id");
    //            repositoryProperties.name = reader.GetAttribute("name");
    //            repositoryProperties.ver = reader.GetAttribute("ver");
    //            repositoryProperties.latestChangesetId = Convert.ToInt32(reader.GetAttribute("lcset"));
    //            return repositoryProperties;
    //        }
    //
    //        internal void ToXml(XmlWriter writer, string element)
    //        {
    //            writer.WriteStartElement(element);
    //            writer.WriteAttributeString("id", Id);
    //            writer.WriteAttributeString("name", Name);
    //            writer.WriteAttributeString("ver", ver);
    //            writer.WriteAttributeString("lcset", LatestChangesetId.ToString());
    //            writer.WriteEndElement();
    //        }
    //
    //        public override string ToString()
    //        {
    //            StringBuilder sb = new StringBuilder();
    //
    //            sb.Append("RepositoryProperties instance ");
    //            sb.Append(GetHashCode());
    //
    //            sb.Append("\n	 Id: ");
    //            sb.Append(Id);
    //
    //            sb.Append("\n	 Name: ");
    //            sb.Append(Name);
    //
    //            sb.Append("\n	 Ver: ");
    //            sb.Append(Ver);
    //
    //            sb.Append("\n	 LatestChangesetId: ");
    //            sb.Append(LatestChangesetId);
    //
    //            return sb.ToString();
    //        }
    //
    //        public string Id
    //        {
    //            get { return id; }
    //        }
    //
    //        public string Name
    //        {
    //            get { return name; }
    //        }
    //
    //        public int LatestChangesetId
    //        {
    //            get { return latestChangesetId; }
    //        }
    //
    //        public string Ver
    //        {
    //            get { return ver; }
    //        }
    //    }
}
