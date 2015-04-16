// UrlHelper.cs
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

namespace MonoDevelop.VersionControl.TFS.Helpers
{
    public static class UrlHelper
    {
        private static readonly char urlSeparator = '/';

        public static Uri AddPath(this Uri baseUri, string path)
        {
            if (baseUri == null)
                throw new ArgumentNullException("baseUri");
            if (path == null)
                throw new ArgumentNullException("path");

            var uriBuilder = new UriBuilder(baseUri);
            uriBuilder.Path = CombinePaths(uriBuilder.Path, path);
            return uriBuilder.Uri;
        }

        public static string CombinePaths(string path1, string path2)
        {
            if (path1 == null)
                path1 = string.Empty;
            if (path2 == null)
                path2 = string.Empty;
            path1 = path1.TrimEnd(urlSeparator);
            path2 = path2.TrimStart(urlSeparator);
            return path1 + urlSeparator + path2;
        }

//        public static string GetFirstItemOfPath(string path)
//        {
//            if (path == null)
//                throw new ArgumentNullException("path");
//            var items = path.Split(urlSeparator, StringSplitOptions.RemoveEmptyEntries);
//            if (items.Length > 0)
//                return items[0];
//            return string.Empty;
//        }
    }
}

