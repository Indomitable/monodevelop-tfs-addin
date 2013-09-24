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
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Web.Services;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
	public sealed class ExtendedItem
	{
		private ChangeType changeType = ChangeType.None;
		private int deletionId;
		private int encoding;
		private int itemId;
		private string localItem;
		private string sourceServerItem;
		private string targetServerItem;
		private string lockOwner;
		private bool hasOtherPendingChange;
		private ItemType itemType;
		private LockLevel lockStatus;
		private int versionLatest;
		private int versionLocal;

		//<ExtendedItem lver="int" did="int" latest="int" 
		//type="Any or Folder or File" enc="int" itemid="int" 
		//local="string" titem="string" sitem="string" 
		//chg="None or Add or Edit or Encoding or Rename or Delete or Undelete or Branch or Merge or Lock" 
		//ochg="boolean" lock="None or Checkin or CheckOut or Unchanged" 
		//lowner="string" />

		internal static ExtendedItem FromXml(Repository repository, XmlReader reader)
		{
			ExtendedItem item = new ExtendedItem();

			string chg = reader.GetAttribute("chg");
			if (!String.IsNullOrEmpty(chg)) item.changeType = (ChangeType) Enum.Parse(typeof(ChangeType), chg, true);

			string ochg = reader.GetAttribute("ochg");
			if (!String.IsNullOrEmpty(ochg)) item.hasOtherPendingChange = bool.Parse(ochg);

			string xlock = reader.GetAttribute("lock");
			if (!String.IsNullOrEmpty(xlock)) item.lockStatus = (LockLevel) Enum.Parse(typeof(LockLevel), xlock, true);

			item.lockOwner = reader.GetAttribute("lowner");
			item.localItem = TfsPath.ToPlatformPath(reader.GetAttribute("local"));
			item.targetServerItem = reader.GetAttribute("titem");
			item.sourceServerItem = reader.GetAttribute("sitem");

			item.itemType = (ItemType) Enum.Parse(typeof(ItemType), reader.GetAttribute("type"), true);
			item.itemId = Convert.ToInt32(reader.GetAttribute("itemid"));
			item.encoding = Convert.ToInt32(reader.GetAttribute("enc"));
			item.versionLocal = Convert.ToInt32(reader.GetAttribute("lver"));
			item.versionLatest = Convert.ToInt32(reader.GetAttribute("latest"));
			item.deletionId = Convert.ToInt32(reader.GetAttribute("did"));

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

		public int VersionLatest
		{
			get { return versionLatest; }
		}

		public int VersionLocal
		{
			get { return versionLocal; }
		}

		public LockLevel LockStatus
		{
			get { return lockStatus; }
		}

		public ItemType ItemType
		{
			get { return itemType; }
		}

		public ChangeType ChangeType
		{
			get { return changeType; }
		}

		public int DeletionId
		{
			get { return deletionId; }
		}

		public bool HasOtherPendingChange
		{
			get { return hasOtherPendingChange; }
		}

		public bool IsInWorkspace
		{
			get { return (!String.IsNullOrEmpty(LocalItem)); }
		}

		public bool IsLatest
		{
			get { return versionLatest == versionLocal; }
		}

		public int ItemId
		{
			get { return itemId; }
		}

		public int Encoding
		{
			get { return encoding; }
		}

		public string LocalItem
		{
			get { return localItem; }
		}

		public string LockOwner
		{
			get { return lockOwner; }
		}

		public string SourceServerItem
		{
			get { return sourceServerItem; }
		}

		public string TargetServerItem
		{
			get { return targetServerItem; }
		}

	}
}

