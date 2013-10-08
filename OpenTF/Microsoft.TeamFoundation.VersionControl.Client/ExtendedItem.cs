//
// Microsoft.TeamFoundation.VersionControl.Client.ExtendedItem
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
using System.Xml.Linq;
using Microsoft.TeamFoundation.VersionControl.Common;
using Microsoft.TeamFoundation.Common;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
    public sealed class ExtendedItem : IItem
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

            if (!string.IsNullOrEmpty(element.GetAttribute("chg")))
                item.ChangeType = (ChangeType)Enum.Parse(typeof(ChangeType), element.Attribute("chg").Value, true);

            if (!string.IsNullOrEmpty(element.GetAttribute("ochg")))
                item.HasOtherPendingChange = bool.Parse(element.Attribute("ochg").Value);

            if (!string.IsNullOrEmpty(element.GetAttribute("lock")))
                item.LockStatus = (LockLevel)Enum.Parse(typeof(LockLevel), element.Attribute("lock").Value, true);

            if (!string.IsNullOrEmpty(element.GetAttribute("lowner")))
                item.LockOwner = element.Attribute("lowner").Value;

            if (!string.IsNullOrEmpty(element.GetAttribute("local")))
                item.LocalItem = TfsPath.ToPlatformPath(element.Attribute("local").Value);

            if (!string.IsNullOrEmpty(element.GetAttribute("titem")))
                item.TargetServerItem = element.Attribute("titem").Value;

            if (!string.IsNullOrEmpty(element.GetAttribute("sitem")))
                item.SourceServerItem = element.Attribute("sitem").Value;

            if (!string.IsNullOrEmpty(element.GetAttribute("type")))
                item.ItemType = (ItemType)Enum.Parse(typeof(ItemType), element.Attribute("type").Value, true);

            if (!string.IsNullOrEmpty(element.GetAttribute("itemid")))
                item.ItemId = Convert.ToInt32(element.Attribute("itemid").Value);

            if (!string.IsNullOrEmpty(element.GetAttribute("enc")))
                item.Encoding = Convert.ToInt32(element.Attribute("enc").Value);

            if (!string.IsNullOrEmpty(element.GetAttribute("lver")))
                item.VersionLocal = Convert.ToInt32(element.Attribute("lver").Value);

            if (!string.IsNullOrEmpty(element.GetAttribute("latest")))
                item.VersionLatest = Convert.ToInt32(element.Attribute("latest").Value);

            if (!string.IsNullOrEmpty(element.GetAttribute("did")))
                item.DeletionId = Convert.ToInt32(element.Attribute("did").Value);

            item.CheckinDate = DateTime.Parse(element.Attribute("date").Value);

            if (element.Element(XmlNamespaces.GetMessageElementName("IsBranch")) != null &&
                !string.IsNullOrEmpty(element.Element(XmlNamespaces.GetMessageElementName("IsBranch")).Value))
                item.IsBranch = Convert.ToBoolean(element.Element(XmlNamespaces.GetMessageElementName("IsBranch")).Value);

            return item;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("ExtendedItem instance ");
            sb.Append(GetHashCode());

            sb.Append("\n	 LockOwner: ");
            sb.Append(LockOwner);

            sb.Append("\n	 HasOtherPendingChange: ");
            sb.Append(HasOtherPendingChange);

            sb.Append("\n	 ChangeType: ");
            sb.Append(ChangeType);

            sb.Append("\n	 LockStatus: ");
            sb.Append(LockStatus.ToString());

            sb.Append("\n	 VersionLocal: ");
            sb.Append(VersionLocal);

            sb.Append("\n	 VersionLatest: ");
            sb.Append(VersionLatest);

            sb.Append("\n	 Encoding: ");
            sb.Append(Encoding);

            sb.Append("\n	 LocalItem: ");
            sb.Append(LocalItem);

            sb.Append("\n	 TargetServerItem: ");
            sb.Append(TargetServerItem);

            sb.Append("\n	 SourceServerItem: ");
            sb.Append(SourceServerItem);

            sb.Append("\n	 ItemId: ");
            sb.Append(ItemId);

            sb.Append("\n	 ItemType: ");
            sb.Append(ItemType);

            sb.Append("\n	 DeletionId: ");
            sb.Append(DeletionId);

            return sb.ToString();
        }

        public int VersionLatest { get; private set; }

        public int VersionLocal { get; private set; }

        public LockLevel LockStatus { get; private set; }

        public ItemType ItemType { get; private set; }

        public ChangeType ChangeType { get; private set; }

        public int DeletionId { get; private set; }

        public bool HasOtherPendingChange { get; private set; }

        public bool IsInWorkspace { get { return (!string.IsNullOrEmpty(LocalItem)); } }

        public bool IsLatest { get { return VersionLatest == VersionLocal; } }

        public int ItemId { get; private set; }

        public int Encoding { get; private set; }

        public string LocalItem { get; private set; }

        public string LockOwner { get; private set; }

        public string SourceServerItem { get; private set; }

        public string TargetServerItem { get; private set; }

        public DateTime CheckinDate { get; private set; }

        public VersionControlPath ServerPath { get { return TargetServerItem; } }

        public bool IsBranch { get; private set; }
    }
}

