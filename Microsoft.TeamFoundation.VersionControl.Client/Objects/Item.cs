//
// Microsoft.TeamFoundation.VersionControl.Client.Item
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
    public sealed class Item: BaseItem
    {
        //<Item cs="1" date="2006-12-15T16:16:26.95Z" enc="-3" type="Folder" itemid="1" item="$/" />
        //<Item cs="30884" date="2012-08-29T15:35:18.273Z" enc="65001" type="File" itemid="189452" item="$/.gitignore" hash="/S3KuHKFNtrxTG7LeQA7LQ==" len="387" />
        internal static Item FromXml(XElement element)
        {
            if (element == null)
                return null;
            Item item = new Item();
            item.ServerItem = element.GetAttribute("item");
            item.ItemType = EnumHelper.ParseItemType(element.GetAttribute("type"));
            item.DeletionId = GeneralHelper.XmlAttributeToInt(element.GetAttribute("did"));
            item.CheckinDate = DateTime.Parse(element.GetAttribute("date"));
            item.ChangesetId = GeneralHelper.XmlAttributeToInt(element.GetAttribute("cs"));
            item.ItemId = GeneralHelper.XmlAttributeToInt(element.GetAttribute("itemid"));
            item.Encoding = GeneralHelper.XmlAttributeToInt(element.GetAttribute("enc"));

            if (!string.IsNullOrEmpty(element.GetAttribute("isbranch")))
            {
                item.IsBranch = GeneralHelper.XmlAttributeToBool(element.GetAttribute("isbranch"));
            }
            if (item.ItemType == ItemType.File)
            {
                item.ContentLength = GeneralHelper.XmlAttributeToInt(element.GetAttribute("len"));
                item.ArtifactUri = element.GetAttribute("durl");
                item.HashValue = GeneralHelper.ToByteArray(element.GetAttribute("hash"));
            }
            return item;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Item instance ");
            sb.Append(GetHashCode());

            sb.Append("\n	 ItemId: ");
            sb.Append(ItemId);

            sb.Append("\n	 CheckinDate: ");
            sb.Append(CheckinDate.ToString("s"));

            sb.Append("\n	 ChangesetId: ");
            sb.Append(ChangesetId);

            sb.Append("\n	 DeletionId: ");
            sb.Append(DeletionId);

            sb.Append("\n	 ItemType: ");
            sb.Append(ItemType);

            sb.Append("\n	 ServerItem: ");
            sb.Append(ServerItem);

            sb.Append("\n	 ContentLength: ");
            sb.Append(ContentLength);

            sb.Append("\n	 Download URL: ");
            sb.Append(ArtifactUri);

            sb.Append("\n	 Hash: ");
            string hash = String.Empty;
            if (HashValue != null)
                hash = Convert.ToBase64String(HashValue);
            sb.Append(hash);

            return sb.ToString();
        }

        public int ContentLength { get; private set; }

        public DateTime CheckinDate { get; private set; }

        public int ChangesetId { get; private set; }

        public int DeletionId { get; private set; }

        public int Encoding { get; private set; }

        public int ItemId { get; private set; }

        public byte[] HashValue { get; private set; }

        public string ArtifactUri { get; private set; }

        public string ServerItem { get; private set; }

        public override VersionControlPath ServerPath { get { return ServerItem; } }

        public string ShortName
        {
            get
            {
                return ServerPath.ItemName;
            }
        }

        public bool IsBranch { get; private set; }
    }
}

