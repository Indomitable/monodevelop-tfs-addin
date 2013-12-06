//
// Microsoft.TeamFoundation.VersionControl.Client.ExtendedItem
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

namespace Microsoft.TeamFoundation.VersionControl.Client.Objects
{
    public sealed class ExtendedItem : BaseItem
    {
        //          <ExtendedItem lver="int" did="int" latest="int" type="Any or Folder or File" enc="int" itemid="int" local="string" titem="string" sitem="string" chg="None or Add or Edit or Encoding or Rename or Delete or Undelete or Branch or Merge or Lock or Rollback or SourceRename or Property" chgEx="int" ochg="boolean" lock="None or Checkin or CheckOut or Unchanged" lowner="string" lownerdisp="string" date="dateTime">
        //            <IsBranch>boolean</IsBranch>
        //            <PropertyValues xsi:nil="true" />
        //          </ExtendedItem>
        //        <s:complexType name="ExtendedItem">
        //            <s:sequence>
        //                <s:element minOccurs="0" maxOccurs="1" default="false" name="IsBranch" type="s:boolean"/>
        //                <s:element minOccurs="0" maxOccurs="1" name="PropertyValues" type="tns:ArrayOfPropertyValue"/>
        //            </s:sequence>
        //            <s:attribute default="0" name="lver" type="s:int"/>
        //            <s:attribute default="0" name="did" type="s:int"/>
        //            <s:attribute default="0" name="latest" type="s:int"/>
        //            <s:attribute default="Any" name="type" type="tns:ItemType"/>
        //            <s:attribute default="-3" name="enc" type="s:int"/>
        //            <s:attribute default="0" name="itemid" type="s:int"/>
        //            <s:attribute name="local" type="s:string"/>
        //            <s:attribute name="titem" type="s:string"/>
        //            <s:attribute name="sitem" type="s:string"/>
        //            <s:attribute default="None" name="chg" type="tns:ChangeType"/>
        //            <s:attribute default="0" name="chgEx" type="s:int"/>
        //            <s:attribute default="false" name="ochg" type="s:boolean"/>
        //            <s:attribute default="None" name="lock" type="tns:LockLevel"/>
        //            <s:attribute name="lowner" type="s:string"/>
        //            <s:attribute name="lownerdisp" type="s:string"/>
        //            <s:attribute name="date" type="s:dateTime" use="required"/>
        //        </s:complexType>
        internal static ExtendedItem FromXml(XElement element)
        {
            ExtendedItem item = new ExtendedItem
            {
                ChangeType = ChangeType.None,
                VersionLocal = 0,
                DeletionId = 0,
                VersionLatest = 0,
                ItemType = ItemType.Any,
                Encoding = -3,
                ItemId = 0,
                HasOtherPendingChange = false,
                LockStatus = LockLevel.None
            };

            item.ChangeType = EnumHelper.ParseChangeType(element.GetAttribute("chg"));
            item.HasOtherPendingChange = GeneralHelper.XmlAttributeToBool(element.GetAttribute("ochg"));
            item.LockStatus = EnumHelper.ParseLockLevel(element.GetAttribute("lock"));
            item.LockOwner = element.GetAttribute("lowner");
            item.LocalItem = TfsPath.ToPlatformPath(element.GetAttribute("local"));
            item.TargetServerItem = element.GetAttribute("titem");
            item.SourceServerItem = element.GetAttribute("sitem");
            item.ItemType = EnumHelper.ParseItemType(element.GetAttribute("type"));
            item.ItemId = GeneralHelper.XmlAttributeToInt(element.GetAttribute("itemid"));
            item.Encoding = GeneralHelper.XmlAttributeToInt(element.GetAttribute("enc"));
            item.VersionLocal = GeneralHelper.XmlAttributeToInt(element.GetAttribute("lver"));
            item.VersionLatest = GeneralHelper.XmlAttributeToInt(element.GetAttribute("latest"));
            item.DeletionId = GeneralHelper.XmlAttributeToInt(element.GetAttribute("did"));
            item.CheckinDate = GeneralHelper.XmlAttributeToDate(element.GetAttribute("date"));

            if (element.Element(XmlNamespaces.GetMessageElementName("IsBranch")) != null &&
                !string.IsNullOrEmpty(element.Element(XmlNamespaces.GetMessageElementName("IsBranch")).Value))
                item.IsBranch = GeneralHelper.XmlAttributeToBool(element.Element(XmlNamespaces.GetMessageElementName("IsBranch")).Value);

            return item;
        }

        public override string ToString()
        {
            return TargetServerItem;
        }

        public int VersionLatest { get; private set; }

        public int VersionLocal { get; private set; }

        public LockLevel LockStatus { get; private set; }

        public bool IsLocked
        {
            get
            {
                return LockStatus.HasFlag(LockLevel.CheckOut) || LockStatus.HasFlag(LockLevel.Checkin);
            }
        }

        public ChangeType ChangeType { get; private set; }

        public int DeletionId { get; private set; }

        public bool HasOtherPendingChange { get; private set; }

        public bool IsInWorkspace { get { return !string.IsNullOrEmpty(LocalItem); } }

        public bool IsLatest { get { return VersionLatest == VersionLocal; } }

        public int ItemId { get; private set; }

        public int Encoding { get; private set; }

        public string LocalItem { get; private set; }

        public string LockOwner { get; private set; }

        public string SourceServerItem { get; private set; }

        public string TargetServerItem { get; private set; }

        public DateTime CheckinDate { get; private set; }

        public override VersionControlPath ServerPath { get { return TargetServerItem; } }

        public bool IsBranch { get; private set; }
    }
}

