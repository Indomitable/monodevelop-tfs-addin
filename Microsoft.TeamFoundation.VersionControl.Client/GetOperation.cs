//
// Microsoft.TeamFoundation.VersionControl.Client.GetOperation
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
using System.Xml.Linq;
using Microsoft.TeamFoundation.VersionControl.Client.Enums;
using Microsoft.TeamFoundation.VersionControl.Client.Helpers;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
    internal class GetOperation : ILocalUpdateOperation
    {
        public ChangeType ChangeType { get; private set; }

        public int DeletionId { get; private set; }

        public int ItemId { get; private set; }

        public ItemType ItemType { get; private set; }

        public string TargetLocalItem { get; private set; }

        public string SourceLocalItem { get; private set; }

        public string SourceServerItem { get; private set; }

        public string TargetServerItem { get; private set; }

        public int VersionLocal { get; private set; }

        public int VersionServer { get; private set; }

        public string ArtifactUri { get; private set; }

        public LockLevel LockLevel { get; private set; }
        //<s:complexType name="GetOperation">
        //    <s:sequence>
        //        <s:element minOccurs="0" maxOccurs="1" name="HashValue" type="s:base64Binary"/>
        //        <s:element minOccurs="0" maxOccurs="1" name="Properties" type="tns:ArrayOfPropertyValue"/>
        //        <s:element minOccurs="0" maxOccurs="1" name="PropertyValues" type="tns:ArrayOfPropertyValue"/>
        //    </s:sequence>
        //    <s:attribute default="Any" name="type" type="tns:ItemType"/>
        //    <s:attribute default="0" name="itemid" type="s:int"/>
        //    <s:attribute name="slocal" type="s:string"/>
        //    <s:attribute name="tlocal" type="s:string"/>
        //    <s:attribute name="titem" type="s:string"/>
        //    <s:attribute name="sitem" type="s:string"/>
        //    <s:attribute default="0" name="sver" type="s:int"/>
        //    <s:attribute default="-2" name="vrevto" type="s:int"/>
        //    <s:attribute default="0" name="lver" type="s:int"/>
        //    <s:attribute default="0" name="did" type="s:int"/>
        //    <s:attribute default="0" name="chgEx" type="s:int"/>
        //    <s:attribute default="None" name="chg" type="tns:ChangeType"/>
        //    <s:attribute default="None" name="lock" type="tns:LockLevel"/>
        //    <s:attribute default="true" name="il" type="s:boolean"/>
        //    <s:attribute default="0" name="pcid" type="s:int"/>
        //    <s:attribute default="false" name="cnflct" type="s:boolean"/>
        //    <s:attribute default="None" name="cnflctchg" type="tns:ChangeType"/>
        //    <s:attribute default="0" name="cnflctchgEx" type="s:int"/>
        //    <s:attribute default="0" name="cnflctitemid" type="s:int"/>
        //    <s:attribute name="nmscnflct" type="s:unsignedByte" use="required"/>
        //    <s:attribute name="durl" type="s:string"/>
        //    <s:attribute default="-2" name="enc" type="s:int"/>
        //    <s:attribute default="0001-01-01T00:00:00" name="vsd" type="s:dateTime"/>
        //</s:complexType>
        internal static GetOperation FromXml(XElement element)
        {
            GetOperation getOperation = new GetOperation
            {
                ChangeType = ChangeType.None,
                ItemType = ItemType.Any,
                LockLevel = LockLevel.None,
                VersionServer = 0,
                VersionLocal = 0,
            };

            getOperation.ItemType = EnumHelper.ParseItemType(element.GetAttribute("type"));
            getOperation.ItemId = GeneralHelper.XmlAttributeToInt(element.GetAttribute("itemid"));
            getOperation.SourceLocalItem = TfsPath.ToPlatformPath(element.GetAttribute("slocal"));
            getOperation.TargetLocalItem = TfsPath.ToPlatformPath(element.GetAttribute("tlocal"));
            getOperation.SourceServerItem = element.GetAttribute("sitem");
            getOperation.TargetServerItem = element.GetAttribute("titem");
            getOperation.VersionServer = GeneralHelper.XmlAttributeToInt(element.GetAttribute("sver"));
            getOperation.VersionLocal = GeneralHelper.XmlAttributeToInt(element.GetAttribute("lver"));
            getOperation.ChangeType = EnumHelper.ParseChangeType(element.GetAttribute("chg"));

            // setup download url if found
            getOperation.ArtifactUri = element.GetAttribute("durl");

            // here's what you get if you remap a working folder from one
            // team project to another team project with the same file
            // first you get the update getOperation, then you get this later on
            // <GetOperation type="File" itemid="159025" slocal="foo.xml" titem="$/bar/foo.xml" lver="12002"><HashValue /></GetOperation>

            // look for a deletion id
            getOperation.DeletionId = GeneralHelper.XmlAttributeToInt(element.GetAttribute("did"));
            return getOperation;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("GetOperation instance ");
            sb.Append(GetHashCode());

            sb.Append("\n	 type: ");
            sb.Append(ItemType.ToString());

            sb.Append("\n	 itemid: ");
            sb.Append(ItemId);

            sb.Append("\n	 slocal: ");
            sb.Append(this.SourceLocalItem);

            sb.Append("\n	 tlocal: ");
            sb.Append(this.TargetLocalItem);

            sb.Append("\n	 titem: ");
            sb.Append(this.TargetServerItem);

            sb.Append("\n	 sver: ");
            sb.Append(this.VersionServer);

            sb.Append("\n	 lver: ");
            sb.Append(this.VersionLocal);

            sb.Append("\n	 did: ");
            sb.Append(this.DeletionId);

            sb.Append("\n	 ArtifactUri: ");
            sb.Append(this.ArtifactUri);

            sb.Append("\n	 ChangeType: ");
            sb.Append(ChangeType.ToString());

            return sb.ToString();
        }

        public bool IsAdd
        {
            get { return ChangeType.HasFlag(ChangeType.Add); }
        }

        public bool IsDelete
        {
            get { return ChangeType.HasFlag(ChangeType.Delete); }
        }

        public bool IsEdit
        {
            get { return ChangeType.HasFlag(ChangeType.Edit); }
        }

        public bool IsEncoding
        {
            get { return ChangeType.HasFlag(ChangeType.Encoding); }
        }

        public bool IsRename
        {
            get { return ChangeType.HasFlag(ChangeType.Rename); }
        }
    }
}
