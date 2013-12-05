//
// Microsoft.TeamFoundation.VersionControl.Client.PendingChange
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
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Xml.Linq;
using Microsoft.TeamFoundation.VersionControl.Client.Enums;
using Microsoft.TeamFoundation.VersionControl.Client.Helpers;

namespace Microsoft.TeamFoundation.VersionControl.Client.Objects
{
    //<s:complexType name="PendingChange">
    //    <s:sequence>
    //        <s:element minOccurs="0" maxOccurs="1" name="MergeSources" type="tns:ArrayOfMergeSource"/>
    //        <s:element minOccurs="0" maxOccurs="1" name="PropertyValues" type="tns:ArrayOfPropertyValue"/>
    //    </s:sequence>
    //    <s:attribute default="0" name="chgEx" type="s:int"/>
    //    <s:attribute default="None" name="chg" type="tns:ChangeType"/>
    //    <s:attribute name="date" type="s:dateTime" use="required"/>
    //    <s:attribute default="0" name="did" type="s:int"/>
    //    <s:attribute default="Any" name="type" type="tns:ItemType"/>
    //    <s:attribute default="-2" name="enc" type="s:int"/>
    //    <s:attribute default="0" name="itemid" type="s:int"/>
    //    <s:attribute name="local" type="s:string"/>
    //    <s:attribute default="None" name="lock" type="tns:LockLevel"/>
    //    <s:attribute name="item" type="s:string"/>
    //    <s:attribute name="srclocal" type="s:string"/>
    //    <s:attribute name="srcitem" type="s:string"/>
    //    <s:attribute default="0" name="svrfm" type="s:int"/>
    //    <s:attribute default="0" name="sdi" type="s:int"/>
    //    <s:attribute default="0" name="ver" type="s:int"/>
    //    <s:attribute name="hash" type="s:base64Binary"/>
    //    <s:attribute default="-1" name="len" type="s:long"/>
    //    <s:attribute name="uhash" type="s:base64Binary"/>
    //    <s:attribute default="0" name="pcid" type="s:int"/>
    //    <s:attribute name="durl" type="s:string"/>
    //    <s:attribute name="shelvedurl" type="s:string"/>
    //    <s:attribute name="ct" type="s:int" use="required"/>
    //</s:complexType>
    public class PendingChange
    {
        internal static PendingChange FromXml(XElement element)
        {
            PendingChange change = new PendingChange();
            change.ServerItem = element.GetAttribute("item");
            change.LocalItem = TfsPath.ToPlatformPath(element.GetAttribute("local"));
            change.ItemId = GeneralHelper.XmlAttributeToInt(element.GetAttribute("itemid"));
            change.Encoding = GeneralHelper.XmlAttributeToInt(element.GetAttribute("enc"));
            change.Version = GeneralHelper.XmlAttributeToInt(element.GetAttribute("ver"));
            change.CreationDate = DateTime.Parse(element.GetAttribute("date"));
            change.Hash = GeneralHelper.ToByteArray(element.GetAttribute("hash"));
            change.uploadHashValue = GeneralHelper.ToByteArray(element.GetAttribute("uhash"));
            change.ItemType = EnumHelper.ParseItemType(element.GetAttribute("type"));
            change.DownloadUrl = element.GetAttribute("durl");
            change.ChangeType = EnumHelper.ParseChangeType(element.GetAttribute("chg"));
            if (change.ChangeType == ChangeType.Edit)
                change.ItemType = ItemType.File;
            return change;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("PendingChange instance ");
            sb.Append(GetHashCode());

            sb.Append("\n	 ServerItem: ");
            sb.Append(ServerItem);

            sb.Append("\n	 LocalItem: ");
            sb.Append(LocalItem);

            sb.Append("\n	 ItemId: ");
            sb.Append(ItemId);

            sb.Append("\n	 Encoding: ");
            sb.Append(Encoding);

            sb.Append("\n	 Creation Date: ");
            sb.Append(CreationDate);

            sb.Append("\n	 ChangeType: ");
            sb.Append(ChangeType);

            sb.Append("\n	 ItemType: ");
            sb.Append(ItemType);

            sb.Append("\n	 Download URL: ");
            sb.Append(DownloadUrl);

            return sb.ToString();
        }

        internal void UpdateUploadHashValue()
        {
            using (FileStream stream = new FileStream(LocalItem, FileMode.Open, FileAccess.Read))
            {
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                md5.ComputeHash(stream);
                uploadHashValue = md5.Hash;
            }
        }

        public byte[] Hash { get; set; }

        private byte[] uploadHashValue;

        public byte[] UploadHashValue
        {
            get
            {
                if (uploadHashValue == null)
                    UpdateUploadHashValue();
                return uploadHashValue; 
            }
        }

        public DateTime CreationDate { get; private set; }

        public int Encoding { get; private set; }

        public string LocalItem { get; private set; }

        public int ItemId { get; private set; }

        public ItemType ItemType { get; private set; }

        public int Version { get; private set; }

        public bool IsAdd
        {
            get { return ChangeType.HasFlag(ChangeType.Add); }
        }

        public bool IsBranch
        {
            get { return ChangeType.HasFlag(ChangeType.Branch); }
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

        public bool IsLock
        {
            get { return ChangeType.HasFlag(ChangeType.Lock); }
        }

        public bool IsMerge
        {
            get { return ChangeType.HasFlag(ChangeType.Merge); }
        }

        public bool IsRename
        {
            get { return ChangeType.HasFlag(ChangeType.Rename); }
        }

        public ChangeType ChangeType { get; private set; }

        public string ServerItem { get; private set; }

        public string DownloadUrl { get; set; }

        static public string GetLocalizedStringForChangeType(ChangeType changeType)
        {
            return changeType.ToString();
        }
    }
}

