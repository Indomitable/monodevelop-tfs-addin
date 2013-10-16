//
// TFSRepository.cs
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
using MonoDevelop.Core;
using MonoDevelop.VersionControl.TFS.Infrastructure.Objects;
using System.IO;
using MonoDevelop.Ide;
using System.Linq;
using Mono.TextEditor;
using Microsoft.TeamFoundation.VersionControl.Common;

namespace MonoDevelop.VersionControl.TFS
{
    public class TFSRepository : Repository
    {
        private readonly List<Workspace> workspaces = new List<Workspace>();

        public TfsVersionControlService VersionControlService { get; set; }

        private Workspace GetWorkspaceByLocalPath(FilePath path)
        {
            return workspaces.SingleOrDefault(x => x.IsLocalPathMapped(path));
        }

        public Workspace GetWorkspaceByServerPath(string path)
        {
            return workspaces.SingleOrDefault(x => x.IsServerPathMapped(path));
        }

        public void AttachWorkspace(Workspace workspace)
        {
            if (workspace == null)
                throw new ArgumentNullException("workspace");
            if (workspaces.Contains(workspace))
                return;
            workspace.FillPendingChanges();
            workspaces.Add(workspace);
        }

        public TFSRepository(TfsVersionControlService versionControlService)
        {
            this.VersionControlService = versionControlService;
            MonitorFileChanged();
        }

        private void MonitorFileChanged()
        {
            IdeApp.Workbench.DocumentOpened += (sender, de) =>
            {
                if (de.Document.Editor == null || de.Document.Editor.Document == null)
                    return;
                var textDocument = de.Document.Editor.Document;
                textDocument.DocumentUpdated += (s, e) =>
                {

                };
//                if (File.GetAttributes(de.Document.FileName).HasFlag(FileAttributes.ReadOnly))
//                    return;
//                var content = de.Document.ActiveView.GetContent<IViewContent>();
//                if (content != null)
//                {
//                    content.DirtyChanged += (s, e) => Workspace.CheckOutFile(de.Document.FileName);
//                }
            };
        }

        #region implemented abstract members of Repository

        public override string GetBaseText(FilePath localFile)
        {
            var workspace = this.GetWorkspaceByLocalPath(localFile);
            var serverPath = workspace.TryGetServerItemForLocalItem(localFile);
            if (string.IsNullOrEmpty(serverPath))
                return string.Empty;

            var item = workspace.GetItem(serverPath, ItemType.File, true);
            return workspace.GetItemContent(item);
        }

        protected override Revision[] OnGetHistory(FilePath localFile, Revision since)
        {
            var workspace = this.GetWorkspaceByLocalPath(localFile);
            var serverPath = workspace.TryGetServerItemForLocalItem(localFile);
            ItemSpec spec = new ItemSpec(serverPath, RecursionType.None);
            ChangesetVersionSpec versionFrom = null;
            if (since != null)
                versionFrom = new ChangesetVersionSpec(((TfsRevision)since).Version);
            return this.VersionControlService.QueryHistory(spec, VersionSpec.Latest, versionFrom, null).Select(x => new TfsRevision(this, serverPath, x)).ToArray();
        }

        protected VersionStatus GetLocalVersionStatus(ExtendedItem item)
        {
            if (item == null)
                return VersionStatus.Unversioned;
            var workspace = this.GetWorkspaceByLocalPath(item.LocalItem);
            if (workspace == null)
                return VersionStatus.Unversioned;
            var status = VersionStatus.Versioned;
            var changes = workspace.PendingChanges.Where(ch => ch.ItemId == item.ItemId);
            foreach (var change in changes)
            {
                if (change.ChangeType.HasFlag(ChangeType.Add))
                    status = status | VersionStatus.ScheduledAdd;

                if (change.ChangeType.HasFlag(ChangeType.Edit))
                    status = status | VersionStatus.Modified;

                if (change.ChangeType.HasFlag(ChangeType.Delete))
                    status = status | VersionStatus.ScheduledDelete;

                if (change.ChangeType.HasFlag(ChangeType.Lock))
                    status = status | VersionStatus.Locked;
            }
            return status;
        }

        protected VersionStatus GetServerVersionStatus(ExtendedItem item)
        {
            if (item == null)
                return VersionStatus.Unversioned;
            if (item.DeletionId > 0)
                return VersionStatus.Missing;
            if (item.VersionLatest > item.VersionLocal)
                return VersionStatus.Modified;
            return VersionStatus.Versioned;
        }

        protected Revision GetLocalRevision(ExtendedItem item)
        {
            return new TfsRevision(this, item.VersionLocal, item.SourceServerItem);
        }

        protected Revision GetServerRevision(ExtendedItem item)
        {
            return new TfsRevision(this, item.VersionLatest, item.SourceServerItem);
        }

        protected override IEnumerable<VersionInfo> OnGetVersionInfo(IEnumerable<FilePath> paths, bool getRemoteStatus)
        {
            return GetItemsVersionInfo(paths.ToArray(), getRemoteStatus, RecursionType.None);
        }

        private VersionInfo[] GetItemsVersionInfo(FilePath[] paths, bool getRemoteStatus, RecursionType recursive)
        {
            List<ItemSpec> serverPaths = new List<ItemSpec>();
            List<VersionInfo> infos = new List<VersionInfo>();
            Dictionary<Workspace, List<ItemSpec>> workspaceItems = new Dictionary<Workspace, List<ItemSpec>>();
            foreach (var path in paths)
            {
                var workspace = this.GetWorkspaceByLocalPath(path);
                var serverPath = workspace.TryGetServerItemForLocalItem(path);  
                if (string.IsNullOrEmpty(serverPath))
                    infos.Add(VersionInfo.CreateUnversioned(path, path.IsDirectory));
                var item = new ItemSpec(serverPath, recursive);
                if (workspaceItems.ContainsKey(workspace))
                {
                    workspaceItems[workspace].Add(item);
                }
                else
                {
                    workspaceItems.Add(workspace, new List<ItemSpec> { item });
                }
            }
            var items = workspaceItems.SelectMany(x => x.Key.GetExtendedItems(x.Value, DeletedState.Any, ItemType.Any)).ToArray();
            infos.AddRange(items
                .Where(i => i.IsInWorkspace) //Remove not downloaded
                .Select(i => new VersionInfo(i.LocalItem, i.ServerPath, i.ItemType == ItemType.Folder, 
                GetLocalVersionStatus(i), GetLocalRevision(i), 
                getRemoteStatus ? GetServerVersionStatus(i) : VersionStatus.Versioned, 
                getRemoteStatus ? GetServerRevision(i) : null)));
            infos.AddRange(paths.Where(p => infos.All(i => p != i.LocalPath)).Select(p => VersionInfo.CreateUnversioned(p, p.IsDirectory)));
            return infos.ToArray();
        }

        protected override VersionInfo[] OnGetDirectoryVersionInfo(FilePath localDirectory, bool getRemoteStatus, bool recursive)
        {
            RecursionType recursionType = recursive ? RecursionType.Full : RecursionType.OneLevel;
            return GetItemsVersionInfo(new [] { localDirectory }, getRemoteStatus, recursionType);
        }

        protected override Repository OnPublish(string serverPath, FilePath localPath, FilePath[] files, string message, IProgressMonitor monitor)
        {
            throw new NotImplementedException();
        }

        protected override void OnUpdate(FilePath[] localPaths, bool recurse, IProgressMonitor monitor)
        {
            throw new NotImplementedException();
        }

        protected override void OnCommit(ChangeSet changeSet, IProgressMonitor monitor)
        {
            throw new NotImplementedException();
        }

        protected override void OnCheckout(FilePath targetLocalPath, Revision rev, bool recurse, IProgressMonitor monitor)
        {
//            SourceControlExplorerView view = new SourceControlExplorerView(monitor);
//            view.Load(Url);
//            IdeApp.Workbench.OpenDocument(view, true);
        }

        protected override void OnRevert(FilePath[] localPaths, bool recurse, IProgressMonitor monitor)
        {
            var paths = new Dictionary<Workspace, List<string>>(); 
            for (int i = 0; i < localPaths.Length; i++)
            {
                var workspace = this.GetWorkspaceByLocalPath(localPaths[i]);
                if (paths.ContainsKey(workspace))
                {
                    paths[workspace].Add(localPaths[i]);
                }
                else
                {
                    paths.Add(workspace, new List<string> { localPaths[i] });
                }
            }
            foreach (var ws in paths)
            {
                ws.Key.Undo(ws.Value, recurse ? RecursionType.Full : RecursionType.None);
            }
            FileService.NotifyFilesChanged(localPaths.Where(x => File.Exists(x)));
            FileService.NotifyFilesRemoved(localPaths.Where(x => !File.Exists(x)));
        }

        protected override void OnRevertRevision(FilePath localPath, Revision revision, IProgressMonitor monitor)
        {
            throw new NotImplementedException();
        }

        protected override void OnRevertToRevision(FilePath localPath, Revision revision, IProgressMonitor monitor)
        {
            throw new NotImplementedException();
        }

        protected override void OnAdd(FilePath[] localPaths, bool recurse, IProgressMonitor monitor)
        {
            throw new NotImplementedException();
        }

        protected override void OnDeleteFiles(FilePath[] localPaths, bool force, IProgressMonitor monitor)
        {
            throw new NotImplementedException();
        }

        protected override void OnDeleteDirectories(FilePath[] localPaths, bool force, IProgressMonitor monitor)
        {
            throw new NotImplementedException();
        }

        protected override string OnGetTextAtRevision(FilePath repositoryPath, Revision revision)
        {
            var tfsRevision = (TfsRevision)revision;
            var workspace = GetWorkspaceByLocalPath(repositoryPath);
            var changeSet = this.VersionControlService.QueryChangeset(tfsRevision.Version, true, true);
            var serverPath = workspace.TryGetServerItemForLocalItem(repositoryPath);
            var change = changeSet.Changes.FirstOrDefault(c => string.Equals(serverPath, c.Item.ServerItem));
            if (change == null)
                return string.Empty;
            return workspace.GetItemContent(change.Item);
        }

        protected override RevisionPath[] OnGetRevisionChanges(Revision revision)
        {
            var tfsRevision = (TfsRevision)revision;

            var changeSet = this.VersionControlService.QueryChangeset(tfsRevision.Version, true);
            List<RevisionPath> revisionPaths = new List<RevisionPath>();
            foreach (var change in changeSet.Changes)
            {
                RevisionAction action = RevisionAction.Other;
                var changeType = change.ChangeType;
                changeType &= ~ChangeType.Encoding;
                switch (changeType)
                {
                    case ChangeType.Edit:
                        action = RevisionAction.Modify;
                        break;
                    case ChangeType.Add:
                        action = RevisionAction.Add;
                        break;
                    case ChangeType.Delete:
                        action = RevisionAction.Delete;
                        break;
                    case ChangeType.Rename:
                        action = RevisionAction.Replace;
                        break;
                }
                var workspace = GetWorkspaceByServerPath(change.Item.ServerPath);
                string path = workspace.TryGetLocalItemForServerItem(change.Item.ServerItem);
                path = string.IsNullOrEmpty(path) ? change.Item.ServerItem : path;
                revisionPaths.Add(new RevisionPath(path, action, changeType.ToString()));
            }
            return revisionPaths.ToArray();
        }

        protected override void OnIgnore(FilePath[] localPath)
        {
            //TODO: Add Ignore Option
        }

        protected override void OnUnignore(FilePath[] localPath)
        {
            //TODO: Add UnIgnore Option
        }

        public override DiffInfo GenerateDiff(FilePath baseLocalPath, VersionInfo versionInfo)
        {
            if (versionInfo.LocalPath.IsDirectory)
                return null;
            string text = string.Empty;
            if (versionInfo.Status == VersionStatus.ScheduledAdd || versionInfo.Status == VersionStatus.ScheduledDelete)
            {
                var lines = File.ReadAllLines(versionInfo.LocalPath);
                if (versionInfo.Status == VersionStatus.ScheduledAdd)
                {
                    text = string.Join(Environment.NewLine, lines.Select(l => "+" + l));
                    return new DiffInfo(baseLocalPath, versionInfo.LocalPath, text);
                }
                //if (versionInfo.Status == VersionStatus.ScheduledDelete)
                //{
                text = string.Join(Environment.NewLine, lines.Select(l => "-" + l));
                return new DiffInfo(baseLocalPath, versionInfo.LocalPath, text);
                //}
            }
            else
            {
                text = File.ReadAllText(versionInfo.LocalPath);
                var local = new TextDocument(string.Join(Environment.NewLine, text));
                //Compare with same revision on server
                var serverText = OnGetTextAtRevision(versionInfo.LocalPath, versionInfo.Revision);
                var server = new TextDocument(serverText);
                var diff = Mono.TextEditor.Utils.Diff.GetDiffString(server, local);
                return new DiffInfo(baseLocalPath, versionInfo.LocalPath, diff);
            }
        }

        #endregion

        public override bool RequestFileWritePermission(FilePath path)
        {
            if (!File.Exists(path))
                return true;
            if ((File.GetAttributes(path) & FileAttributes.ReadOnly) == 0)
                return true;
//            try
//            {
//                //workspace.
//            }
//            catch
//            {
//
//            }
//            VersionControlService.NotifyFileStatusChanged (new FileUpdateEventArgs (this, path, false));
            return true;
        }
        //        #region implemented abstract members of UrlBasedRepository
        //
        //        public override string[] SupportedProtocols
        //        {
        //            get
        //            {
        //                return new [] { "http", "https" };
        //            }
        //        }
        //
        //        #endregion
        protected override VersionControlOperation GetSupportedOperations(VersionInfo vinfo)
        {
            var operations = base.GetSupportedOperations(vinfo);
            //operations &= ~VersionControlOperation.
            return operations;
        }
    }
}

