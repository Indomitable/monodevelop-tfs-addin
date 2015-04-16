// LocalPath.cs
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
using System.IO;
using System.Text.RegularExpressions;
using MonoDevelop.VersionControl.TFS.VersionControl.Helpers;

namespace MonoDevelop.VersionControl.TFS.VersionControl.Infrastructure
{
    internal sealed class LocalPath : BasePath, IEquatable<LocalPath>, IComparable<LocalPath>
    {
        public LocalPath(string localPath)
        {
            this.Path = !string.IsNullOrWhiteSpace(localPath) ? localPath.TrimEnd(System.IO.Path.DirectorySeparatorChar) : string.Empty;
            //When runing linux we should check formating. If localPath is comming from server the it will be in format U:\SomeFolder\...
            //We have to convert it to /SomeFolder/...
            if (EnvironmentHelper.IsRunningOnUnix)
            {
                var regEx = new Regex("^[a-zA-Z]:\\\\");
                if (regEx.IsMatch(this.Path))
                {
                    this.Path = this.Path.Remove(0, 2).Replace('\\', '/'); //Remove Drive letter and convert slashes
                }
            }
         }

        private StringComparison CompareType
        {
            get
            {
                return EnvironmentHelper.IsRunningOnUnix ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            }
        }

        public override bool IsDirectory
        {
            get { return Directory.Exists(Path); }
        }

        public bool IsEmpty
        {
            get { return string.IsNullOrEmpty(Path); }
        }

        public bool IsChildOrEqualOf(LocalPath other)
        {
            if (other.IsEmpty)
                return false;
            if (this == other)
                return true;
            return this.Path.StartsWith(other.Path + System.IO.Path.DirectorySeparatorChar, CompareType);
        }

        public string ToRelativeOf(LocalPath basePath)
        {
            if (!IsChildOrEqualOf(basePath))
                throw new Exception("Could not convert to relative path!");
            if (this == basePath)
                return string.Empty;
            return this.Path.Substring(basePath.Path.Length + 1); //Skip start slash
        }

        public LocalPath GetDirectory()
        {
            return new LocalPath(System.IO.Path.GetDirectoryName(Path));
        }

        public bool Exists()
        {
            return !IsEmpty && (Directory.Exists(Path) || File.Exists(Path));
        }

        //TFS requires local file names to be in Windows format to have a drive letter and to use \ slash
        public string ToRepositoryLocalPath()
        {
            if (!EnvironmentHelper.IsRunningOnUnix)
                return this.Path;
            else
            {
                return "U:" + this.Path.Replace('/', '\\'); //Use U: like git-tf
            }
        }
        
        public static implicit operator LocalPath(string path)
        {
            return new LocalPath(path);
        }

        public override string ToString()
        {
            return this.Path;
        }

        #region Equal

        #region IComparable<LocalPath> Members

        public int CompareTo(LocalPath other)
        {

            return string.Compare(this.Path, other.Path, CompareType);
        }

        #endregion

        #region IEquatable<LocalPath> Members

        public bool Equals(LocalPath other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return string.Equals(this.Path, other.Path, CompareType);
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            var cast = obj as LocalPath;
            if (cast == null)
                return false;
            return Equals(cast);
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }

        public static bool operator ==(LocalPath left, LocalPath right)
        {
            return ReferenceEquals(null, left) ? ReferenceEquals(null, right) : left.Equals(right);
        }

        public static bool operator !=(LocalPath left, LocalPath right)
        {
            return !(left == right);
        }

        #endregion Equal
    }
}