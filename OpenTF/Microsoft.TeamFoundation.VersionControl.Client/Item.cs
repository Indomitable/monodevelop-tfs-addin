//
// Microsoft.TeamFoundation.VersionControl.Client.Item
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Web.Services;

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

		internal Item (VersionControlServer versionControlServer, string serverItem)
		{
			this.versionControlServer = versionControlServer;
			this.serverItem = serverItem;
		}

		public void DownloadFile(string localFileName)
		{
			if (itemType == ItemType.Folder) return;

			Client.DownloadFile.WriteTo(localFileName, VersionControlServer.Repository,
																	ArtifactUri);
		}

		//		<?xml version="1.0" encoding="utf-8"?><soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema"><soap:Body><QueryItemsResponse xmlns="http://schemas.microsoft.com/TeamFoundation/2005/06/VersionControl/ClientServices/03"><QueryItemsResult><ItemSet><QueryPath>$/LSG-1.0</QueryPath><Pattern>*</Pattern><Item cs="7550" date="2006-07-31T18:28:33.933Z" enc="1252" type="File" itemid="41475" item="$/LSG-1.0/configure" hash="KW/KR55BW9qI4Fcg0cEVJQ==" len="146815" durl="sfid=62793,109833,108681,62795,72164,108744,72943,87351,72163,21458&amp;ts=633006048546770184&amp;s=ZNoNAgB8PlCgbs2Lqc4
		//zH21A4SRFW3edqRcbnRKOeC%2F1IZ02f%2FHe8EdYvcU8PNwnBPhBGSGgaKcQWNjPussH3LgjhxQLzxZsfXQWDAllOnbf%2BrOMQYY30SF9e4R4eUjg1wccIkUpkEMOv1edrteyDu5H5ZjISxHQWhLTJ4OyJQ%3D&amp;fid=72164" /><Item cs="7043" date="2006-06-14T20:33:16.16Z" enc="-3" type="Folder" itemid="41609" item="$/LSG-1.0/db" hash="" />
		internal static Item FromXml(Repository repository, XmlReader reader)
		{
			string serverItem = reader.GetAttribute("item");
			
			Item item = new Item(repository.VersionControlServer, serverItem);

			string itype = reader.GetAttribute("type");
			if (!String.IsNullOrEmpty(itype))
				item.itemType = (ItemType) Enum.Parse(typeof(ItemType), itype, true);

			string sdate = reader.GetAttribute("date");
			if (!String.IsNullOrEmpty(sdate))
				item.checkinDate = DateTime.Parse(sdate);

			item.changesetId = Convert.ToInt32(reader.GetAttribute("cs"));
			item.itemId = Convert.ToInt32(reader.GetAttribute("itemid"));
			item.encoding = Convert.ToInt32(reader.GetAttribute("enc"));

			if (item.ItemType == ItemType.File)
				{
					item.contentLength = Convert.ToInt32(reader.GetAttribute("len"));
					item.downloadUrl = reader.GetAttribute("durl");

					string hash = reader.GetAttribute("hash");
					if (!String.IsNullOrEmpty(hash))
						item.hashValue = Convert.FromBase64String(hash);
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
			if (HashValue != null) hash = Convert.ToBase64String(HashValue);
			sb.Append(hash);

			return sb.ToString();
		}

		public int ContentLength
		{
			get { return contentLength; }
		}

		public ItemType ItemType
		{
			get { return itemType; }
		}
		
		public DateTime CheckinDate
		{
			get { return checkinDate; }
		}

		public int ChangesetId
		{
			get { return changesetId; }
		}

		public int DeletionId
		{
			get { return deletionId; }
		}

		public int Encoding
		{
			get { return encoding; }
		}

		public int ItemId
		{
			get { return itemId; }
		}

		public byte[] HashValue
		{
			get { return hashValue; }
		}

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

		public string ServerItem
		{
			get { return serverItem; }
		}

		public VersionControlServer VersionControlServer
		{
			get { return versionControlServer; }
		}

		internal class ItemGenericComparer : IComparer<Item>
		{
			public static ItemGenericComparer Instance = new ItemGenericComparer();

			public int Compare(Item x, Item y)
			{
				return x.ServerItem.CompareTo(y.ServerItem);
			}
		}

		internal static IComparer<Item> GenericComparer {
			get { 
				return ItemGenericComparer.Instance; 
			}
		}

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

		public static IComparer Comparer {
			get { 
				return ItemComparer.Instance; 
			}
		}

	}
}

