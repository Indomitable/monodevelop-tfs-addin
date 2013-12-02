//
// TFSClient.cs
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
using Microsoft.TeamFoundation.Client;
using MonoDevelop.VersionControl.TFS.Helpers;
using System.Linq;
using Microsoft.TeamFoundation.VersionControl.Client;
using System.Collections.Generic;
using MonoDevelop.Core;
using System.IO;
using System;
using MonoDevelop.VersionControl.TFS.GUI.Server;

namespace MonoDevelop.VersionControl.TFS
{
    public class TFSClient : VersionControlSystem
    {
        private TFSRepository repository;

        public TFSClient()
        {
            if (VersionControlService.IsGloballyDisabled)
            {
                var pad = MonoDevelop.Ide.IdeApp.Workbench.GetPad<MonoDevelop.VersionControl.TFS.GUI.TeamExplorerPad>();
                if (pad != null)
                {
                    pad.Destroy();
                }
            }
        }

        #region implemented abstract members of VersionControlSystem

        protected override Repository OnCreateRepositoryInstance()
        {
            return new TFSRepository(null);
        }

        public override IRepositoryEditor CreateRepositoryEditor(Repository repo)
        {
            return new TFSRepositoryEditor((TFSRepository)repo);
        }

        public override string Name { get { return "TFS"; } }

        #endregion

        public override bool IsInstalled { get { return true; } }

        public override Repository GetRepositoryReference(FilePath path, string id)
        {
            if (path.IsNullOrEmpty)
                return null;
            var solutionPath = Path.ChangeExtension(Path.Combine(path, id), "sln");
            if (File.Exists(solutionPath)) //Read Solution
            {
                var repo = FindBySolution(solutionPath);
                if (repo != null)
                    return repo;
                else
                    return FindByPath(path);
            }
            else
            {
                return FindByPath(path);
            }
            return null;
        }

        private Repository FindBySolution(FilePath solutionPath)
        {
            var content = File.ReadAllLines(solutionPath);
            var line = content.FirstOrDefault(x => x.IndexOf("SccTeamFoundationServer", System.StringComparison.OrdinalIgnoreCase) > -1);
            if (line == null)
                return null;
            var parts = line.Split(new [] { "=" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                return null;
            var serverPath = new Uri(parts[1].Trim());
            foreach (var server in TFSVersionControlService.Instance.Servers)
            {
                if (string.Equals(serverPath.Host, server.Uri.Host, StringComparison.OrdinalIgnoreCase))
                {
                    var repo = GetRepoFromServer(server, solutionPath);
                    if (repo != null)
                        return repo;
                }
            }
            return null;
        }

        private Repository FindByPath(FilePath path)
        {
            foreach (var server in TFSVersionControlService.Instance.Servers)
            {
                var repo = GetRepoFromServer(server, path);
                if (repo != null)
                    return repo;
            }
            return null;
        }

        private Repository GetRepoFromServer(TeamFoundationServer server, FilePath path)
        {
            foreach (var collection in server.ProjectCollections)
            {
                var workspaces = WorkspaceHelper.GetLocalWorkspaces(collection);
                var workspace = workspaces.SingleOrDefault(w => w.IsLocalPathMapped(path));
                if (workspace != null)
                {
                    var result = repository ?? (repository = new TFSRepository(workspace.VersionControlService));
                    result.AttachWorkspace(workspace);
                    return result;
                }
            }
            return null;
        }
    }
}

