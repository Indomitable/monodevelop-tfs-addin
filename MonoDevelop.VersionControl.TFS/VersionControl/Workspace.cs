//
// Microsoft.TeamFoundation.VersionControl.Client.Workspace
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Client.Enums;
using Microsoft.TeamFoundation.VersionControl.Client.Objects;
using MonoDevelop.VersionControl.TFS.Core.Structure;
using MonoDevelop.VersionControl.TFS.Helpers;
using MonoDevelop.VersionControl.TFS.MonoDevelopWrappers;
using MonoDevelop.VersionControl.TFS.VersionControl.Models;
using MonoDevelop.VersionControl.TFS.VersionControl.Structure;
using MonoDevelop.VersionControl.TFS.WorkItemTracking;
using MonoDevelop.VersionControl.TFS.WorkItemTracking.Structure;

namespace MonoDevelop.VersionControl.TFS.VersionControl
{
    sealed class Workspace : IWorkspace
    {
        private readonly ProjectCollection collection;
        private readonly WorkspaceData workspaceData;
        private readonly ILoggingService _loggingService;
        private readonly IProjectService _projectService;
        private readonly IProgressService _progressService;
        private readonly List<PendingChange> pendingChanges = new List<PendingChange>(); 

        public Workspace(WorkspaceData data, ProjectCollection collection, ILoggingService loggingService, IProjectService projectService, IProgressService progressService)
        {
            _loggingService = loggingService;
            _projectService = projectService;
            _progressService = progressService;
            this.workspaceData = data;
            this.collection = collection;

            RefreshPendingChanges();
        }

        public CheckInResult CheckIn(List<PendingChange> changes, string comment, Dictionary<int, WorkItemCheckinAction> workItems)
        {
            foreach (var change in changes)
            {
                this.collection.UploadFile(this.workspaceData, change);
            }
            var result = this.collection.CheckIn(this.workspaceData, changes, comment, workItems);
            if (result.ChangeSet > 0)
            {
                WorkItemManager wm = new WorkItemManager(this.collection);
                wm.UpdateWorkItems(result.ChangeSet, workItems, comment);
            }
            this.RefreshPendingChanges();
            ProcessGetOperations(result.LocalVersionUpdates, ProcessType.Get);
            foreach (var file in changes.Where(ch => ch.ItemType == ItemType.File && !string.IsNullOrEmpty(ch.LocalItem)).Select(ch => ch.LocalItem).Distinct())
            {
                MakeFileReadOnly(file);
            }
            return result;
        }

        #region Pending Changes

        public List<PendingChange> PendingChanges { get { return pendingChanges; } }

        public void RefreshPendingChanges()
        {
            this.PendingChanges.Clear();
            var paths = this.Data.WorkingFolders.Select(f => f.LocalItem);
            this.PendingChanges.AddRange(this.GetPendingChanges(paths));
        }

        public List<PendingChange> GetPendingChanges(IEnumerable<ServerItem> items)
        {
            return this.collection.QueryPendingChangesForWorkspace(workspaceData, items.Select(ItemSpec.FromServerItem), false);
        }

        public List<PendingChange> GetPendingChanges(IEnumerable<LocalPath> paths)
        {
            return this.collection.QueryPendingChangesForWorkspace(workspaceData, paths.Select(ItemSpec.FromLocalPath), false);
        }

        public List<PendingSet> GetPendingSets(string item, RecursionType recurse)
        {
            ItemSpec[] items = { new ItemSpec(item, recurse) };
            return this.collection.QueryPendingSets(this.Data.Name, this.Data.Owner, string.Empty, string.Empty, items, false);
        }

        #endregion

        #region GetItems

        public Item GetItem(ItemSpec item, ItemType itemType, bool includeDownloadUrl)
        {
            var items = this.GetItems(item.ToEnumerable(), VersionSpec.Latest, DeletedState.Any, itemType, includeDownloadUrl);
            return items.SingleOrDefault();
        }

        public List<Item> GetItems(IEnumerable<ItemSpec> itemSpecs, VersionSpec versionSpec, DeletedState deletedState, ItemType itemType, bool includeDownloadUrl)
        {
            return this.collection.QueryItems(workspaceData, itemSpecs, versionSpec, deletedState, itemType, includeDownloadUrl);
        }

        public ExtendedItem GetExtendedItem(ItemSpec item, ItemType itemType)
        {
            var items = this.collection.QueryItemsExtended(workspaceData, new[] { item }, DeletedState.Any, itemType);
            return items.SingleOrDefault();
        }

        public List<ExtendedItem> GetExtendedItems(IEnumerable<ItemSpec> itemSpecs, DeletedState deletedState, ItemType itemType)
        {
            return this.collection.QueryItemsExtended(workspaceData, itemSpecs, deletedState, itemType);
        }

        #endregion

        public void Map(string serverPath, string localPath)
        {
            this.Data.WorkingFolders.Add(new WorkingFolder(serverPath, localPath));
            this.Update();
        }

        private void Update()
        {
            this.collection.UpdateWorkspace(this.Data.Name, this.Data.Owner, workspaceData);
        }


        public void ResetDownloadStatus(int itemId)
        {
            var updateVer = new UpdateLocalVersion(itemId, string.Empty, 0);
            var queue = new UpdateLocalVersionQueue(this);
            queue.QueueUpdate(updateVer);
            queue.Flush();
        }

        #region Version Control Operations

        public GetStatus Get(GetRequest request, GetOptions options)
        {
            return Get(request.ToEnumerable(), options);
        }

        public GetStatus Get(IEnumerable<GetRequest> requests, GetOptions options)
        {
            bool force = options.HasFlag(GetOptions.GetAll);
            bool noGet = options.HasFlag(GetOptions.Preview);

            var getOperations = this.collection.Get(workspaceData, requests, force, noGet);   
            ProcessGetOperations(getOperations, ProcessType.Get);
            return new GetStatus(getOperations.Count);
        }

        private void CollectPaths(LocalPath root, List<ChangeRequest> paths)
        {
            if (!root.IsDirectory)
                return;
            foreach (var dir in Directory.EnumerateDirectories(root))
            {
                paths.Add(new ChangeRequest((LocalPath)dir, RequestType.Add, ItemType.Folder));
                CollectPaths(dir, paths);
            }
            paths.AddRange(Directory.EnumerateFiles(root).Select(file => new ChangeRequest((LocalPath) file, RequestType.Add, ItemType.File)));
        }

        public void PendAdd(IEnumerable<LocalPath> paths, bool isRecursive, out ICollection<Failure> failures)
        {
            List<ChangeRequest> changes = new List<ChangeRequest>();

            foreach (var path in paths)
            {
                var itemType = path.IsDirectory ? ItemType.Folder : ItemType.File;
                changes.Add(new ChangeRequest(path, RequestType.Add, itemType));
                if (isRecursive && itemType == ItemType.Folder)
                {
                    CollectPaths(path, changes);
                }
            }

            if (changes.Count == 0)
            {
                failures = new List<Failure>();
                return;
            }

            var operations = this.collection.PendChanges(workspaceData, changes, out failures);
            ProcessGetOperations(operations, ProcessType.Add);
            this.RefreshPendingChanges();
        }

        //Delete from Version Control, but don't delete file from file system - Monodevelop Logic.
        public void PendDelete(IEnumerable<LocalPath> paths, RecursionType recursionType, bool keepLocal, out ICollection<Failure> failures)
        {
            var changes = paths.Select(p => new ChangeRequest(p, RequestType.Delete, Directory.Exists(p) ? ItemType.Folder : ItemType.File, recursionType, LockLevel.None, VersionSpec.Latest)).ToList();

            if (changes.Count == 0)
            {
                failures = new List<Failure>();
                return;
            }

            var getOperations = this.collection.PendChanges(workspaceData, changes, out failures);
            var processType = keepLocal ? ProcessType.DeleteKeep : ProcessType.Delete;
            ProcessGetOperations(getOperations, processType);
            this.RefreshPendingChanges();
        }

        public void PendEdit(IEnumerable<BasePath> paths, RecursionType recursionType, LockLevel lockLevel, out ICollection<Failure> failures)
        {
            var changes = paths.Select(p => new ChangeRequest(p, RequestType.Edit, ItemType.File, recursionType, lockLevel, VersionSpec.Latest)).ToList();
            if (changes.Count == 0)
            {
                failures = new List<Failure>();
                return;
            }

            var getOperations = this.collection.PendChanges(workspaceData, changes, out failures);
            ProcessGetOperations(getOperations, ProcessType.Edit);
            this.RefreshPendingChanges();
        }

        public void PendRename(LocalPath oldPath, LocalPath newPath, out ICollection<Failure> failures)
        {
            List<ChangeRequest> changes = new List<ChangeRequest>
            {
                new ChangeRequest(oldPath, newPath, RequestType.Rename, oldPath.IsDirectory ? ItemType.Folder : ItemType.File)
            };
            var getOperations = this.collection.PendChanges(workspaceData, changes, out failures);
            ProcessGetOperations(getOperations, ProcessType.Rename);
            this.RefreshPendingChanges();
        }

        public List<LocalPath> Undo(IEnumerable<ItemSpec> items)
        {
            var operations = this.collection.UndoPendChanges(workspaceData, items);
            UndoGetOperations(operations);
            this.RefreshPendingChanges();
            return operations.Select(op => op.TargetLocalItem).ToList();
        }

        public void UnLockItems(IEnumerable<BasePath> paths)
        {
            this.LockItems(paths, LockLevel.None);
        }

        public void LockItems(IEnumerable<BasePath> paths, LockLevel lockLevel)
        {
            var changes = paths.Select(p => new ChangeRequest(p, RequestType.Lock, 
                                                              ItemType.Any,
                                                              p.IsDirectory ? RecursionType.Full : RecursionType.None, 
                                                              lockLevel, 
                                                              VersionSpec.Latest)).ToList();
            if (changes.Count == 0)
                return;

            ICollection<Failure> failures;
            var getOperations = this.collection.PendChanges(workspaceData, changes, out failures);
            ProcessGetOperations(getOperations, ProcessType.Get);
            this.RefreshPendingChanges();
        }

        public List<Changeset> QueryHistory(ItemSpec item, VersionSpec versionItem, VersionSpec versionFrom, VersionSpec versionTo, short maxCount)
        {
            return this.collection.QueryHistory(item, versionItem, versionFrom, versionTo, maxCount);
        }

        public Changeset QueryChangeset(int changeSetId, bool includeChanges = false, bool includeDownloadUrls = false, bool includeSourceRenames = true)
        {
            return this.collection.QueryChangeset(changeSetId, includeChanges, includeDownloadUrls, includeSourceRenames);
        }

        public List<Conflict> GetConflicts(IEnumerable<LocalPath> paths)
        {
            var itemSpecs = paths.Select(p => new ItemSpec(p, RecursionType.Full)).ToList();
            return this.collection.QueryConflicts(workspaceData, itemSpecs);
        }

        public void Resolve(Conflict conflict, ResolutionType resolutionType)
        {
            var result = this.collection.Resolve(workspaceData, conflict, resolutionType);
            ProcessGetOperations(result.GetOperations, ProcessType.Get);
            this.Undo(result.UndoOperations.Select(x => new ItemSpec(x.TargetLocalItem, RecursionType.None)));
        }

        public string DownloadToTempWithName(string downloadUrl, string fileName)
        {
            return this.collection.DownloadToTempWithName(downloadUrl, fileName);
        }

        public string DownloadToTemp(string downloadUrl)
        {
            return this.collection.DownloadToTemp(downloadUrl);
        }

        public void UpdateLocalVersion(UpdateLocalVersionQueue updateLocalVersionQueue)
        {
            this.collection.UpdateLocalVersion(this.workspaceData, updateLocalVersionQueue);
        }

        public void CheckOut(IEnumerable<LocalPath> paths, out ICollection<Failure> failures)
        {
            foreach (var localPath in paths)
            {
                Get(new GetRequest(localPath, RecursionType.None, VersionSpec.Latest), GetOptions.GetAll);    
                PendEdit(new[] { localPath }, RecursionType.None, TFSVersionControlService.Instance.CheckOutLockLevel, out failures);
                if (failures.Any())
                    return;
            }
            failures = new List<Failure>();
        }


        #endregion

        internal void MakeFileReadOnly(string path)
        {
            if (File.Exists(path))
                File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.ReadOnly);
        }

        internal void MakeFileWritable(string path)
        {
            if (File.Exists(path))
                File.SetAttributes(path, File.GetAttributes(path) & ~FileAttributes.ReadOnly);
        }

        internal void UnsetDirectoryAttributes(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            FileInfo[] localFiles = dir.GetFiles("*", SearchOption.AllDirectories);
            foreach (FileInfo file in localFiles)
                File.SetAttributes(file.FullName, FileAttributes.Normal);
        }

        #region Equal

        #region IComparable<Workspace> Members

        public int CompareTo(IWorkspace other)
        {
            var nameCompare = string.Compare(this.Data.Name, other.Data.Name, StringComparison.Ordinal);
            if (nameCompare != 0)
                return nameCompare;
            return string.Compare(this.Data.Owner, other.Data.Owner, StringComparison.Ordinal);
        }

        #endregion

        #region IEquatable<Workspace> Members

        public bool Equals(IWorkspace other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return string.Equals(other.Data.Name, this.Data.Name) && string.Equals(other.Data.Owner, this.Data.Owner);
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            var cast = obj as IWorkspace;
            if (cast == null)
                return false;
            return Equals(cast);
        }

        public override int GetHashCode()
        {
            var hash = Data.Name.GetHashCode();
            hash ^= 307 * Data.Owner.GetHashCode();
            return hash;
        }

        public static bool operator ==(Workspace left, Workspace right)
        {
            return ReferenceEquals(null, left) ? ReferenceEquals(null, right) : left.Equals(right);
        }

        public static bool operator !=(Workspace left, Workspace right)
        {
            return !(left == right);
        }

        #endregion Equal

        #region Process Get Operations

        private string DownloadFile(GetOperation operation)
        {
            var path = operation.TargetLocalItem.IsEmpty ? operation.SourceLocalItem : operation.TargetLocalItem;
            if (path.IsEmpty)
                return string.Empty;
            if (operation.ItemType == ItemType.Folder)
            {
                if (!path.Exists())
                    Directory.CreateDirectory(path);
                return path;
            }
            if (operation.ItemType == ItemType.File)
            {
                if (!path.GetDirectory().Exists())
                    Directory.CreateDirectory(path.GetDirectory());
                return this.collection.Download(path, operation.ArtifactUri);
            }
            return string.Empty;
        }

        private UpdateLocalVersion ProcessAdd(GetOperation operation, ProcessDirection processDirection)
        {
            if (processDirection == ProcessDirection.Undo)
            {
                FileHelper.Delete(operation.ItemType, operation.TargetLocalItem);
            }
            return null;
        }

        private UpdateLocalVersion ProcessEdit(GetOperation operation, ProcessDirection processDirection)
        {
            if (processDirection == ProcessDirection.Undo)
            {
                var path = DownloadFile(operation);
                if (operation.ItemType == ItemType.File)
                    MakeFileReadOnly(path);
            }
            else
            {
                string path = string.IsNullOrEmpty(operation.TargetLocalItem) ? operation.SourceLocalItem : operation.TargetLocalItem;
                MakeFileWritable(path);
            }
            return new UpdateLocalVersion(operation.ItemId, operation.TargetLocalItem, operation.VersionServer);
        }

        private UpdateLocalVersion ProcessGet(GetOperation operation, ProcessDirection processDirection)
        {
            if (processDirection == ProcessDirection.Normal)
            {
                var path = DownloadFile(operation);
                if (operation.ItemType == ItemType.File)
                    MakeFileReadOnly(path);
                return new UpdateLocalVersion(operation.ItemId, path, operation.VersionServer);
            }
            return null;
        }

        private UpdateLocalVersion ProcessDelete(GetOperation operation, ProcessDirection processDirection, ProcessType processType)
        {
            if (processDirection == ProcessDirection.Undo)
            {
                var update = ProcessGet(operation, ProcessDirection.Normal);
                _projectService.AddFile(operation.TargetLocalItem);
                return update;
            }
            return InternalProcessDelete(operation, processType);
        }

        private UpdateLocalVersion InternalProcessDelete(GetOperation operation, ProcessType processType)
        {
            var path = operation.SourceLocalItem;
            if (processType == ProcessType.Delete)
            {
                try
                {
                    if (operation.ItemType == ItemType.File)
                    {
                        FileHelper.FileDelete(path);
                    }
                    else
                    {
                        FileHelper.FolderDelete(path);
                    }
                }
                catch
                {
                    _loggingService.LogToInfo("Can not delete path:" + path);
                }
            }
            return new UpdateLocalVersion(operation.ItemId, null, operation.VersionServer);
        }

        private UpdateLocalVersion ProcessRename(GetOperation operation)
        {
            //If the operation is called by Repository OnMoveFile or OnMoveDirectory file/folder is moved before this method.
            //When is called by Source Exporer or By Revert command file is not moved
            bool hasBeenMoved = !FileHelper.Exists(operation.SourceLocalItem) && FileHelper.Exists(operation.TargetLocalItem);
            if (!hasBeenMoved)
            {
                if (operation.ItemType == ItemType.File)
                {
                    FileHelper.FileMove(operation.SourceLocalItem, operation.TargetLocalItem);
                    _projectService.MoveFile(operation.SourceLocalItem, operation.TargetLocalItem);
                }
                else if (operation.ItemType == ItemType.Folder)
                {
                    FileHelper.FolderMove(operation.SourceLocalItem, operation.TargetLocalItem);
                    _projectService.MoveFolder(operation.SourceLocalItem, operation.TargetLocalItem);
                }
            }
            return new UpdateLocalVersion(operation.ItemId, operation.TargetLocalItem, operation.VersionServer);
        }

        private enum ProcessDirection
        {
            Normal,
            Undo
        }

        private enum ProcessType
        {
            Get,
            Add,
            Edit,
            Rename,
            Delete,
            DeleteKeep
        }

        private void ProcessGetOperations(List<GetOperation> getOperations, ProcessType processType)
        {
            if (getOperations == null || getOperations.Count == 0)
                return;
            using (var progressDisplay = _progressService.CreateProgress())
            {
                progressDisplay.BeginTask("Process", getOperations.Count);
                UpdateLocalVersionQueue updates = new UpdateLocalVersionQueue(this);
                foreach (var operation in getOperations)
                {
                    try
                    {
                        if (progressDisplay.IsCancelRequested)
                            break;
                        progressDisplay.BeginTask(processType + " " + operation.TargetLocalItem, 1);
                        UpdateLocalVersion update;

                        switch (processType)
                        {
                            case ProcessType.Add:
                                update = ProcessAdd(operation, ProcessDirection.Normal);
                                break;
                            case ProcessType.Edit:
                                update = ProcessEdit(operation, ProcessDirection.Normal);
                                break;
                            case ProcessType.Get:
                                update = ProcessGet(operation, ProcessDirection.Normal);
                                break;
                            case ProcessType.Rename:
                                update = ProcessRename(operation);
                                break;
                            case ProcessType.Delete:
                            case ProcessType.DeleteKeep:
                                update = ProcessDelete(operation, ProcessDirection.Normal, processType);
                                break;
                            default:
                                update = ProcessGet(operation, ProcessDirection.Normal);
                                break;
                        }

                        if (update != null)
                            updates.QueueUpdate(update);
                    }
                    finally
                    {
                        progressDisplay.EndTask();
                    }
                }
                updates.Flush();
                progressDisplay.EndTask();
            }
        }

        private void UndoGetOperations(List<GetOperation> getOperations)
        {
            if (getOperations == null || getOperations.Count == 0)
                return;
           // IProgressMonitor progress = monitor ?? new MonoDevelop.Ide.ProgressMonitoring.MessageDialogProgressMonitor(DispatchService.IsGuiThread, false, false);
            using (var progressDisplay = _progressService.CreateProgress())
            {
                progressDisplay.BeginTask("Undo", getOperations.Count);
                UpdateLocalVersionQueue updates = new UpdateLocalVersionQueue(this);
                foreach (var operation in getOperations)
                {
                    try
                    {
                        if (progressDisplay.IsCancelRequested)
                            break;
                        string stepName = operation.ChangeType == ChangeType.None ? "Undo " : operation.ChangeType.ToString();
                        progressDisplay.BeginTask(stepName + " " + operation.TargetLocalItem, 1);
                        UpdateLocalVersion update;

                        if (operation.IsAdd)
                        {
                            update = ProcessAdd(operation, ProcessDirection.Undo);
                            if (update != null)
                                updates.QueueUpdate(update);
                            continue;
                        }

                        if (operation.IsDelete)
                        {
                            update = ProcessDelete(operation, ProcessDirection.Undo, ProcessType.Delete);
                            if (update != null)
                                updates.QueueUpdate(update);
                        }

                        if (operation.IsRename)
                        {
                            update = ProcessRename(operation);
                            if (update != null)
                                updates.QueueUpdate(update);
                        }
                        if (operation.IsEdit || operation.IsEncoding)
                        {
                            update = ProcessEdit(operation, ProcessDirection.Undo);
                            if (update != null)
                                updates.QueueUpdate(update);
                        }
                    }
                    finally
                    {
                        progressDisplay.EndTask();
                    }
                }
                updates.Flush();
                progressDisplay.EndTask();
            }
        }

        #endregion

        public string GetItemContent(Item item)
        {
            if (item == null || item.ItemType == ItemType.Folder)
                return string.Empty;
            if (item.DeletionId > 0)
                return string.Empty;
            var tempName = this.collection.DownloadToTemp(item.ArtifactUri);
            var text = item.Encoding > 0 ? File.ReadAllText(tempName, Encoding.GetEncoding(item.Encoding)) :
                       File.ReadAllText(tempName);
            FileHelper.FileDelete(tempName);
            return text;
        }

        public WorkspaceData Data { get { return workspaceData; }}

        public ProjectCollection ProjectCollection { get { return collection; } }

        public override string ToString()
        {
            return "Owner: " + Data.Owner + ", Name: " + Data.Name;
        }
    }
}
