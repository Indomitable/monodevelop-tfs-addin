//
// OrderByNodes.cs
//
// Author:
//       Ventsislav Mladenov <vmladenov.mladenov@gmail.com>
//
// Copyright (c) 2013 Ventsislav Mladenov
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Text;

namespace Microsoft.TeamFoundation.WorkItemTracking.Client.Query
{
    enum Direction
    {
        Asc,
        Desc,
    }

    class OrderByNode
    {
        public OrderByNode(string name, Direction direction)
        {
            ColumnName = name.Trim().Trim('[', ']');
            this.Direction = direction;
        }

        public string ColumnName { get; set; }

        public Direction Direction { get; set; }
    }

    class OrderByList : List<OrderByNode>
    {
        public override string ToString()
        {
            return string.Join(", ", this.Select(x => "[" + x.ColumnName + "] " + x.Direction));
        }

        public string WriteToXml()
        {
            StringBuilder builder = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = Encoding.UTF8;
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            //settings.NewLineChars = Environment.NewLine;
            using (XmlWriter writer = XmlWriter.Create(builder, settings))
            {
                writer.WriteStartElement("sort");
                foreach (var node in this)
                {
                    writer.WriteStartElement("QuerySortOrderEntry");
                    writer.WriteElementString("ColumnName", node.ColumnName);
                    writer.WriteElementString("Ascending", node.Direction == Direction.Asc ? "true" : "false");
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }

            return builder.ToString();
        }
    }
}

