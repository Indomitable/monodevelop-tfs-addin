//
// XmlHelper.cs
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
using Microsoft.TeamFoundation.VersionControl.Client.Enums;

namespace Microsoft.TeamFoundation.VersionControl.Client.Helpers
{
    public static class EnumHelper
    {
        public static ChangeType ParseChangeType(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return ChangeType.None;
            ChangeType changeType;
            if (Enum.TryParse<ChangeType>(value.Replace(" ", ","), true, out changeType))
                return changeType;
            else
                return ChangeType.None;
        }

        public static ItemType ParseItemType(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return ItemType.Any;
            ItemType itemType;
            if (Enum.TryParse<ItemType>(value.Replace(" ", ","), true, out itemType))
                return itemType;
            else
                return ItemType.Any;
        }

        public static ConflictType ParseConflictType(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException("value");
            ConflictType conflictType;
            if (Enum.TryParse<ConflictType>(value.Replace(" ", ","), true, out conflictType))
                return conflictType;
            else
                throw new ArgumentException("Unknown Conflict Type", "value");
        }

        public static RequestType ParseRequestType(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return RequestType.None;
            RequestType requestType;
            if (Enum.TryParse<RequestType>(value.Replace(" ", ","), true, out requestType))
                return requestType;
            else
                return RequestType.None;
        }

        public static LockLevel ParseLockLevel(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return LockLevel.None;
            LockLevel lockType;
            if (Enum.TryParse<LockLevel>(value.Replace(" ", ","), true, out lockType))
                return lockType;
            else
                return LockLevel.None;
        }

        public static SeverityType ParseSeverityType(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException("value");
            SeverityType severityType;
            if (Enum.TryParse<SeverityType>(value.Replace(" ", ","), true, out severityType))
                return severityType;
            else
                throw new ArgumentException("Unknown Severity Type", "value");
        }
    }
}

