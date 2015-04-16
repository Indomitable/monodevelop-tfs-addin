// Conflict.cs
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

using System.Xml.Linq;
using MonoDevelop.VersionControl.TFS.Helpers;
using MonoDevelop.VersionControl.TFS.VersionControl.Enums;
using MonoDevelop.VersionControl.TFS.VersionControl.Helpers;
using MonoDevelop.VersionControl.TFS.VersionControl.Infrastructure;

namespace MonoDevelop.VersionControl.TFS.VersionControl.Models
{
    sealed class Conflict
    {
        internal static Conflict FromXml(XElement element)
        {
            Conflict conflict = new Conflict();
            conflict.ConflictId = element.GetIntAttribute("cid");
            conflict.PendingChangeId = element.GetIntAttribute("pcid");
            //Your
            conflict.YourChangeType = EnumHelper.ParseChangeType(element.GetAttributeValue("ychg"));
            conflict.YourServerItem = element.GetAttributeValue("ysitem");
            conflict.YourServerItemSource = element.GetAttributeValue("ysitemsrc");
            conflict.YourEncoding = element.GetIntAttribute("yenc");
            conflict.YourItemType = EnumHelper.ParseItemType(element.GetAttributeValue("ytype"));
            conflict.YourVersion = element.GetIntAttribute("yver");
            conflict.YourItemId = element.GetIntAttribute("yitemid");
            conflict.YourDeletionId = element.GetIntAttribute("ydid");
            conflict.YourLocalChangeType = EnumHelper.ParseChangeType(element.GetAttributeValue("ylchg"));
            conflict.YourLastMergedVersion = element.GetIntAttribute("ylmver");
            //Base
            conflict.BaseServerItem = element.GetAttributeValue("bsitem");
            conflict.BaseEncoding = element.GetIntAttribute("benc");
            conflict.BaseItemId = element.GetIntAttribute("bitemid");
            conflict.BaseVersion = element.GetIntAttribute("bver");
            conflict.BaseHashValue = element.GetByteArrayAttribute("bhash");
            conflict.BaseDeletionId = element.GetIntAttribute("bdid");
            conflict.BaseItemType = EnumHelper.ParseItemType(element.GetAttributeValue("btype"));
            conflict.BaseChangeType = EnumHelper.ParseChangeType(element.GetAttributeValue("bchg"));
            //Their
            conflict.TheirItemId = element.GetIntAttribute("titemid");
            conflict.TheirVersion = element.GetIntAttribute("tver");
            conflict.TheirServerItem = element.GetAttributeValue("tsitem");
            conflict.TheirEncoding = element.GetIntAttribute("tenc");
            conflict.TheirHashValue = element.GetByteArrayAttribute("thash");
            conflict.TheirDeletionId = element.GetIntAttribute("tdid");
            conflict.TheirItemType = EnumHelper.ParseItemType(element.GetAttributeValue("ttype"));
            conflict.TheirLastMergedVersion = element.GetIntAttribute("tlmver");
            conflict.SourceLocalItem = element.GetAttributeValue("srclitem");
            conflict.TargetLocalItem = element.GetAttributeValue("tgtlitem");

            conflict.ConflictType = EnumHelper.ParseConflictType(element.GetAttributeValue("ctype"));
            conflict.Reason = element.GetIntAttribute("reason");
            conflict.IsNamespaceConflict = element.GetBooleanAttribute("isnamecflict");
            conflict.IsForced = element.GetBooleanAttribute("isforced");
            conflict.IsResolved = element.GetBooleanAttribute("tgtlitem");

            conflict.BaseDowloadUrl = element.GetAttributeValue("bdurl");
            conflict.TheirDowloadUrl = element.GetAttributeValue("tdurl");
            conflict.YourDowloadUrl = element.GetAttributeValue("ydurl");

            return conflict;
        }

        public int ConflictId { get; private set; }

        public int PendingChangeId { get; private set; }

        public ChangeType YourChangeType { get; private set; }

        public string YourServerItem { get; private set; }

        public string YourServerItemSource { get; private set; }

        public int YourEncoding { get; private set; }

        public ItemType YourItemType { get; private set; }

        public int YourVersion { get; private set; }

        public int YourItemId { get; private set; }

        public int YourDeletionId { get; private set; }

        public ChangeType YourLocalChangeType { get; private set; }

        public int YourLastMergedVersion { get; private set; }

        public string BaseServerItem { get; private set; }

        public int BaseEncoding { get; private set; }

        public int BaseItemId { get; private set; }

        public int BaseVersion { get; private set; }

        public byte[] BaseHashValue { get; private set; }

        public int BaseDeletionId { get; private set; }

        public ItemType BaseItemType { get; private set; }

        public ChangeType BaseChangeType { get; set; }

        public int TheirItemId { get; private set; }

        public int TheirVersion { get; private set; }

        public string TheirServerItem { get; private set; }

        public int TheirEncoding { get; private set; }

        public byte[] TheirHashValue { get; private set; }

        public int TheirDeletionId { get; private set; }

        public ItemType TheirItemType { get; private set; }

        public int TheirLastMergedVersion { get; private set; }

        public LocalPath SourceLocalItem { get; private set; }

        public LocalPath TargetLocalItem { get; private set; }

        public ConflictType ConflictType { get; private set; }

        public int Reason { get; private set; }

        public bool IsNamespaceConflict { get; private set; }

        public bool IsForced { get; private set; }

        public bool IsResolved { get; private set; }

        public string BaseDowloadUrl { get; private set; }

        public string TheirDowloadUrl { get; private set; }

        public string YourDowloadUrl { get; private set; }
    }
}

