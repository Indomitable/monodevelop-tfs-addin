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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
    public sealed class Item
    {
        private string downloadUrl;
        private int contentLength;
        private int changesetId;
        private int deletionId;
        private int encoding;
        private byte[] hashValue;
        private int itemId;
        private DateTime checkinDate;
        private ItemType itemType = ItemType.File;
        private string serverItem;
        private VersionControlServer versionControlServer;

        internal Item(VersionControlServer versionControlServer, string serverItem)
        {
            this.versionControlServer = versionControlServer;
            this.serverItem = serverItem;
        }

        public void DownloadFile(string localFileName)
        {
            if (itemType == ItemType.Folder)
                return;

            Client.DownloadFile.WriteTo(localFileName, VersionControlServer.Repository,
                ArtifactUri);
        }
        //<Item cs="1" date="2006-12-15T16:16:26.95Z" enc="-3" type="Folder" itemid="1" item="$/" />
        internal static Item FromXml(Repository repository, XElement element)
        {
            if (element == null)
                return null;
            string serverItem = element.Attribute("item").Value;
            Item item = new Item(repository.VersionControlServer, serverItem);

            if (element.Attribute("type") != null && !string.IsNullOrEmpty(element.Attribute("type").Value))
                item.itemType = (ItemType)Enum.Parse(typeof(ItemType), element.Attribute("type").Value, true);

            if (element.Attribute("date") != null && !string.IsNullOrEmpty(element.Attribute("date").Value))
                item.checkinDate = DateTime.Parse(element.Attribute("date").Value);

            item.changesetId = Convert.ToInt32(element.Attribute("cs").Value);
            item.itemId = Convert.ToInt32(element.Attribute("itemid").Value);
            item.encoding = Convert.ToInt32(element.Attribute("enc").Value);

            if (item.ItemType == ItemType.File)
            {
                item.contentLength = Convert.ToInt32(element.Attribute("len").Value);
                item.downloadUrl = element.Attribute("durl").Value;

                if (element.Attribute("hash") != null && !string.IsNullOrEmpty(element.Attribute("hash").Value))
                    item.hashValue = Convert.FromBase64String(element.Attribute("hash").Value);
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
            sb.Append(downloadUrl);

            sb.Append("\n	 Hash: ");
            string hash = String.Empty;
            if (HashValue != null)
                hash = Convert.ToBase64String(HashValue);
            sb.Append(hash);

            return sb.ToString();
        }

        public int ContentLength { get { return contentLength; } }

        public ItemType ItemType { get { return itemType; } }

        public DateTime CheckinDate { get { return checkinDate; } }

        public int ChangesetId { get { return changesetId; } }

        public int DeletionId { get { return deletionId; } }

        public int Encoding { get { return encoding; } }

        public int ItemId { get { return itemId; } }

        public byte[] HashValue { get { return hashValue; } }

        public Uri ArtifactUri
        {
            get
            {
                if (String.IsNullOrEmpty(downloadUrl))
                {
                    Item item = VersionControlServer.GetItem(ItemId, ChangesetId, true);
                    downloadUrl = item.downloadUrl;
                }

                return new Uri(String.Format("{0}?{1}", VersionControlServer.Repository.ItemUrl, downloadUrl));
            }
        }

        public string ServerItem { get { return serverItem; } }

        public VersionControlServer VersionControlServer { get { return versionControlServer; } }

        internal class ItemGenericComparer : IComparer<Item>
        {
            public static ItemGenericComparer Instance = new ItemGenericComparer();

            public int Compare(Item x, Item y)
            {
                return x.ServerItem.CompareTo(y.ServerItem);
            }
        }

        internal static IComparer<Item> GenericComparer { get { return ItemGenericComparer.Instance; } }

        internal class ItemComparer : IComparer
        {
            public static ItemComparer Instance = new ItemComparer();

            public int Compare(object x, object y)
            {
                Item itemX = (Item)x;
                Item itemY = (Item)y;
                return itemX.ServerItem.CompareTo(itemY.ServerItem);
            }
        }

        public static IComparer Comparer { get { return ItemComparer.Instance; } }
    }
}

