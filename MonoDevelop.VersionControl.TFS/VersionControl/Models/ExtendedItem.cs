// ExtendedItem.cs
// 
// Author:
//       Ventsislav Mladenov
// 
// The MIT License (MIT)
// 
// Copyright (c) 2013-2015 Ventsislav Mladenov
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Xml.Linq;
using MonoDevelop.VersionControl.TFS.Helpers;
using MonoDevelop.VersionControl.TFS.VersionControl.Enums;
using MonoDevelop.VersionControl.TFS.VersionControl.Helpers;
using MonoDevelop.VersionControl.TFS.VersionControl.Infrastructure;

namespace MonoDevelop.VersionControl.TFS.VersionControl.Models
{
    sealed class ExtendedItem : BaseItem
    {
        internal static ExtendedItem FromXml(XElement element)
        {
            var item = new ExtendedItem
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

            item.ChangeType = EnumHelper.ParseChangeType(element.GetAttributeValue("chg"));
            item.HasOtherPendingChange = element.GetBooleanAttribute("ochg");
            item.LockStatus = EnumHelper.ParseLockLevel(element.GetAttributeValue("lock"));
            item.LockOwner = element.GetAttributeValue("lowner");
            item.LocalPath = element.GetAttributeValue("local");
            item.TargetServerItem = element.GetAttributeValue("titem");
            item.SourceServerItem = element.GetAttributeValue("sitem");
            item.ItemType = EnumHelper.ParseItemType(element.GetAttributeValue("type"));
            item.ItemId = element.GetIntAttribute("itemid");
            item.Encoding = element.GetIntAttribute("enc");
            item.VersionLocal = element.GetIntAttribute("lver");
            item.VersionLatest = element.GetIntAttribute("latest");
            item.DeletionId = element.GetIntAttribute("did");
            item.CheckinDate = element.GetDateAttribute("date");

            if (element.GetElement("IsBranch") != null &&
                !string.IsNullOrEmpty(element.GetElement("IsBranch").Value))
                item.IsBranch = string.Equals(element.GetElement("IsBranch").Value, "true", StringComparison.OrdinalIgnoreCase);

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

        public bool IsInWorkspace { get { return !LocalPath.IsEmpty; } }

        public bool IsLatest { get { return VersionLatest == VersionLocal; } }

        public int ItemId { get; private set; }

        public int Encoding { get; private set; }

        public LocalPath LocalPath { get; private set; }

        public string LockOwner { get; private set; }

        public string SourceServerItem { get; private set; }

        public string TargetServerItem { get; private set; }

        public DateTime CheckinDate { get; private set; }

        public override RepositoryPath ServerPath { get { return new RepositoryPath(TargetServerItem, ItemType == ItemType.Folder); } }

        public bool IsBranch { get; private set; }
    }
}

