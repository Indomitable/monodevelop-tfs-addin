//
// IItem.cs
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
using Microsoft.TeamFoundation.VersionControl.Client.Enums;
using System;

namespace Microsoft.TeamFoundation.VersionControl.Client.Objects
{
    public abstract class BaseItem : IEquatable<BaseItem>, IComparable<BaseItem>
    {
        public abstract VersionControlPath ServerPath { get; }

        public ItemType ItemType { get; protected set; }

        #region Equal

        #region IComparable<IItem> Members

        public int CompareTo(BaseItem other)
        {
            return ServerPath.CompareTo(other.ServerPath);
        }

        #endregion

        #region IEquatable<IItem> Members

        public bool Equals(BaseItem other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return other.ServerPath == ServerPath;
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            BaseItem cast = obj as BaseItem;
            if (cast == null)
                return false;
            return Equals(cast);
        }

        public override int GetHashCode()
        {
            return ServerPath.GetHashCode();
        }

        public static bool operator ==(BaseItem left, BaseItem right)
        {
            return ReferenceEquals(null, left) ? ReferenceEquals(null, right) : left.Equals(right);
        }

        public static bool operator !=(BaseItem left, BaseItem right)
        {
            return !(left == right);
        }

        #endregion Equal
    }
}

