//
// WorkspaceHelper.cs
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
using Microsoft.TeamFoundation.VersionControl.Client;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Client;

namespace MonoDevelop.VersionControl.TFS.Helpers
{
    public static class WorkspaceHelper
    {
        public static List<Workspace> GetLocalWorkspaces(ProjectCollection collection)
        {
            var versionControl = collection.GetService<RepositoryService>();
            return versionControl.QueryWorkspaces(collection.Server.UserName, Environment.MachineName);
        }

        public static List<Workspace> GetRemoteWorkspaces(ProjectCollection collection)
        {
            var versionControl = collection.GetService<RepositoryService>();
            return versionControl.QueryWorkspaces(collection.Server.UserName, string.Empty);
        }

        public static Workspace GetWorkspace(ProjectCollection collection, string workspaceName)
        {
            var versionControl = collection.GetService<RepositoryService>();
            return versionControl.QueryWorkspace(collection.Server.UserName, workspaceName);
        }
    }
}