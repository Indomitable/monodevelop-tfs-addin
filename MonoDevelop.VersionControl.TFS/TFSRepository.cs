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
using System.Linq;
using Mono.TextEditor;
using MonoDevelop.Ide;
using Microsoft.TeamFoundation.VersionControl.Client.Objects;
using Microsoft.TeamFoundation.VersionControl.Client.Enums;
using MonoDevelop.VersionControl.TFS.GUI.VersionControl;
using MonoDevelop.VersionControl.TFS.Infrastructure;
using MonoDevelop.VersionControl.TFS.WorkItemTracking.Structure;
using MonoDevelop.VersionControl.TFS.VersionControl;

namespace MonoDevelop.VersionControl.TFS
{
    public class TFSRepository : Repository
    {
        private readonly Workspace workspace;
        private readonly RepositoryCache cache;

        internal TFSRepository(Workspace workspace, string rootPath)
        {
            this.workspace = workspace;
            this.RootPath = rootPath;
            this.cache = new RepositoryCache(this);
        }

        internal bool IsFileInWorkspace(FilePath path)
        {
            return workspace.Data.IsLocalPathMapped(path);
        }

        private bool IsRepositoryFileInWorkspace(string path)
        {
            return workspace.Data.IsServerPathMapped(path);
        }

        private VersionStatus GetLocalVersionStatus(ExtendedItem item)
        {
            if (item == null || !IsRepositoryFileInWorkspace(item.ServerPath))
                return VersionStatus.Unversioned;

            var status = VersionStatus.Versioned;

            if (item.IsLocked) //Locked
            {
                if (item.HasOtherPendingChange)//Locked by someone else
                    status |= VersionStatus.Locked;
                else
                    status |= VersionStatus.LockOwned; //Locked by me
            }

            var changes = workspace.PendingChanges.Where(ch => string.Equals(ch.ServerItem, item.ServerPath, StringComparison.OrdinalIgnoreCase)).ToList();

            if (changes.Any(change => change.IsAdd || change.Version == 0))
            {
                status |= VersionStatus.ScheduledAdd;
                return status;
            }
            if (changes.Any(change => change.IsDelete))
            {
                status |= VersionStatus.ScheduledDelete;
                return status;
            }
            if (changes.Any(change => change.IsRename))
            {
                status = status | VersionStatus.ScheduledAdd;
                return status;
            }
            if (changes.Any(change => change.IsEdit || change.IsEncoding))
            {
                status = status | VersionStatus.Modified;
                return status;
            }
            return status;
        }

        private VersionStatus GetServerVersionStatus(ExtendedItem item)
        {
            if (item == null)
                return VersionStatus.Unversioned;
            var status = VersionStatus.Versioned;
            if (item.IsLocked)
                status = status | VersionStatus.Locked;
            if (item.DeletionId > 0)
                return status | VersionStatus.Missing;
            if (item.VersionLatest > item.VersionLocal)
                return status | VersionStatus.Modified;

            return status;
        }

        private Revision GetLocalRevision(ExtendedItem item)
        {
            return new TFSRevision(this, item.VersionLocal, item.SourceServerItem);
        }

        private Revision GetServerRevision(ExtendedItem item)
        {
            return new TFSRevision(this, item.VersionLatest, item.SourceServerItem);
        }

        private IEnumerable<VersionInfo> GetItemVersionInfo(ExtendedItem item, bool getRemoteStatus)
        {
            var localStatus = GetLocalVersionStatus(item);
            var localRevision = GetLocalRevision(item);
            var remoteStatus = getRemoteStatus ? GetServerVersionStatus(item) : VersionStatus.Versioned;
            var remoteRevision = getRemoteStatus ? GetServerRevision(item) : (TFSRevision)null;
            var path = item.LocalItem;
            if (string.IsNullOrEmpty(path) && item.ChangeType.HasFlag(ChangeType.Delete)) //Pending for delete.
            {
                path = workspace.Data.GetLocalPathForServerPath(item.ServerPath);
            }
            yield return new VersionInfo(path, item.ServerPath, item.ItemType == ItemType.Folder, 
                localStatus, localRevision, remoteStatus, remoteRevision);
        }

        private VersionInfo[] GetItemsVersionInfo(List<FilePath> paths, bool getRemoteStatus, RecursionType recursive)
        {
            List<VersionInfo> infos = new List<VersionInfo>();
            var extendedItems = cache.GetItems(paths, recursive);
            foreach (var item in extendedItems.Where(i => i.IsInWorkspace || (!i.IsInWorkspace && i.ChangeType.HasFlag(ChangeType.Delete))).Distinct())
            {
                infos.AddRange(GetItemVersionInfo(item, getRemoteStatus));
            }
            foreach (var path in paths)
            {
                var path1 = path;
                if (infos.All(i => path1.CanonicalPath != i.LocalPath.CanonicalPath))
                {
                    infos.Add(VersionInfo.CreateUnversioned(path1, FileHelper.HasFolder(path1)));
                }
            }
            return infos.ToArray();
        }

        #region implemented members of Repository

        public override string GetBaseText(FilePath localFile)
        {
            var serverPath = workspace.Data.GetServerPathForLocalPath(localFile);
            if (string.IsNullOrEmpty(serverPath))
                return string.Empty;

            var item = workspace.GetItem(serverPath, ItemType.File, true);
            return workspace.GetItemContent(item);
        }

        protected override Revision[] OnGetHistory(FilePath localFile, Revision since)
        {
            var serverPath = workspace.Data.GetServerPathForLocalPath(localFile);
            ItemSpec spec = new ItemSpec(serverPath, RecursionType.None);
            ChangesetVersionSpec versionFrom = null;
            if (since != null)
                versionFrom = new ChangesetVersionSpec(((TFSRevision)since).Version);
            return workspace.QueryHistory(spec, VersionSpec.Latest, versionFrom, null, short.MaxValue).Select(x => new TFSRevision(this, serverPath, x)).ToArray();
        }

        protected override IEnumerable<VersionInfo> OnGetVersionInfo(IEnumerable<FilePath> paths, bool getRemoteStatus)
        {
            return GetItemsVersionInfo(paths.ToList(), getRemoteStatus, RecursionType.None);
        }

        protected override VersionInfo[] OnGetDirectoryVersionInfo(FilePath localDirectory, bool getRemoteStatus, bool recursive)
        {
            var solutions = IdeApp.Workspace.GetAllSolutions().Where(s => s.BaseDirectory.IsChildPathOf(localDirectory) || s.BaseDirectory == localDirectory);
            List<FilePath> paths = new List<FilePath>();
            paths.Add(localDirectory);
            foreach (var solution in solutions)
            {
                var sfiles = solution.GetItemFiles(true);
                paths.AddRange(sfiles.Where(f => f != localDirectory));
            }

            RecursionType recursionType = recursive ? RecursionType.Full : RecursionType.OneLevel;
            return GetItemsVersionInfo(paths, getRemoteStatus, recursionType);
        }

        protected override Repository OnPublish(string serverPath, FilePath localPath, FilePath[] files, string message, IProgressMonitor monitor)
        {
            throw new NotImplementedException();
        }

        protected override void OnUpdate(FilePath[] localPaths, bool recurse, IProgressMonitor monitor)
        {
            var getRequests = localPaths.Where(IsFileInWorkspace)
                                        .Select(file => new GetRequest(file, recurse ? RecursionType.Full : RecursionType.None, VersionSpec.Latest))
                                        .ToList();
            workspace.Get(getRequests, GetOptions.None, monitor);
            cache.RefreshItems(localPaths);
        }

        protected override void OnCommit(ChangeSet changeSet, IProgressMonitor monitor)
        {
            var changes = workspace.PendingChanges.Where(pc => changeSet.Items.Any(it => string.Equals(pc.LocalItem, it.LocalPath))).ToList();
            Dictionary<int, WorkItemCheckinAction> workItems = null;
            if (changeSet.ExtendedProperties.Contains("TFS.WorkItems"))
                workItems = (Dictionary<int, WorkItemCheckinAction>)changeSet.ExtendedProperties["TFS.WorkItems"];
            var result = workspace.CheckIn(changes, changeSet.GlobalComment, workItems);
            if (result.Failures != null && result.Failures.Any(x => x.SeverityType == SeverityType.Error))
            {
                MessageService.ShowError("Commit failed!", string.Join(Environment.NewLine, result.Failures.Select(f => f.Message)));
                ResolveConflictsView.Open(workspace, changeSet.Items.Select(w => w.LocalPath).ToList());
            }

            foreach (var file in changeSet.Items.Where(i => !i.IsDirectory))
            {
                FileService.NotifyFileChanged(file.LocalPath);
            }

            cache.RefreshItems(changeSet.Items.Select(i => i.LocalPath));
        }

        protected override void OnCheckout(FilePath targetLocalPath, Revision rev, bool recurse, IProgressMonitor monitor)
        {
            throw new NotSupportedException("CheckOut is not supported");
        }

        protected override void OnRevert(FilePath[] localPaths, bool recurse, IProgressMonitor monitor)
        {
            var specs = localPaths.Select(x => new ItemSpec(x, recurse ? RecursionType.Full : RecursionType.None));
            var operations = workspace.Undo(specs, monitor);
            cache.RefreshItems(operations);
            FileService.NotifyFilesChanged(operations);

            FileService.NotifyFilesRemoved(localPaths.Where(x => !FileHelper.HasFile(x)));
        }

        protected override void OnRevertRevision(FilePath localPath, Revision revision, IProgressMonitor monitor)
        {
            throw new NotImplementedException();
        }

        protected override void OnRevertToRevision(FilePath localPath, Revision revision, IProgressMonitor monitor)
        {
            var spec = new ItemSpec(localPath, localPath.IsDirectory ? RecursionType.Full : RecursionType.None);
            var rev = (TFSRevision)revision;
            var request = new GetRequest(spec, new ChangesetVersionSpec(rev.Version));
            workspace.Get(request, GetOptions.None, monitor);
            cache.RefreshItem(localPath);
        }

        protected override void OnAdd(FilePath[] localPaths, bool recurse, IProgressMonitor monitor)
        {
            workspace.PendAdd(localPaths, recurse);
            cache.RefreshItems(localPaths);
            FileService.NotifyFilesChanged(localPaths);
        }

        protected override void OnDeleteFiles(FilePath[] localPaths, bool force, IProgressMonitor monitor, bool keepLocal)
        {
            DeletePaths(localPaths, RecursionType.None, monitor, keepLocal);
        }

        protected override void OnDeleteDirectories(FilePath[] localPaths, bool force, IProgressMonitor monitor, bool keepLocal)
        {
            DeletePaths(localPaths, RecursionType.Full, monitor, keepLocal);
        }

        private void DeletePaths(IEnumerable<FilePath> localPaths, RecursionType recursion, IProgressMonitor monitor, bool keepLocal)
        {
            List<Failure> failures;
            workspace.PendDelete(localPaths.Where(IsFileInWorkspace), recursion, keepLocal, out failures);
            if (failures.Any(f => f.SeverityType == SeverityType.Error))
            {
                foreach (var failure in failures.Where(f => f.SeverityType == SeverityType.Error))
                {
                    monitor.ReportError(failure.Code, new Exception(failure.Message));
                }
            }
            cache.RefreshItems(localPaths);
            FileService.NotifyFilesChanged(localPaths);
        }


        protected override string OnGetTextAtRevision(FilePath repositoryPath, Revision revision)
        {
            var tfsRevision = (TFSRevision)revision;
            if (tfsRevision.Version == 0)
                return string.Empty;

            var serverPath = workspace.Data.GetServerPathForLocalPath(repositoryPath);
            if (string.IsNullOrEmpty(serverPath))
                return string.Empty;

            var items = workspace.GetItems(new [] { new ItemSpec(serverPath, RecursionType.None) }, 
                                           new ChangesetVersionSpec(tfsRevision.Version), DeletedState.Any, ItemType.Any, true);
            if (items.Count == 0)
                return string.Empty;
            return workspace.GetItemContent(items[0]);
        }

        protected override RevisionPath[] OnGetRevisionChanges(Revision revision)
        {
            var tfsRevision = (TFSRevision)revision;

            var changeSet = workspace.QueryChangeset(tfsRevision.Version, true);
            List<RevisionPath> revisionPaths = new List<RevisionPath>();
            var changesByItemGroup = from ch in changeSet.Changes
                                              group ch by ch.Item into grCh
                                              select grCh;
            foreach (var changesByItem in changesByItemGroup)
            {
                RevisionAction action;
                var changes = changesByItem.Select(ch => ch.ChangeType).ToList();
                if (changes.Any(ch => ch.HasFlag(ChangeType.Add)))
                    action = RevisionAction.Add;
                else if (changes.Any(ch => ch.HasFlag(ChangeType.Delete)))
                    action = RevisionAction.Delete;
                else if (changes.Any(ch => ch.HasFlag(ChangeType.Rename) || ch.HasFlag(ChangeType.SourceRename)))
                    action = RevisionAction.Replace;
                else if (changes.Any(ch => ch.HasFlag(ChangeType.Edit) || ch.HasFlag(ChangeType.Encoding)))
                    action = RevisionAction.Modify;
                else
                    action = RevisionAction.Other;
                string path = workspace.Data.GetLocalPathForServerPath(changesByItem.Key.ServerItem);
                path = string.IsNullOrEmpty(path) ? changesByItem.Key.ServerItem : path;
                revisionPaths.Add(new RevisionPath(path, action, action.ToString()));
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
            if (Directory.Exists(versionInfo.LocalPath))
                return null;
            string text;
            if (versionInfo.Status.HasFlag(VersionStatus.ScheduledAdd) || versionInfo.Status.HasFlag(VersionStatus.ScheduledDelete))
            {
                if (versionInfo.Status.HasFlag(VersionStatus.ScheduledAdd))
                {
                    var lines = File.ReadAllLines(versionInfo.LocalPath);
                    text = string.Join(Environment.NewLine, lines.Select(l => "+" + l));
                    return new DiffInfo(baseLocalPath, versionInfo.LocalPath, text);
                }
                if (versionInfo.Status.HasFlag(VersionStatus.ScheduledDelete))
                {
                    var item = workspace.GetItem(versionInfo.RepositoryPath, ItemType.File, true);
                    var lines = workspace.GetItemContent(item).Split(new [] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    text = string.Join(Environment.NewLine, lines.Select(l => "-" + l));
                    return new DiffInfo(baseLocalPath, versionInfo.LocalPath, text);
                }
                return null;
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

        protected override void OnMoveFile(FilePath localSrcPath, FilePath localDestPath, bool force, IProgressMonitor monitor)
        {
            base.OnMoveFile(localSrcPath, localDestPath, force, monitor);
            List<Failure> failures;
            workspace.PendRenameFile(localSrcPath, localDestPath, out failures);
            cache.RefreshItem(localDestPath);
            FailuresDisplayDialog.ShowFailures(failures);
        }

        protected override void OnMoveDirectory(FilePath localSrcPath, FilePath localDestPath, bool force, IProgressMonitor monitor)
        {
            base.OnMoveDirectory(localSrcPath, localDestPath, force, monitor);
            List<Failure> failures;
            workspace.PendRenameFolder(localSrcPath, localDestPath, out failures);
            cache.RefreshItem(localDestPath);
            FailuresDisplayDialog.ShowFailures(failures);
        }

        protected override VersionControlOperation GetSupportedOperations(VersionInfo vinfo)
        {
            var supportedOperations = base.GetSupportedOperations(vinfo);
            if (vinfo.HasLocalChanges) //Disable update for modified files.
                supportedOperations &= ~VersionControlOperation.Update;
            supportedOperations &= ~VersionControlOperation.Annotate; //Annotated is not supported yet.
            if (vinfo.Status.HasFlag(VersionStatus.ScheduledAdd))
                supportedOperations &= ~VersionControlOperation.Log;

            if (vinfo.Status.HasFlag(VersionStatus.Locked))
            {
                supportedOperations &= ~VersionControlOperation.Lock;
                supportedOperations &= ~VersionControlOperation.Remove;
            }
            return supportedOperations;
        }

        public override bool RequestFileWritePermission(params FilePath[] paths)
        {
            using (var progress = VersionControlService.GetProgressMonitor("Edit"))
            {
                foreach (var path in paths)
                {
                    if (!File.Exists(path) || !File.GetAttributes(path).HasFlag(FileAttributes.ReadOnly))
                        continue;
                    progress.Log.WriteLine("Start editing item: " + path);
                    try
                    {
                        var failures = workspace.PendEdit(new List<FilePath> { path }, RecursionType.None, TFSVersionControlService.Instance.CheckOutLockLevel);
                        if (failures.Any(f => f.SeverityType == SeverityType.Error))
                        {
                            foreach (var failure in failures.Where(f => f.SeverityType == SeverityType.Error))
                            {
                                progress.ReportError(failure.Code, new Exception(failure.Message));
                            }
                        }
                        else
                        {
                            cache.RefreshItem(path);
                            progress.ReportSuccess("Finish editing item.");
                        }
                    }
                    catch (Exception ex)
                    {
                        progress.ReportError(ex.Message, ex);
                        return false;
                    }
                }
            }
            return true;
        }

        public override bool AllowLocking
        {
            get
            {
                return true;
            }
        }

        public override bool AllowModifyUnlockedFiles
        {
            get
            {
                return true;
            }
        }

        protected override void OnLock(IProgressMonitor monitor, params FilePath[] localPaths)
        {
            workspace.LockFiles(localPaths.Select(f => (string)f), LockLevel.CheckOut);
            cache.RefreshItems(localPaths);
            FileService.NotifyFilesChanged(localPaths);
        }

        protected override void OnUnlock(IProgressMonitor monitor, params FilePath[] localPaths)
        {
            workspace.LockFiles(localPaths.Select(f => (string)f), LockLevel.None);
            cache.RefreshItems(localPaths);
            FileService.NotifyFilesChanged(localPaths);
        }

        #endregion

        internal void CheckoutFile(FilePath path)
        {
            using (var progress = VersionControlService.GetProgressMonitor("CheckOut"))
            {
                progress.Log.WriteLine("Start check out item: " + path);
                workspace.Get(new GetRequest(path, RecursionType.None, VersionSpec.Latest), GetOptions.GetAll, progress);
                var failures = workspace.PendEdit(new List<FilePath> { path }, RecursionType.None, TFSVersionControlService.Instance.CheckOutLockLevel);
                FailuresDisplayDialog.ShowFailures(failures);
                cache.RefreshItem(path);
                FileService.NotifyFileChanged(path);
                progress.ReportSuccess("Finish check out.");
            }
        }

        public void Resolve(Conflict conflict, ResolutionType resolutionType)
        {
            this.workspace.Resolve(conflict, resolutionType);
        }

        internal void Refresh()
        {
            this.cache.ClearCache();
            workspace.RefreshPendingChanges();
        }

        internal Workspace Workspace { get { return workspace; }}
    }
}

