//
// Microsoft.TeamFoundation.VersionControl.Client.GetRequest
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

using System.Xml.Linq;
using Microsoft.TeamFoundation.VersionControl.Client.Objects;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
    public class GetRequest
    {
        public GetRequest(string item, RecursionType recursionType, VersionSpec versionSpec)
        {
            this.ItemSpec = new ItemSpec(item, recursionType);
            this.VersionSpec = versionSpec;
        }

        public GetRequest(ItemSpec itemSpec, VersionSpec versionSpec)
        {
            this.ItemSpec = itemSpec;
            this.VersionSpec = versionSpec;
        }

        internal GetRequest(VersionSpec versionSpec)
        {
            this.VersionSpec = versionSpec;
        }

        public ItemSpec ItemSpec { get; private set; }

        public VersionSpec VersionSpec { get; private set; }

        internal XElement ToXml(XNamespace ns)
        {
            XElement result = new XElement(ns + "GetRequest");
            if (ItemSpec != null)
                result.Add(ItemSpec.ToXml(ns + "ItemSpec"));
            result.Add(VersionSpec.ToXml(ns + "VersionSpec"));
            return result;
        }

        public override string ToString()
        {
            return string.Format("[GetRequest: ItemSpec={0}, VersionSpec={1}]", ItemSpec, VersionSpec);
        }
    }
}
