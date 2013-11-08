//
// Microsoft.TeamFoundation.VersionControl.Client.ChangeRequest
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
using Microsoft.TeamFoundation.VersionControl.Client.Enums;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
    //<s:complexType name="ChangeRequest">
    //    <s:sequence>
    //        <s:element minOccurs="0" maxOccurs="1" name="item" type="tns:ItemSpec"/>
    //        <s:element minOccurs="0" maxOccurs="1" name="vspec" type="tns:VersionSpec"/>
    //        <s:element minOccurs="0" maxOccurs="1" name="Properties" type="tns:ArrayOfPropertyValue"/>
    //    </s:sequence>
    //    <s:attribute default="None" name="req" type="tns:RequestType"/>
    //    <s:attribute default="0" name="did" type="s:int"/>
    //    <s:attribute default="-2" name="enc" type="s:int"/>
    //    <s:attribute default="Any" name="type" type="tns:ItemType"/>
    //    <s:attribute default="Unchanged" name="lock" type="tns:LockLevel"/>
    //    <s:attribute name="target" type="s:string"/>
    //    <s:attribute default="Any" name="targettype" type="tns:ItemType"/>
    //</s:complexType>
    internal class ChangeRequest
    {
        public ChangeRequest(string path, RequestType requestType, ItemType itemType,
                             RecursionType recursion, LockLevel lockLevel, VersionSpec version)
        {
            this.Item = new ItemSpec(path, recursion);
            this.RequestType = requestType;
            this.ItemType = itemType;
            this.LockLevel = lockLevel;
            this.VersionSpec = version;
        }

        public ChangeRequest(string path, RequestType requestType, ItemType itemType)
            : this(path, requestType, itemType, RecursionType.None, LockLevel.None, VersionSpec.Latest)
        {
        }

        public ChangeRequest(string path, string target, RequestType requestType, ItemType itemType)
            : this(path, requestType, itemType)
        {
            this.Target = target;
        }

        internal XElement ToXml(XNamespace ns)
        {
            var result = new XElement(ns + "ChangeRequest", 
                             new XAttribute("req", RequestType),
                             new XAttribute("type", ItemType));

            if (RequestType == RequestType.Lock || LockLevel != LockLevel.None)
                result.Add(new XAttribute("lock", LockLevel));

            if (RequestType == RequestType.Add)
                result.Add(new XAttribute("enc", Encoding));

            if (!string.IsNullOrEmpty(Target))
            {
                // convert local path specs from platform paths to tfs paths as needed
                string fxdTarget = VersionControlPath.IsServerItem(Target) ? Target : TfsPath.FromPlatformPath(Target);
                result.Add(new XAttribute("target", fxdTarget));
            }

            result.Add(this.Item.ToXml(ns + "item"));
            result.Add(this.VersionSpec.ToXml(ns + "vspec"));

            return result;
        }

        public LockLevel LockLevel { get; private set; }

        public ItemSpec Item { get; private set; }

        public VersionSpec VersionSpec { get; private set; }

        public ItemType ItemType { get; set; }

        public ItemType TargetType { get; set; }

        public RequestType RequestType { get; set; }

        public int DeletionId { get; set; }

        public int Encoding { get; set; }

        public string Target { get; set; }
    }
}

