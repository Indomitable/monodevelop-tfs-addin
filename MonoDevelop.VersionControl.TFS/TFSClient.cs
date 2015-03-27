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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MonoDevelop.Core;
using MonoDevelop.VersionControl.TFS.Configuration;
using MonoDevelop.VersionControl.TFS.Core.Structure;
using MonoDevelop.VersionControl.TFS.GUI.Server;
using MonoDevelop.VersionControl.TFS.VersionControl;

namespace MonoDevelop.VersionControl.TFS
{
    public class TFSClient : VersionControlSystem
    {
        private readonly Dictionary<FilePath, TFSRepository> repositoriesCache = new Dictionary<FilePath, TFSRepository>();
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
            return new TFSRepository(null, null);
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
            foreach (var repo in repositoriesCache)
            {
                if (repo.Key == path || path.IsChildPathOf(repo.Key))
                {
                    repo.Value.Refresh();
                    return repo.Value;
                }
                if (repo.Key.IsChildPathOf(path))
                {
                    repositoriesCache.Remove(repo.Key);
                    var repo1 = GetRepository(path, id);
                    repositoriesCache.Add(path, repo1);
                    return repo1;
                }
            }
            var repository = GetRepository(path, id);
            if (repository != null)
                repositoriesCache.Add(path, repository);
            return repository;
        }

        private TFSRepository GetRepository(FilePath path, string id)
        {
            var solutionPath = Path.ChangeExtension(Path.Combine(path, id), "sln");
            if (File.Exists(solutionPath)) //Read Solution
            {
                var repo = FindBySolution(solutionPath);
                return repo ?? FindByPath(path);
            }
            else
            {
                return FindByPath(path);
            }
        }

        private TFSRepository FindBySolution(FilePath solutionPath)
        {
            var content = File.ReadAllLines(solutionPath);
            var line = content.FirstOrDefault(x => x.IndexOf("SccTeamFoundationServer", StringComparison.OrdinalIgnoreCase) > -1);
            if (line == null)
                return null;
            var parts = line.Split(new [] { "=" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                return null;
            var serverPath = new Uri(parts[1].Trim());
            foreach (var server in TFSVersionControlService.Instance.Servers)
            {
                if (string.Equals(serverPath.Host, server.Url.Host, StringComparison.OrdinalIgnoreCase))
                {
                    var repo = GetRepoFromServer(server, solutionPath);
                    if (repo != null)
                        return repo;
                }
            }
            return null;
        }

        private TFSRepository FindByPath(FilePath path)
        {
            foreach (var server in TFSVersionControlService.Instance.Servers)
            {
                var repo = GetRepoFromServer(server, path);
                if (repo != null)
                    return repo;
            }
            return null;
        }

        private TFSRepository GetRepoFromServer(ServerConfig serverConfig, FilePath path)
        {
            var server = TeamFoundationServerFactory.Create(serverConfig);
            foreach (var collectionConfig in serverConfig.ProjectCollections)
            {
                var projectCollection = new ProjectCollection(collectionConfig, server);
                var workspaces = projectCollection.GetLocalWorkspaces();
                var workspaceData = workspaces.SingleOrDefault(w => w.IsLocalPathMapped(path));
                if (workspaceData != null)
                {
                    var workspace = new Workspace(projectCollection, workspaceData);
                    return new TFSRepository(workspace, path);
                }
            }
            return null;
        }

        internal void RefreshRepositories()
        {
            foreach (var repo in repositoriesCache)
            {
                repo.Value.Refresh();
            }
        }
    }
}

