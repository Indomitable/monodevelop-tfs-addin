//
// Microsoft.TeamFoundation.VersionControl.Common.VersionControlPath
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
using System.Linq;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
    public sealed class VersionControlPath : IEquatable<VersionControlPath>, IComparable<VersionControlPath>
    {
        public static readonly string RootFolder = "$/";
        public const char Separator = '/';
        private readonly string[] pathParts;
        private readonly string path;

        public VersionControlPath ParentPath
        { 
            get
            {
                if (this == RootFolder)
                    return null;
                string[] parentPath = new string[pathParts.Length - 1]; 
                Array.Copy(pathParts, 0, parentPath, 0, pathParts.Length - 1);
                return RootFolder + string.Join(Separator.ToString(), parentPath); 
            }
        }

        public VersionControlPath(string path)
        {
            if (!IsServerItem(path))
                throw new Exception("Not a server path");
            this.path = path;
            if (!string.Equals(path, RootFolder, StringComparison.Ordinal))
                this.pathParts = path.Split(Separator).Skip(1).ToArray();
            else
                this.pathParts = new string[0];
        }

        public string ItemName
        {
            get
            {
                if (IsRoot)
                    return VersionControlPath.RootFolder;
                return pathParts[pathParts.Length - 1];
            }
        }

        public bool IsRoot
        {
            get
            {
                return string.Equals(path, RootFolder);
            }
        }

        public static bool IsServerItem(string path)
        {
            return path.StartsWith(RootFolder, StringComparison.Ordinal);
        }

        public static implicit operator VersionControlPath(string path)
        {
            return new VersionControlPath(path);
        }

        public static implicit operator string(VersionControlPath path)
        {
            return path.path;
        }

        public override string ToString()
        {
            return this.path;
        }

        public bool IsChildOrEqualTo(VersionControlPath other)
        {
            if (other == null)
                return false;
            if (other == RootFolder)
                return true;
            if (other == this)
                return true;
            bool isChild = true;
            for (int i = 0; i < other.pathParts.Length; i++)
            {
                if (i > this.pathParts.Length - 1) //This could be a parent if has more items.
                {
                    isChild = false;
                    break;
                }
                var thisPart = this.pathParts[i];
                var otherPart = other.pathParts[i];
                if (!string.Equals(thisPart, otherPart, StringComparison.OrdinalIgnoreCase))
                {
                    isChild = false;
                    break;
                }
            }
            return isChild;
        }

        public string ChildPart(VersionControlPath parent)
        {
            if (parent == null)
                throw new ArgumentNullException("parent");
            if (!IsChildOrEqualTo(parent))
                throw new Exception("Not a child");
            if (this == parent)
                return string.Empty;
            return string.Join(Separator.ToString(), this.pathParts.Skip(parent.pathParts.Length));
        }

        #region Equal

        #region IComparable<VersionControlPath> Members

        public int CompareTo(VersionControlPath other)
        {
            return string.Compare(path, other.path, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region IEquatable<VersionControlPath> Members

        public bool Equals(VersionControlPath other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return string.Equals(other.path, path, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            VersionControlPath cast = obj as VersionControlPath;
            if (cast == null)
                return false;
            return Equals(cast);
        }

        public override int GetHashCode()
        {
            return path.GetHashCode();
        }

        public static bool operator ==(VersionControlPath left, VersionControlPath right)
        {
            return ReferenceEquals(null, left) ? ReferenceEquals(null, right) : left.Equals(right);
        }

        public static bool operator !=(VersionControlPath left, VersionControlPath right)
        {
            return !(left == right);
        }

        #endregion Equal
    }
}
