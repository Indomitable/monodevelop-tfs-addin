// RepositoryPath.cs
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
using System.Linq;

namespace MonoDevelop.VersionControl.TFS.VersionControl.Infrastructure
{
    sealed class RepositoryPath : BasePath, IEquatable<RepositoryPath>, IComparable<RepositoryPath>
    {
        private const string RootFolder = "$";
        public const char Separator = '/';

        private string[] GetPathParts()
        {
            return this.Path.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries);;
        }

        public RepositoryPath(string path, bool isFolder)
        {
            if (!IsServerItem(path))
                throw new Exception("Not a server path");
            this.Path = path.TrimEnd(Separator);
            if (isFolder)
                this.Path += Separator;
        }

        public static bool TryGet(string path, bool isFolder, out RepositoryPath result)
        {
            if (!IsServerItem(path))
            {
                result = null;
                return false;
            }
            result = new RepositoryPath(path, isFolder);
            return true;
        }

        public static RepositoryPath RootPath
        {
            get
            {
                return new RepositoryPath(RootFolder, true);
            }
        }

        public string ItemName
        {
            get
            {
                var parts = GetPathParts();
                return parts[parts.Length - 1];
            }
        }

        public bool IsRoot
        {
            get
            {
                return string.Equals(ItemName, RootFolder, StringComparison.Ordinal);
            }
        }

        public override bool IsDirectory
        {
            get { return this.Path.EndsWith(Separator.ToString()); }
        }

        public static bool IsServerItem(string path)
        {
            return path.StartsWith(RootFolder, StringComparison.Ordinal);
        }

        public RepositoryPath ParentPath
        {
            get
            {
                if (IsRoot)
                    return null;
                var parts = GetPathParts();
                var parentPath = string.Join(Separator.ToString(), parts.Take(parts.Length - 1));
                return new RepositoryPath(parentPath, true);
            }
        }
        
        public override string ToString()
        {
            return this.Path;
        }

        public bool IsChildOrEqualOf(RepositoryPath other)
        {
            if (other == null)
                return false;
            if (this == other)
                return true;
            //Could not be a child of file.
            if (!other.IsDirectory)
                return false;
            
            return this.Path.StartsWith(other.Path, StringComparison.OrdinalIgnoreCase);
        }

        public string ToRelativeOf(RepositoryPath basePath)
        {
            if (!IsChildOrEqualOf(basePath))
                throw new Exception("Could not convert to relative path!");
            return this.Path.Substring(basePath.Path.Length);
        }

        #region Equal

        #region IComparable<VersionControlPath> Members

        public int CompareTo(RepositoryPath other)
        {
            return string.Compare(Path, other.Path, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region IEquatable<VersionControlPath> Members

        public bool Equals(RepositoryPath other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return string.Equals(other.Path, Path, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            RepositoryPath cast = obj as RepositoryPath;
            if (cast == null)
                return false;
            return Equals(cast);
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }

        public static bool operator ==(RepositoryPath left, RepositoryPath right)
        {
            return ReferenceEquals(null, left) ? ReferenceEquals(null, right) : left.Equals(right);
        }

        public static bool operator !=(RepositoryPath left, RepositoryPath right)
        {
            return !(left == right);
        }

        #endregion Equal
    }
}
