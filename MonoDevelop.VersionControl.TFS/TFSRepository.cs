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
using Microsoft.TeamFoundation.WorkItemTracking.Client.Enums;
using MonoDevelop.VersionControl.TFS.GUI.VersionControl;
using MonoDevelop.VersionControl.TFS.Helpers;

namespace MonoDevelop.VersionControl.TFS
{
    public class TFSRepository : Repository
    {
        private readonly List<Workspace> workspaces = new List<Workspace>();

        internal TFSRepository(RepositoryService versionControlService)
        {
            this.VersionControlService = versionControlService;
        }

        private Workspace GetWorkspaceByLocalPath(FilePath path)
        {
            return workspaces.SingleOrDefault(x => x.IsLocalPathMapped(path));
        }

        private Workspace GetWorkspaceByServerPath(string path)
        {
            return workspaces.SingleOrDefault(x => x.IsServerPathMapped(path));
        }

        private List<IGrouping<Workspace, FilePath>> GroupFilesPerWorkspace(IEnumerable<FilePath> filePaths)
        {
            var filesPerWorkspace = from f in filePaths
                                             let workspace = this.GetWorkspaceByLocalPath(f)
                                             group f by workspace into wg
                                             select wg;
            return filesPerWorkspace.ToList();
        }

        private VersionStatus GetLocalVersionStatus(ExtendedItem item)
        {
            if (item == null)
                return VersionStatus.Unversioned;
            var workspace = this.GetWorkspaceByLocalPath(item.LocalItem);
            if (workspace == null)
                return VersionStatus.Unversioned;
            var status = VersionStatus.Versioned;

            if (item.LockStatus.HasFlag(LockLevel.CheckOut) || item.LockStatus.HasFlag(LockLevel.Checkin)) //Locked
            {
                if (item.HasOtherPendingChange)//Locked by someone else
                    status |= VersionStatus.Locked;
                else
                    status |= VersionStatus.LockOwned; //Locked by me
            }

            var changes = workspace.PendingChanges.Where(ch => string.Equals(ch.LocalItem, item.LocalItem, StringComparison.OrdinalIgnoreCase)).ToList(); //ch.ItemId == item.ItemId

            if (changes.Any(change => change.ChangeType.HasFlag(ChangeType.Add) && change.Version == 0))
            {
                status |= VersionStatus.ScheduledAdd;
                return status;
            }
            if (changes.Any(change => change.ChangeType.HasFlag(ChangeType.Delete)))
            {
                status |= VersionStatus.ScheduledDelete;
                return status;
            }
//            if (changes.Any(change => change.ChangeType.HasFlag(ChangeType.Rename)))
//            {
//                status = status | VersionStatus.ScheduledReplace;
//                return status;
//            }
//            if (changes.Any(change => change.ChangeType.HasFlag(ChangeType.Merge)))
//            {
//                status = status | VersionStatus.Conflicted;
//                return status;
//            }
            if (changes.Any(change => change.ChangeType.HasFlag(ChangeType.Edit)))
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
            if (item.LockStatus.HasFlag(LockLevel.Checkin) || item.LockStatus.HasFlag(LockLevel.CheckOut))
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
            if (item.ChangeType.HasFlag(ChangeType.Rename))
            {
                var deleteWorkspace = GetWorkspaceByServerPath(item.SourceServerItem);
                var deletePath = deleteWorkspace.TryGetLocalItemForServerItem(item.SourceServerItem);
                yield return new VersionInfo(deletePath, item.SourceServerItem, item.ItemType == ItemType.Folder, VersionStatus.Versioned | VersionStatus.ScheduledDelete,
                    GetLocalRevision(item), VersionStatus.Versioned | VersionStatus.ScheduledDelete, GetServerRevision(item));

                var addWorkspace = GetWorkspaceByServerPath(item.TargetServerItem);
                var addPath = addWorkspace.TryGetLocalItemForServerItem(item.TargetServerItem);
                yield return new VersionInfo(addPath, item.TargetServerItem, item.ItemType == ItemType.Folder, VersionStatus.Versioned | VersionStatus.ScheduledAdd,
                    GetLocalRevision(item), VersionStatus.Versioned | VersionStatus.ScheduledAdd, GetServerRevision(item));
            }
            else
            {
                var localStatus = GetLocalVersionStatus(item);
                var localRevision = GetLocalRevision(item);
                var remoteStatus = getRemoteStatus ? GetServerVersionStatus(item) : VersionStatus.Versioned;
                var remoteRevision = getRemoteStatus ? GetServerRevision(item) : (TFSRevision)null;
                yield return new VersionInfo(item.LocalItem, item.ServerPath, item.ItemType == ItemType.Folder, 
                    localStatus, localRevision, remoteStatus, remoteRevision);
            }
        }

        private VersionInfo[] GetItemsVersionInfo(List<FilePath> paths, bool getRemoteStatus, RecursionType recursive)
        {
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
            foreach (var item in items.Where(i => i.IsInWorkspace).Distinct())
            {
                infos.AddRange(GetItemVersionInfo(item, getRemoteStatus));
            }
            infos.AddRange(paths.Where(p => infos.All(i => p.CanonicalPath != i.LocalPath.CanonicalPath)).Select(p => VersionInfo.CreateUnversioned(p, p.IsDirectory)));
            return infos.ToArray();
        }

        #region implemented members of Repository

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
                versionFrom = new ChangesetVersionSpec(((TFSRevision)since).Version);
            return this.VersionControlService.QueryHistory(spec, VersionSpec.Latest, versionFrom, null).Select(x => new TFSRevision(this, serverPath, x)).ToArray();
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
            foreach (var workspace in GroupFilesPerWorkspace(localPaths))
            {
                var getRequests = workspace.Select(file => new GetRequest(file, recurse ? RecursionType.Full : RecursionType.None, VersionSpec.Latest)).ToList();
                workspace.Key.Get(getRequests, GetOptions.None, monitor);
            }
        }

        protected override void OnCommit(ChangeSet changeSet, IProgressMonitor monitor)
        {
            var groupByWorkspace = from it in changeSet.Items
                                            let workspace = this.GetWorkspaceByLocalPath(it.LocalPath)
                                            group it by workspace into wg
                                            select wg;

            foreach (var workspace in groupByWorkspace)
            {
                var workspace1 = workspace;
                var changes = workspace.Key.PendingChanges.Where(pc => workspace1.Any(wi => string.Equals(pc.LocalItem, wi.LocalPath))).ToList();
                Dictionary<int, WorkItemCheckinAction> workItems = null;
                if (changeSet.ExtendedProperties.Contains("TFS.WorkItems"))
                    workItems = (Dictionary<int, WorkItemCheckinAction>)changeSet.ExtendedProperties["TFS.WorkItems"];
                var result = workspace.Key.CheckIn(changes, changeSet.GlobalComment, workItems);
                if (result.Failures != null && result.Failures.Any(x => x.SeverityType == SeverityType.Error))
                {
                    MessageService.ShowError("Commit failed!", string.Join(Environment.NewLine, result.Failures.Select(f => f.Message)));
                    ResolveConflictsView.Open(this, workspace1.Select(w => w.LocalPath).ToList());
                    break;
                }
            }
            foreach (var file in changeSet.Items.Where(i => !i.IsDirectory))
            {
                FileService.NotifyFileChanged(file.LocalPath);
            }
        }

        protected override void OnCheckout(FilePath targetLocalPath, Revision rev, bool recurse, IProgressMonitor monitor)
        {
            var ws = WorkspaceHelper.GetLocalWorkspaces(this.VersionControlService.Collection);
            if (ws.Count == 0)
            {
                Workspace newWorkspace = new Workspace(this.VersionControlService, 
                                             Environment.MachineName + ".WS", this.VersionControlService.Collection.Server.UserName, "Auto created", 
                                             new List<WorkingFolder> { new WorkingFolder(VersionControlPath.RootFolder, targetLocalPath) }, Environment.MachineName);
                var workspace = this.VersionControlService.CreateWorkspace(newWorkspace);
                workspace.Get(new GetRequest(VersionControlPath.RootFolder, RecursionType.Full, VersionSpec.Latest), GetOptions.None, monitor);
            }
            else
            {
                this.workspaces.AddRange(ws);
                var workspace = GetWorkspaceByLocalPath(targetLocalPath);
                if (workspace == null)
                    return;
                workspace.Get(new GetRequest(workspace.TryGetServerItemForLocalItem(targetLocalPath), RecursionType.Full, VersionSpec.Latest), GetOptions.None, monitor);
            }
        }

        protected override void OnRevert(FilePath[] localPaths, bool recurse, IProgressMonitor monitor)
        {
            foreach (var ws in GroupFilesPerWorkspace(localPaths))
            {
                var specs = ws.Select(x => new ItemSpec(x, recurse ? RecursionType.Full : RecursionType.None)).ToList();
                var operations = ws.Key.Undo(specs, monitor);
                FileService.NotifyFilesChanged(operations);
                foreach (var item in operations)
                {
                    MonoDevelop.VersionControl.VersionControlService.NotifyFileStatusChanged(new FileUpdateEventArgs(this, item, item.IsDirectory));
                }
            }
            FileService.NotifyFilesRemoved(localPaths.Where(x => !File.Exists(x)));
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
            var workspace = GetWorkspaceByLocalPath(localPath);
            if (workspace != null)
            {
                workspace.Get(request, GetOptions.None, monitor);
            }
        }

        protected override void OnAdd(FilePath[] localPaths, bool recurse, IProgressMonitor monitor)
        {
            foreach (var ws in GroupFilesPerWorkspace(localPaths))
            {
                ws.Key.PendAdd(ws.ToList(), recurse);
            }
            FileService.NotifyFilesChanged(localPaths);
        }

        protected override void OnDeleteFiles(FilePath[] localPaths, bool force, IProgressMonitor monitor)
        {
            foreach (var ws in GroupFilesPerWorkspace(localPaths))
            {
                List<Failure> failures;
                ws.Key.PendDelete(ws.ToList(), RecursionType.None, out failures);
                if (failures.Any(f => f.SeverityType == SeverityType.Error))
                {
                    foreach (var failure in failures.Where(f => f.SeverityType == SeverityType.Error))
                    {
                        monitor.ReportError(failure.Code, new Exception(failure.Message));
                    }
                }
            }
            FileService.NotifyFilesChanged(localPaths);
        }

        protected override void OnDeleteDirectories(FilePath[] localPaths, bool force, IProgressMonitor monitor)
        {
            foreach (var ws in GroupFilesPerWorkspace(localPaths))
            {
                List<Failure> failures;
                ws.Key.PendDelete(ws.ToList(), RecursionType.Full, out failures);
                if (failures.Any(f => f.SeverityType == SeverityType.Error))
                {
                    foreach (var failure in failures.Where(f => f.SeverityType == SeverityType.Error))
                    {
                        monitor.ReportError(failure.Code, new Exception(failure.Message));
                    }
                }
            }
            FileService.NotifyFilesChanged(localPaths);
        }

        protected override string OnGetTextAtRevision(FilePath repositoryPath, Revision revision)
        {
            var tfsRevision = (TFSRevision)revision;
            if (tfsRevision.Version == 0)
                return string.Empty;
            var workspace = GetWorkspaceByLocalPath(repositoryPath);
            var serverPath = workspace.TryGetServerItemForLocalItem(repositoryPath);
            var items = this.VersionControlService.QueryItems(new ItemSpec(serverPath, RecursionType.None), 
                            new ChangesetVersionSpec(tfsRevision.Version), DeletedState.Any, ItemType.Any, true);
            if (items.Count == 0)
                return string.Empty;
            return workspace.GetItemContent(items[0]);
        }

        protected override RevisionPath[] OnGetRevisionChanges(Revision revision)
        {
            var tfsRevision = (TFSRevision)revision;

            var changeSet = this.VersionControlService.QueryChangeset(tfsRevision.Version, true);
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
                var workspace = GetWorkspaceByServerPath(changesByItem.Key.ServerItem);
                string path = workspace.TryGetLocalItemForServerItem(changesByItem.Key.ServerItem);
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
            if (versionInfo.LocalPath.IsDirectory)
                return null;
            string text;
            if (versionInfo.Status.HasFlag(VersionStatus.ScheduledAdd) || versionInfo.Status.HasFlag(VersionStatus.ScheduledDelete))
            {
                var lines = File.ReadAllLines(versionInfo.LocalPath);
                if (versionInfo.Status.HasFlag(VersionStatus.ScheduledAdd))
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

        protected override void OnMoveFile(FilePath localSrcPath, FilePath localDestPath, bool force, IProgressMonitor monitor)
        {
            var workspace = GetWorkspaceByLocalPath(localSrcPath);
            List<Failure> failures;
            workspace.PendRenameFile(localSrcPath, localDestPath, out failures);
            FailuresDisplayDialog.ShowFailures(failures);
            base.OnMoveFile(localSrcPath, localDestPath, force, monitor);
        }

        protected override void OnMoveDirectory(FilePath localSrcPath, FilePath localDestPath, bool force, IProgressMonitor monitor)
        {
            var workspace = GetWorkspaceByLocalPath(localSrcPath);
            List<Failure> failures;
            workspace.PendRenameFolder(localSrcPath, localDestPath, out failures);
            FailuresDisplayDialog.ShowFailures(failures);
            base.OnMoveDirectory(localSrcPath, localDestPath, force, monitor);
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

        public override bool RequestFileWritePermission(FilePath path)
        {
            if (!File.Exists(path))
                return true;
            if (!File.GetAttributes(path).HasFlag(FileAttributes.ReadOnly))
                return true;
            try
            {
                var doc = IdeApp.Workbench.ActiveDocument;
                DocumentLocation location = default(DocumentLocation);
                if (doc != null && doc.FileName == path && doc.Editor != null)
                {
                    location = doc.Editor.Caret.Location;
                }
                this.CheckoutFile(path);
                if (!location.IsEmpty)
                {
                    doc.Editor.SetCaretTo(location.Line, location.Column, false);
                }
            }
            catch
            {
                return false;
            }
            MonoDevelop.VersionControl.VersionControlService.NotifyFileStatusChanged(new FileUpdateEventArgs(this, path, false));
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
            foreach (var item in GroupFilesPerWorkspace(localPaths))
            {
                item.Key.LockFiles(item.ToList(), LockLevel.CheckOut);
            }
        }

        protected override void OnUnlock(IProgressMonitor monitor, params FilePath[] localPaths)
        {
            foreach (var item in GroupFilesPerWorkspace(localPaths))
            {
                item.Key.LockFiles(item.ToList(), LockLevel.None);
            }
        }

        #endregion

        internal void CheckoutFile(FilePath path)
        {
            using (var progress = MonoDevelop.VersionControl.VersionControlService.GetProgressMonitor("CheckOut"))
            {
                progress.Log.WriteLine("Start check out item: " + path);
                this.ClearCachedVersionInfo(path);
                var workspace = this.GetWorkspaceByLocalPath(path);
                workspace.Get(new GetRequest(path, RecursionType.None, VersionSpec.Latest), GetOptions.GetAll, progress);
                var failures = workspace.PendEdit(new List<FilePath> { path }, RecursionType.None, TFSVersionControlService.Instance.CheckOutLockLevel);
                FailuresDisplayDialog.ShowFailures(failures);
                MonoDevelop.VersionControl.VersionControlService.NotifyFileStatusChanged(new FileUpdateEventArgs(this, path, false));
                FileService.NotifyFileChanged(path);
                progress.ReportSuccess("Finish check out.");
            }
        }

        internal List<FilePath> CheckItemsChangedOnServer(List<FilePath> localPaths)
        {
            var result = new List<FilePath>();
            foreach (var ws in GroupFilesPerWorkspace(localPaths))
            {
                List<ItemSpec> specs = new List<ItemSpec>();
                foreach (var item in ws)
                {
                    specs.Add(new ItemSpec(item, RecursionType.None));
                }
                var items = ws.Key.GetExtendedItems(specs, DeletedState.Any, ItemType.Any);
                foreach (var item in items)
                {
                    if (item.VersionLocal != item.VersionLatest)
                        result.Add(item.LocalItem);
                }
            }
            return result;
        }

        internal void SetConflicted(List<FilePath> result)
        {
            this.ClearCachedVersionInfo(result.ToArray());

            var args = new FileUpdateEventArgs();
            args.AddRange(result.Select(path => new FileUpdateEventInfo(this, path, false)));
            MonoDevelop.VersionControl.VersionControlService.NotifyFileStatusChanged(args);	
        }

        internal void AttachWorkspace(Workspace workspace)
        {
            if (workspace == null)
                throw new ArgumentNullException("workspace");
            if (workspaces.Contains(workspace))
                return;
            workspace.RefreshPendingChanges();
            workspaces.Add(workspace);
        }

        internal List<PendingChange> PendingChanges
        {
            get
            {
                return workspaces.SelectMany(w => w.PendingChanges).ToList();
            }
        }

        internal RepositoryService VersionControlService { get; set; }

        internal List<Conflict> GetConflicts(List<FilePath> paths)
        {
            List<Conflict> conflicts = new List<Conflict>();
            foreach (var workspacePaths in GroupFilesPerWorkspace(paths))
            {
                conflicts.AddRange(workspacePaths.Key.GetConflicts(workspacePaths));
            }
            return conflicts;
        }

        public void Resolve(Conflict conflict, ResolutionType resolutionType)
        {
            conflict.Workspace.Resolve(conflict, resolutionType);
        }
    }
}

