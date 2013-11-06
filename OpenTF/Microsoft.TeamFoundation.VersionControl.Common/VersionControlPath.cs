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
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.TeamFoundation.VersionControl.Common
{
    public sealed class VersionControlPath
    {
        public const string RootFolder = "$/";
        public const char Separator = '/';
        private readonly string[] pathParts;
        private readonly string path;

        public VersionControlPath ParentPath
        { 
            get
            { 
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
            this.pathParts = path.Split(Separator).Skip(1).ToArray();
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
    }
}
