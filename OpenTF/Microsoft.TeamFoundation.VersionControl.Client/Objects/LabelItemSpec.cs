//
// Microsoft.TeamFoundation.VersionControl.Client.LabelItemSpec
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
using Microsoft.TeamFoundation.VersionControl.Client.Helpers;

namespace Microsoft.TeamFoundation.VersionControl.Client.Objects
{
    public sealed class LabelItemSpec
    {
        public LabelItemSpec(ItemSpec itemSpec, VersionSpec version, bool exclude)
        {
            this.ItemSpec = itemSpec;
            this.Version = version;
            this.Exclude = exclude;
        }

        internal XElement ToXml(XName element)
        {
            return new XElement(element, 
                new XAttribute("ex", Exclude.ToLowString()),
                ItemSpec.ToXml(element.Namespace + "ItemSpec"),
                Version.ToXml(element.Namespace + "Version"));
        }

        public bool Exclude { get; set; }

        public ItemSpec ItemSpec { get; set; }

        public VersionSpec Version { get; set; }
    }
}