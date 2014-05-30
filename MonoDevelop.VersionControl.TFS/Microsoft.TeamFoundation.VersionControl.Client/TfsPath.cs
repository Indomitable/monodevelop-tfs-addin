//
// Microsoft.TeamFoundation.VersionControl.Client.Repository
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
using System.Runtime.ConstrainedExecution;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
    static class TfsPath
    {
        static bool IsRunningUnix
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        private static bool isRunningOnUnix = true;
        public static StringComparison PlatformComparison = StringComparison.InvariantCulture;

        static TfsPath()
        {
            isRunningOnUnix = IsRunningUnix;

            if (!isRunningOnUnix)
                PlatformComparison = StringComparison.InvariantCultureIgnoreCase;
        }

        public static string ToPlatformPath(string path)
        {
            if (String.IsNullOrEmpty(path))
                return path;

            // TFS servers corrupt *nix type paths
            if (!isRunningOnUnix)
                return path;
            return path.Remove(0, 2).Replace('\\', '/');
        }

        public static string FromPlatformPath(string path)
        {
            if (String.IsNullOrEmpty(path))
                return path;

            // TFS servers corrupt *nix type paths
            if (!isRunningOnUnix)
                return path;
            return "U:" + path.Replace('/', '\\'); //Use U: like git-tf
        }

        public static string ServerToLocalPath(string path)
        {
            //tfs uses / for server paths and \ for localpaths
            if (!isRunningOnUnix)
            {
                path = path.Replace('/', '\\');
            }
            return path;
        }

        public static string LocalToServerPath(string path)
        {
            //tfs uses / for server paths and \ for localpaths
            if (!isRunningOnUnix)
            {
                path = path.Replace('\\', '/');
            }
            return path;
        }
    }
}
