// TFSRepository.cs
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.TextEditor;
using Mono.TextEditor.Utils;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.VersionControl.TFS.GUI.VersionControl;
using MonoDevelop.VersionControl.TFS.Helpers;
using MonoDevelop.VersionControl.TFS.Infrastructure;
using MonoDevelop.VersionControl.TFS.Infrastructure.Models;
using MonoDevelop.VersionControl.TFS.Infrastructure.Services;
using MonoDevelop.VersionControl.TFS.MonoDevelopWrappers;
using MonoDevelop.VersionControl.TFS.VersionControl;
using MonoDevelop.VersionControl.TFS.VersionControl.Enums;
using MonoDevelop.VersionControl.TFS.VersionControl.Infrastructure;
using MonoDevelop.VersionControl.TFS.VersionControl.Models;
using MonoDevelop.VersionControl.TFS.WorkItemTracking.Structure;

namespace MonoDevelop.VersionControl.TFS
{
    internal class TFSRepository : Repository
    {
        private readonly IWorkspace workspace;
        //private readonly RepositoryCache cache;
        private readonly VersionInfoResolver _versionInfoResolver;
        private readonly TFSVersionControlService _versionControlService;
        private readonly IFileKeeperService _fileKeeperService;

        public TFSRepository(string rootPath, IWorkspace workspace,
                             TFSVersionControlService versionControlService, IFileKeeperService fileKeeperService)
        {
            if (workspace == null)
                return;
            this.RootPath = rootPath;
            this.workspace = workspace;
            _versionControlService = versionControlService;
            _fileKeeperService = fileKeeperService;
            _versionInfoResolver = new VersionInfoResolver(this);
        }

        public INotificationService NotificationService { get; set; }

        private bool IsFileInWorkspace(LocalPath path)
        {
            return workspace.Data.IsLocalPathMapped(path);
        }

//        private IEnumerable<VersionInfo> GetItemVersionInfo(ExtendedItem item, bool getRemoteStatus)
//        {
//            var localStatus = GetLocalVersionStatus(item);
//            var localRevision = GetLocalRevision(item);
//            var remoteStatus = getRemoteStatus ? GetServerVersionStatus(item) : VersionStatus.Versioned;
//            var remoteRevision = getRemoteStatus ? GetServerRevision(item) : (TFSRevision)null;
//            var path = item.LocalPath;
//            if (string.IsNullOrEmpty(path) && item.ChangeType.HasFlag(ChangeType.Delete)) //Pending for delete.
//            {
//                path = workspace.Data.GetLocalPathForServerPath(item.ServerPath);
//            }
//            yield return new VersionInfo(new FilePath(path), item.ServerPath, item.ItemType == ItemType.Folder, 
//                localStatus, localRevision, remoteStatus, remoteRevision);
//        }

        private VersionInfo[] GetItemsVersionInfo(IEnumerable<LocalPath> paths, bool recursive = false)
        {
            return _versionInfoResolver.GetStatus(paths, recursive).Values.ToArray();


//            List<VersionInfo> infos = new List<VersionInfo>();
//            var extendedItems = cache.GetItems(paths, recursive);
//            foreach (var item in extendedItems.Where(i => i.IsInWorkspace || (!i.IsInWorkspace && i.ChangeType.HasFlag(ChangeType.Delete))).Distinct())
//            {
//                infos.AddRange(GetItemVersionInfo(item, getRemoteStatus));
//            }
//            foreach (var path in paths)
//            {
//                var path1 = path;
//                if (infos.All(i => path1 != i.LocalPath))
//                {
//                    infos.Add(VersionInfo.CreateUnversioned(new FilePath(path1), FileHelper.HasFolder(path1)));
//                }
//            }
//            return infos.ToArray();
        }

        #region implemented members of Repository

        public override string GetBaseText(FilePath localFile)
        {
            return GetBaseText(new LocalPath(localFile));
        }

        private string GetBaseText(LocalPath localFile)
        {
            var item = workspace.GetItem(ItemSpec.FromLocalPath(localFile), ItemType.File, true);
            return workspace.GetItemContent(item);
        }

        protected override Revision[] OnGetHistory(FilePath localFile, Revision since)
        {
            return OnGetHistory(new LocalPath(localFile), since);
        }

        private Revision[] OnGetHistory(LocalPath localFile, Revision since)
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
            return GetItemsVersionInfo(paths.Select(p => new LocalPath(p)));
        }

        protected override VersionInfo[] OnGetDirectoryVersionInfo(FilePath localDirectory, bool getRemoteStatus, bool recursive)
        {
            //Get only files in current solution.
            var solutions = IdeApp.Workspace.GetAllSolutions().Where(s => s.BaseDirectory.IsChildPathOf(localDirectory) || s.BaseDirectory == localDirectory);
            var paths = new List<LocalPath> { new LocalPath(localDirectory) };
            foreach (var solution in solutions)
            {
                var sfiles = solution.GetItemFiles(true);
                paths.AddRange(sfiles.Where(f => f != localDirectory).Select(f => new LocalPath(f)));
            }
            return GetItemsVersionInfo(paths, recursive);
        }

        protected override Repository OnPublish(string serverPath, FilePath localPath, FilePath[] files, string message, IProgressMonitor monitor)
        {
            throw new NotImplementedException();
        }

        protected override void OnUpdate(FilePath[] localPaths, bool recurse, IProgressMonitor monitor)
        {
            var paths = localPaths.Select(x => new LocalPath(x)).ToArray();
            var getRequests = paths.Select(p => new LocalPath(p))
                                   .Where(IsFileInWorkspace)
                                   .Select(file => new GetRequest(file, recurse ? RecursionType.Full : RecursionType.None, VersionSpec.Latest))
                                   .ToList();
            workspace.Get(getRequests, GetOptions.None);
            _versionInfoResolver.InvalidateCache(paths);
            NotificationService.NotifyFilesChanged(paths);
        }

        protected override void OnCommit(ChangeSet changeSet, IProgressMonitor monitor)
        {
            var commitItems = (from it in changeSet.Items
                let path = new LocalPath(it.LocalPath)
                let needUpload = path.IsFile && (it.Status.HasFlag(VersionStatus.ScheduledAdd) || it.Status.HasFlag(VersionStatus.Modified))
                select new CommitItem
                {
                    LocalPath = path,
                    NeedUpload = needUpload
                }).ToArray();

            Dictionary<int, WorkItemCheckinAction> workItems = null;
            if (changeSet.ExtendedProperties.Contains("TFS.WorkItems"))
                workItems = (Dictionary<int, WorkItemCheckinAction>)changeSet.ExtendedProperties["TFS.WorkItems"];
            var result = workspace.CheckIn(commitItems, changeSet.GlobalComment, workItems);
            if (result.Failures != null && result.Failures.Any(x => x.SeverityType == SeverityType.Error))
            {
                MessageService.ShowError("Commit failed!", string.Join(Environment.NewLine, result.Failures.Select(f => f.Message)));
                ResolveConflictsView.Open(workspace, changeSet.Items.Select(w => new LocalPath(w.LocalPath)).ToList());
            }

            foreach (var file in changeSet.Items.Where(i => !i.IsDirectory))
            {
                VersionControlService.NotifyFileStatusChanged(new FileUpdateEventArgs());
                NotificationService.NotifyFileChanged(file.LocalPath);
            }

            _versionInfoResolver.InvalidateCache(commitItems.Select(i => i.LocalPath));
        }

        protected override void OnCheckout(FilePath targetLocalPath, Revision rev, bool recurse, IProgressMonitor monitor)
        {
            throw new NotSupportedException("CheckOut is not supported");
        }

        protected override void OnRevert(FilePath[] localPaths, bool recurse, IProgressMonitor monitor)
        {
            var specs = localPaths.Select(x => new ItemSpec(x, recurse ? RecursionType.Full : RecursionType.None));
            var operations = workspace.Undo(specs);
            _versionInfoResolver.InvalidateCache(operations);

            NotificationService.NotifyFilesChanged(operations);
            NotificationService.NotifyFilesRemoved(localPaths.Select(p => new LocalPath(p)).Where(p => !p.Exists));
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
            workspace.Get(request, GetOptions.None);
            _versionInfoResolver.InvalidateCache(localPath);
            NotificationService.NotifyFileChanged(localPath);
        }

        protected override void OnAdd(FilePath[] localPaths, bool recurse, IProgressMonitor monitor)
        {
            var paths = localPaths.Select(x => new LocalPath(x)).Where(IsFileInWorkspace).ToArray();
            ICollection<Failure> failures;
            workspace.PendAdd(paths, recurse, out failures);
            FailuresDisplayDialog.ShowFailures(failures);
            _versionInfoResolver.InvalidateCache(paths, recurse);
            NotificationService.NotifyFilesChanged(paths);
        }

        protected override void OnDeleteFiles(FilePath[] localPaths, bool force, IProgressMonitor monitor, bool keepLocal)
        {
            var paths = localPaths.Select(p => new LocalPath(p)).ToArray();
            DeletePaths(paths, false, monitor, keepLocal);
        }

        protected override void OnDeleteDirectories(FilePath[] localPaths, bool force, IProgressMonitor monitor, bool keepLocal)
        {
            var paths = localPaths.Select(p => new LocalPath(p)).ToArray();
            DeletePaths(paths, true, monitor, keepLocal);
        }

        private void DeletePaths(LocalPath[] localPaths, bool recursive, IProgressMonitor monitor, bool keepLocal)
        {
            using (var keeper = _fileKeeperService.StartSession())
            {
                if (keepLocal) keeper.Save(localPaths, recursive);

                var statuses = _versionInfoResolver.GetStatus(localPaths, recursive);
                //Remove files which are versioned and not added.
                var forRemove = statuses.Where(s => s.Value.IsVersioned &&
                                                    !s.Value.HasLocalChange(VersionStatus.ScheduledAdd)).Select(s => s.Key);

                ICollection<Failure> failures;
                workspace.PendDelete(forRemove, recursive ? RecursionType.Full : RecursionType.None, keepLocal, out failures);
                if (failures.Any(f => f.SeverityType == SeverityType.Error))
                {
                    foreach (var failure in failures.Where(f => f.SeverityType == SeverityType.Error))
                    {
                        monitor.ReportError(failure.Code, new Exception(failure.Message));
                    }
                }

                //Rever added files.
                var addedFiles = statuses.Where(s => s.Value.HasLocalChange(VersionStatus.ScheduledAdd)).Select(s => new FilePath(s.Key)).ToArray();
                this.Revert(addedFiles, recursive, monitor);

                _versionInfoResolver.InvalidateCache(localPaths, recursive);
                NotificationService.NotifyFilesRemoved(localPaths);
            }
        }



        protected override string OnGetTextAtRevision(FilePath repositoryPath, Revision revision)
        {
            var tfsRevision = (TFSRevision)revision;
            if (tfsRevision.Version == 0)
                return string.Empty;

            var serverPath = workspace.Data.GetServerPathForLocalPath(new LocalPath(repositoryPath));
            if (string.IsNullOrEmpty(serverPath))
                return string.Empty;

            var item = new ItemSpec(serverPath, RecursionType.None);
            var items = workspace.GetItems(item.ToEnumerable(), new ChangesetVersionSpec(tfsRevision.Version), DeletedState.Any, ItemType.Any, true);
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
                string path = workspace.Data.GetLocalPathForServerPath(changesByItem.Key.ServerPath);
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
                    var item = workspace.GetItem(ItemSpec.FromServerPath(new RepositoryPath(versionInfo.RepositoryPath, false)), ItemType.File, true);
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
                var diff = Diff.GetDiffString(server, local);
                return new DiffInfo(baseLocalPath, versionInfo.LocalPath, diff);
            }
        }

        protected override void OnMoveFile(FilePath localSrcPath, FilePath localDestPath, bool force, IProgressMonitor monitor)
        {
            base.OnMoveFile(localSrcPath, localDestPath, force, monitor);
            Move(new LocalPath(localSrcPath), new LocalPath(localDestPath));
        }

        protected override void OnMoveDirectory(FilePath localSrcPath, FilePath localDestPath, bool force, IProgressMonitor monitor)
        {
            base.OnMoveDirectory(localSrcPath, localDestPath, force, monitor);
            Move(new LocalPath(localSrcPath), new LocalPath(localDestPath));
        }

        private void Move(LocalPath from, LocalPath to)
        {
            ICollection<Failure> failures;
            workspace.PendRename(from, to, out failures);
            FailuresDisplayDialog.ShowFailures(failures);
            _versionInfoResolver.InvalidateCache(to);

            NotificationService.NotifyFileRemoved(from);
            NotificationService.NotifyFileChanged(to);
        }

        protected override VersionControlOperation GetSupportedOperations(VersionInfo vinfo)
        {
            if (!this.IsFileInWorkspace(new LocalPath(vinfo.LocalPath)))
            {
                return VersionControlOperation.None;
            }
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
                foreach (var path in paths.Select(p => new LocalPath(p)))
                {
                    if (!path.Exists || !path.IsReadOnly)
                        continue;
                    progress.Log.WriteLine("Start editing item: " + path);
                    try
                    {
                        ICollection<Failure> failures;
                        workspace.PendEdit(path.ToEnumerable(), RecursionType.None, _versionControlService.CheckOutLockLevel, out failures);
                        if (failures.Any(f => f.SeverityType == SeverityType.Error))
                        {
                            foreach (var failure in failures.Where(f => f.SeverityType == SeverityType.Error))
                            {
                                progress.ReportError(failure.Code, new Exception(failure.Message));
                            }
                        }
                        else
                        {
                            _versionInfoResolver.InvalidateCache(path);                            
                            NotificationService.NotifyFileChanged(path);
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
            var paths = localPaths.Select(x => new LocalPath(x)).ToArray();
            LockItems(paths, LockLevel.CheckOut);
        }

        protected override void OnUnlock(IProgressMonitor monitor, params FilePath[] localPaths)
        {
            var paths = localPaths.Select(x => new LocalPath(x)).ToArray();
            LockItems(paths, LockLevel.None);
        }

        private void LockItems(LocalPath[] localPaths, LockLevel lockLevel)
        {
            workspace.LockItems(localPaths, lockLevel);
            _versionInfoResolver.InvalidateCache(localPaths);
            NotificationService.NotifyFilesChanged(localPaths);
        }
        
        #endregion

        internal void CheckoutFile(LocalPath path)
        {
            using (var progress = VersionControlService.GetProgressMonitor("CheckOut"))
            {
                progress.Log.WriteLine("Start check out item: " + path);
                ICollection<Failure> failures;
                workspace.CheckOut(path.ToEnumerable(), out failures);
                FailuresDisplayDialog.ShowFailures(failures);
                _versionInfoResolver.InvalidateCache(path);
                NotificationService.NotifyFileChanged(path);
                progress.ReportSuccess("Finish check out.");
            }
        }

        internal void Refresh()
        {
            this._versionInfoResolver.InvalidateCache();
            workspace.RefreshPendingChanges();
        }

        internal IWorkspace Workspace { get { return workspace; }}
    }
}

