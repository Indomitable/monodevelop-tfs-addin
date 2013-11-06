//
// Microsoft.TeamFoundation.VersionControl.Client.ItemSet
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

using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;

namespace Microsoft.TeamFoundation.VersionControl.Client.Objects
{
    public sealed class ItemSet
    {
        private Item[] items;
        private string pattern;
        private string queryPath;
        //<QueryItemsResult xmlns="http://schemas.microsoft.com/TeamFoundation/2005/06/VersionControl/ClientServices/03">
        //  <ItemSet>
        //    <QueryPath>$/</QueryPath>
        //    <Items>
        //      <Item cs="1" date="2006-12-15T16:16:26.95Z" enc="-3" type="Folder" itemid="1" item="$/" />
        internal static ItemSet FromXml(XElement element)
        {
            ItemSet itemSet = new ItemSet();
            List<Item> items = new List<Item>();

            var patternElement = element.Element(XmlNamespaces.GetMessageElementName("Pattern"));
            itemSet.pattern = patternElement != null ? patternElement.Value : string.Empty;

            var queryPathElement = element.Element(XmlNamespaces.GetMessageElementName("QueryPath"));
            itemSet.queryPath = queryPathElement != null ? queryPathElement.Value : string.Empty;

            var itemElements = element.Element(XmlNamespaces.GetMessageElementName("Items")).Elements(XmlNamespaces.GetMessageElementName("Item"));
            items.AddRange(itemElements.Select(it => Item.FromXml(it)));

            items.Sort();
            itemSet.items = items.ToArray();
            return itemSet;
        }

        public Item[] Items { get { return items; } }

        public string Pattern { get { return pattern; } }

        public string QueryPath { get { return queryPath; } }
    }
}

