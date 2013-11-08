//
// UrlHelper.cs
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
using System.Linq;

namespace Microsoft.TeamFoundation.Client
{
    public static class UrlHelper
    {
        private static readonly string[] urlSeparator = { "/" };

        public static Uri AddPathToUri(Uri baseUri, string path)
        {
            if (baseUri == null)
                throw new ArgumentNullException("baseUri");
            if (path == null)
                throw new ArgumentNullException("path");
            var uriBuilder = new UriBuilder(baseUri);
            var splOpt = StringSplitOptions.RemoveEmptyEntries;
            uriBuilder.Path = string.Join(urlSeparator[0], uriBuilder.Path.Split(urlSeparator, splOpt).Union(path.Split(urlSeparator, splOpt)));
            return uriBuilder.Uri;
        }

        public static string GetFirstItemOfPath(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            var items = path.Split(urlSeparator, StringSplitOptions.RemoveEmptyEntries);
            if (items.Length > 0)
                return items[0];
            return string.Empty;
        }
    }
}

