//
// Conflict.cs
//
// Author:
//       Ventsislav Mladenov <vmladenov.mladenov@gmail.com>
//
// Copyright (c) 2013 Ventsislav Mladenov
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
using Microsoft.TeamFoundation.VersionControl.Client.Helpers;
using Microsoft.TeamFoundation.VersionControl.Client.Enums;

namespace Microsoft.TeamFoundation.VersionControl.Client.Objects
{
    public class Conflict
    {
        internal static Conflict FromXml(XElement element, Workspace workspace)
        {
            Conflict conflict = new Conflict();
            conflict.ConflictId = GeneralHelper.XmlAttributeToInt(element.GetAttribute("cid"));
            conflict.PendingChangeId = GeneralHelper.XmlAttributeToInt(element.GetAttribute("pcid"));
            //Your
            conflict.YourChangeType = EnumHelper.ParseChangeType(element.GetAttribute("ychg"));
            conflict.YourServerItem = element.GetAttribute("ysitem");
            conflict.YourServerItemSource = element.GetAttribute("ysitemsrc");
            conflict.YourEncoding = GeneralHelper.XmlAttributeToInt(element.GetAttribute("yenc"));
            conflict.YourItemType = EnumHelper.ParseItemType(element.GetAttribute("ytype"));
            conflict.YourVersion = GeneralHelper.XmlAttributeToInt(element.GetAttribute("yver"));
            conflict.YourItemId = GeneralHelper.XmlAttributeToInt(element.GetAttribute("yitemid"));
            conflict.YourDeletionId = GeneralHelper.XmlAttributeToInt(element.GetAttribute("ydid"));
            conflict.YourLocalChangeType = EnumHelper.ParseChangeType(element.GetAttribute("ylchg"));
            conflict.YourLastMergedVersion = GeneralHelper.XmlAttributeToInt(element.GetAttribute("ylmver"));
            //Base
            conflict.BaseServerItem = element.GetAttribute("bsitem");
            conflict.BaseEncoding = GeneralHelper.XmlAttributeToInt(element.GetAttribute("benc"));
            conflict.BaseItemId = GeneralHelper.XmlAttributeToInt(element.GetAttribute("bitemid"));
            conflict.BaseVersion = GeneralHelper.XmlAttributeToInt(element.GetAttribute("bver"));
            conflict.BaseHashValue = GeneralHelper.ToByteArray(element.GetAttribute("bhash"));
            conflict.BaseDeletionId = GeneralHelper.XmlAttributeToInt(element.GetAttribute("bdid"));
            conflict.BaseItemType = EnumHelper.ParseItemType(element.GetAttribute("btype"));
            conflict.BaseChangeType = EnumHelper.ParseChangeType(element.GetAttribute("bchg"));
            //Their
            conflict.TheirItemId = GeneralHelper.XmlAttributeToInt(element.GetAttribute("titemid"));
            conflict.TheirVersion = GeneralHelper.XmlAttributeToInt(element.GetAttribute("tver"));
            conflict.TheirServerItem = element.GetAttribute("tsitem");
            conflict.TheirEncoding = GeneralHelper.XmlAttributeToInt(element.GetAttribute("tenc"));
            conflict.TheirHashValue = GeneralHelper.ToByteArray(element.GetAttribute("thash"));
            conflict.TheirDeletionId = GeneralHelper.XmlAttributeToInt(element.GetAttribute("tdid"));
            conflict.TheirItemType = EnumHelper.ParseItemType(element.GetAttribute("ttype"));
            conflict.TheirLastMergedVersion = GeneralHelper.XmlAttributeToInt(element.GetAttribute("tlmver"));
            conflict.SourceLocalItem = TfsPath.ToPlatformPath(element.GetAttribute("srclitem"));
            conflict.TargetLocalItem = TfsPath.ToPlatformPath(element.GetAttribute("tgtlitem"));

            conflict.ConflictType = EnumHelper.ParseConflictType(element.GetAttribute("ctype"));
            conflict.Reason = GeneralHelper.XmlAttributeToInt(element.GetAttribute("reason"));
            conflict.IsNamespaceConflict = GeneralHelper.XmlAttributeToBool(element.GetAttribute("isnamecflict"));
            conflict.IsForced = GeneralHelper.XmlAttributeToBool(element.GetAttribute("isforced"));
            conflict.IsResolved = GeneralHelper.XmlAttributeToBool(element.GetAttribute("tgtlitem"));

            conflict.BaseDowloadUrl = element.GetAttribute("bdurl");
            conflict.TheirDowloadUrl = element.GetAttribute("tdurl");
            conflict.YourDowloadUrl = element.GetAttribute("ydurl");

            conflict.Workspace = workspace;
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

        public string SourceLocalItem { get; private set; }

        public string TargetLocalItem { get; private set; }

        public ConflictType ConflictType { get; private set; }

        public int Reason { get; private set; }

        public bool IsNamespaceConflict { get; private set; }

        public bool IsForced { get; private set; }

        public bool IsResolved { get; private set; }

        public string BaseDowloadUrl { get; private set; }

        public string TheirDowloadUrl { get; private set; }

        public string YourDowloadUrl { get; private set; }

        public Workspace Workspace { get; set; }
    }
}

