//
// Microsoft.TeamFoundation.VersionControl.Client.ItemSpec
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
using System.Xml.Linq;

namespace Microsoft.TeamFoundation.VersionControl.Client.Objects
{
    public class ItemSpec
    {
        public ItemSpec(string item, RecursionType recursionType)
        {
            if (string.IsNullOrEmpty(item))
                throw new ArgumentException("Value cannot be null or empty.");

            this.Item = item;
            this.RecursionType = recursionType;
        }

        public ItemSpec(string item, RecursionType recursionType, int deletionId)
        {
            if (string.IsNullOrEmpty(item))
                throw new ArgumentException("Value cannot be null or empty.");

            this.Item = item;
            this.RecursionType = recursionType;
            this.DeletionId = deletionId;
        }

        internal XElement ToXml(XName element)
        {
            XElement result = new XElement(element);
            if (this.RecursionType != RecursionType.None)
                result.Add(new XAttribute("recurse", RecursionType));
            if (this.DeletionId != 0)
                result.Add(new XAttribute("did", DeletionId));
            if (VersionControlPath.IsServerItem(Item))
                result.Add(new XAttribute("item", Item));
            else
                result.Add(new XAttribute("item", TfsPath.FromPlatformPath(Item)));
            return result;
        }

        public int DeletionId { get; set; }

        public string Item { get; set; }

        public RecursionType RecursionType { get; set; }

        public override string ToString()
        {
            return string.Format("Item: {0}, Recursion: {1}", Item, RecursionType);
        }
    }
}
