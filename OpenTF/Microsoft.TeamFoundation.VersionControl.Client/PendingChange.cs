//
// Microsoft.TeamFoundation.VersionControl.Client.PendingChange
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
using System.IO;
using System.Text;
using System.Xml;
using System.Security.Cryptography;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
	public class PendingChange
	{
		private ChangeType changeType;
		private VersionControlServer versionControlServer;
		private string serverItem;
		private string localItem;
		private int encoding;
		private int itemId;
		private DateTime creationDate;
		private byte[] uploadHashValue;
		private string downloadUrl;
		private ItemType itemType = ItemType.Any;

		//		<PendingChange chg="Add Edit Encoding" hash="" uhash="" pcid="-339254" />

			internal static PendingChange FromXml(Repository repository, XmlReader reader)
		{
			PendingChange change = new PendingChange();
			change.versionControlServer = repository.VersionControlServer;

			change.serverItem = reader.GetAttribute("item");
			change.localItem = TfsPath.ToPlatformPath(reader.GetAttribute("local"));
			change.itemId = Convert.ToInt32(reader.GetAttribute("itemid"));
			change.encoding = Convert.ToInt32(reader.GetAttribute("enc"));
			change.creationDate = DateTime.Parse(reader.GetAttribute("date"));

			string itype = reader.GetAttribute("type");
			if (!String.IsNullOrEmpty(itype))
					change.itemType = (ItemType) Enum.Parse(typeof(ItemType), itype, true);

			change.downloadUrl = reader.GetAttribute("durl");
			
			string chgAttr = reader.GetAttribute("chg");
			change.changeType = (ChangeType) Enum.Parse(typeof(ChangeType), chgAttr.Replace(" ", ","), true);
			if (change.changeType == ChangeType.Edit) change.itemType = ItemType.File;

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
			sb.Append(downloadUrl);

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

		public void DownloadBaseFile(string localFileName)
		{
			if (itemType == ItemType.Folder) return;
			Uri artifactUri = new Uri(String.Format("{0}?{1}", VersionControlServer.Repository.ItemUrl, downloadUrl));
			//Console.WriteLine(ToString());
			//Console.WriteLine(artifactUri.ToString());
			//Console.WriteLine("---------------------------------------------------");
			Client.DownloadFile.WriteTo(localFileName, VersionControlServer.Repository,
																	artifactUri);
		}

		public byte[] UploadHashValue
		{
			get {
				if (uploadHashValue == null) UpdateUploadHashValue();
				return uploadHashValue; 
			}
		}

		public DateTime CreationDate
		{
			get { return creationDate; }
		}

		public int Encoding
		{
			get { return encoding; }
		}

		public string LocalItem
		{
			get { return localItem; }
		}

		public int ItemId
		{
			get { return itemId; }
		}

		public ItemType ItemType
		{
			get { return itemType; }
		}
 
		public bool IsAdd
		{
			get { return (changeType & ChangeType.Add) == ChangeType.Add; }
		}

		public bool IsBranch
		{
			get { return (changeType & ChangeType.Branch) == ChangeType.Branch; }
		}

		public bool IsDelete
		{
			get { return (changeType & ChangeType.Delete) == ChangeType.Delete; }
		}

		public bool IsEdit
		{
			get { return (changeType & ChangeType.Edit) == ChangeType.Edit; }
		}

		public bool IsEncoding
		{
			get { return (changeType & ChangeType.Encoding) == ChangeType.Encoding; }
		}

		public bool IsLock
		{
			get { return (changeType & ChangeType.Lock) == ChangeType.Lock; }
		}

		public bool IsMerge
		{
			get { return (changeType & ChangeType.Merge) == ChangeType.Merge; }
		}

		public bool IsRename
		{
			get { return (changeType & ChangeType.Rename) == ChangeType.Rename; }
		}

		public ChangeType ChangeType
		{
			get { return changeType; }
		}

		public string ServerItem
		{
			get { return serverItem; }
		}

		public VersionControlServer VersionControlServer
		{
			get { return versionControlServer; }
		}

		static public string GetLocalizedStringForChangeType(ChangeType changeType)
		{
			return changeType.ToString();
		}
	}
}

