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
using System.Text;
using System.Xml.Linq;
using System.Linq;
using MonoDevelop.Core;
using Microsoft.TeamFoundation.VersionControl.Client.Objects;
using Microsoft.TeamFoundation.VersionControl.Client.Enums;
using MonoDevelop.Ide;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client.Enums;
using MonoDevelop.Projects;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
    public sealed class Workspace : IEquatable<Workspace>, IComparable<Workspace>
    {
        #region Constructors

        private Workspace(string name, 
                          string ownerName, string comment, 
                          List<WorkingFolder> folders, string computer)
        {
            this.Name = name;
            this.OwnerName = ownerName;
            this.Comment = comment;
            this.Folders = folders;
            this.Computer = computer;
            PendingChanges = new List<PendingChange>();
        }

        public Workspace(RepositoryService versionControl, string name, 
                         string ownerName, string comment, 
                         List<WorkingFolder>  folders, string computer) 
            : this(name, ownerName, comment, folders, computer)
        {
            this.ProjectCollection = versionControl.Collection;
            this.VersionControlService = versionControl;
        }

        public Workspace(Microsoft.TeamFoundation.Client.ProjectCollection collection, string name, 
                         string ownerName, string comment, 
                         List<WorkingFolder>  folders, string computer)
            : this(name, ownerName, comment, folders, computer)
        {
            this.ProjectCollection = collection;
            this.VersionControlService = collection.GetService<RepositoryService>();
        }

        public Workspace(RepositoryService versionControl, WorkspaceData workspaceData) 
            : this(versionControl, workspaceData.Name, workspaceData.Owner, workspaceData.Comment, workspaceData.WorkingFolders, workspaceData.Computer)
        {
        }

        public Workspace(Microsoft.TeamFoundation.Client.ProjectCollection collection, WorkspaceData workspaceData) 
            : this(collection, workspaceData.Name, workspaceData.Owner, workspaceData.Comment, workspaceData.WorkingFolders, workspaceData.Computer)
        {
        }

        #endregion

        public CheckInResult CheckIn(List<PendingChange> changes, string comment, Dictionary<int, WorkItemCheckinAction> workItems)
        {
            foreach (var change in changes)
            {
                this.VersionControlService.UploadFile(this, change);
            }
            var result = this.VersionControlService.CheckIn(this, changes, comment, workItems);
            if (result.ChangeSet > 0)
            {
                WorkItemManager wm = new WorkItemManager(this.ProjectCollection);
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

        public List<PendingChange> PendingChanges { get; set; }

        public void RefreshPendingChanges()
        {
            this.PendingChanges.Clear();
            var paths = this.Folders.Select(f => f.LocalItem).ToArray();
            this.PendingChanges.AddRange(this.GetPendingChanges(paths, RecursionType.Full));
        }

        public List<PendingChange> GetPendingChanges()
        {
            return GetPendingChanges(VersionControlPath.RootFolder, RecursionType.Full);
        }

        public List<PendingChange> GetPendingChanges(string item)
        {
            return GetPendingChanges(item, RecursionType.None);
        }

        public List<PendingChange> GetPendingChanges(string item, RecursionType rtype)
        {
            return GetPendingChanges(item, rtype, false);
        }

        public List<PendingChange> GetPendingChanges(string item, RecursionType rtype,
                                                     bool includeDownloadInfo)
        {
            string[] items = { item };
            return GetPendingChanges(items, rtype, includeDownloadInfo);
        }

        public List<PendingChange> GetPendingChanges(string[] items, RecursionType rtype)
        {
            return GetPendingChanges(items, rtype, false);
        }

        public List<PendingChange> GetPendingChanges(string[] items, RecursionType rtype,
                                                     bool includeDownloadInfo)
        {

            var itemSpecs = new List<ItemSpec>(items.Select(i => new ItemSpec(i, rtype)));
            return this.VersionControlService.QueryPendingChangesForWorkspace(this, itemSpecs, includeDownloadInfo);
        }

        public List<PendingChange> GetPendingChanges(List<ItemSpec> items)
        {
            return this.VersionControlService.QueryPendingChangesForWorkspace(this, items, false);
        }

        public List<PendingSet> GetPendingSets(string item, RecursionType recurse)
        {
            ItemSpec[] items = { new ItemSpec(item, recurse) };
            return this.VersionControlService.QueryPendingSets(this.Name, this.OwnerName, string.Empty, string.Empty, items, false);
        }

        #endregion

        #region GetItems

        public Item GetItem(string path, ItemType itemType)
        {
            return GetItem(path, itemType, false);
        }

        public Item GetItem(string path, ItemType itemType, bool includeDownloadUrl)
        {
            var itemSpec = new ItemSpec(path, RecursionType.None);
            var items = this.VersionControlService.QueryItems(this, itemSpec, VersionSpec.Latest, DeletedState.Any, itemType, includeDownloadUrl);
            return items.SingleOrDefault();
        }

        public ExtendedItem GetExtendedItem(string path, ItemType itemType)
        {
            var itemSpec = new ItemSpec(path, RecursionType.None);
            var items = this.VersionControlService.QueryItemsExtended(this, itemSpec, DeletedState.Any, itemType);
            return items.SingleOrDefault();
        }

        public List<ExtendedItem> GetExtendedItems(List<ItemSpec> itemSpecs,
                                                   DeletedState deletedState,
                                                   ItemType itemType)
        {
            return this.VersionControlService.QueryItemsExtended(this.Name, this.OwnerName, itemSpecs, deletedState, itemType);
        }

        #endregion

        public bool IsLocalPathMapped(string localPath)
        {
            if (localPath == null)
                throw new ArgumentNullException("localPath");
            return Folders.Any(f => localPath.StartsWith(f.LocalItem, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsServerPathMapped(VersionControlPath serverPath)
        {
            return Folders.Any(f => serverPath.IsChildOrEqualTo(f.ServerItem));
        }

        public VersionControlPath GetServerPathForLocalPath(string localItem)
        {
            var mappedFolder = Folders.FirstOrDefault(f => localItem.StartsWith(f.LocalItem, StringComparison.OrdinalIgnoreCase));
            if (mappedFolder == null)
                return null;
            if (string.Equals(mappedFolder.LocalItem, localItem, StringComparison.OrdinalIgnoreCase))
                return mappedFolder.ServerItem;
            else
            {
                string rest = TfsPath.LocalToServerPath(localItem.Substring(mappedFolder.LocalItem.Length));
                return mappedFolder.ServerItem + rest;
            }
        }

        public string GetLocalPathForServerPath(VersionControlPath serverItem)
        {
            var mappedFolder = Folders.FirstOrDefault(f => serverItem.IsChildOrEqualTo(f.ServerItem));
            if (mappedFolder == null)
                return null;
            if (serverItem == mappedFolder.ServerItem)
                return mappedFolder.LocalItem;
            else
            {
                //string rest = TfsPath.ServerToLocalPath(serverItem.ToString().Substring(mappedFolder.ServerItem.ToString().Length + 1));
                string rest = TfsPath.ServerToLocalPath(serverItem.ChildPart(mappedFolder.ServerItem));
                return Path.Combine(mappedFolder.LocalItem, rest);
            }
        }

        public WorkingFolder GetWorkingFolderForServerItem(string serverItem)
        {
            int maxPath = 0;
            WorkingFolder workingFolder = null;

            foreach (WorkingFolder folder in Folders)
            {
                if (!serverItem.StartsWith(folder.ServerItem, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                if (folder.LocalItem.Length > maxPath)
                {
                    workingFolder = folder;
                    maxPath = folder.LocalItem.Length;
                }
            }

            return workingFolder;
        }

        public void Map(string serverPath, string localPath)
        {
            this.Folders.Add(new WorkingFolder(serverPath, localPath));
            this.Update();
        }

        private void Update()
        {
            this.VersionControlService.UpdateWorkspace(this.Name, this.OwnerName, this);
        }

        #region Version Control Operations

        public GetStatus Get(GetRequest request, GetOptions options, IProgressMonitor monitor = null)
        {
            var requests = new List<GetRequest> { request };
            return Get(requests, options, monitor);
        }

        public GetStatus Get(List<GetRequest> requests, GetOptions options, IProgressMonitor monitor = null)
        {
            bool force = options.HasFlag(GetOptions.GetAll);
            bool noGet = options.HasFlag(GetOptions.Preview);

            var getOperations = this.VersionControlService.Get(this, requests, force, noGet);           
            ProcessGetOperations(getOperations, ProcessType.Get, monitor);
            return new GetStatus(getOperations.Count);
        }

        private void CollectPaths(FilePath root, List<ChangeRequest> paths)
        {
            if (!root.IsDirectory)
                return;
            foreach (var dir in Directory.EnumerateDirectories(root))
            {
                paths.Add(new ChangeRequest(dir, RequestType.Add, ItemType.Folder));
                CollectPaths(dir, paths);
            }
            foreach (var file in Directory.EnumerateFiles(root))
            {
                paths.Add(new ChangeRequest(file, RequestType.Add, ItemType.File));
            }
        }

        public int PendAdd(List<FilePath> paths, bool isRecursive)
        {
            if (paths.Count == 0)
                return 0;

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
            List<Failure> failures;
            var operations = this.VersionControlService.PendChanges(this, changes, out failures);
            ProcessGetOperations(operations, ProcessType.Add);
            this.RefreshPendingChanges();
            return operations.Count;
        }
        //Delete from Version Control, but don't delete file from file system - Monodevelop Logic.
        public void PendDelete(List<FilePath> paths, RecursionType recursionType, out List<Failure> failures)
        {
            if (paths.Count == 0)
            {
                failures = new List<Failure>();
                return;
            }

            var changes = paths.Select(p => new ChangeRequest(p, RequestType.Delete, Directory.Exists(p) ? ItemType.Folder : ItemType.File, recursionType, LockLevel.None, VersionSpec.Latest)).ToList();
            var getOperations = this.VersionControlService.PendChanges(this, changes, out failures);
            ProcessGetOperations(getOperations, ProcessType.Delete);
            this.RefreshPendingChanges();
        }

        public List<Failure> PendEdit(FilePath path, RecursionType recursionType, CheckOutLockLevel checkOutlockLevel)
        {
            return this.PendEdit(new List<FilePath> { path }, recursionType, checkOutlockLevel);
        }

        public List<Failure> PendEdit(List<FilePath> paths, RecursionType recursionType, CheckOutLockLevel checkOutlockLevel)
        {
            if (paths.Count == 0)
                return new List<Failure>();
            LockLevel lockLevel = LockLevel.None;
            if (checkOutlockLevel == CheckOutLockLevel.CheckOut)
                lockLevel = LockLevel.CheckOut;
            else if (checkOutlockLevel == CheckOutLockLevel.CheckIn)
                lockLevel = LockLevel.Checkin;
            var changes = paths.Select(p => new ChangeRequest(p, RequestType.Edit, ItemType.File, recursionType, lockLevel, VersionSpec.Latest)).ToList();
            List<Failure> failures;
            var getOperations = this.VersionControlService.PendChanges(this, changes, out failures);
            ProcessGetOperations(getOperations, ProcessType.Edit);
            this.RefreshPendingChanges();
            return failures;
        }

        private void PendRename(string oldPath, string newPath, ItemType itemType, out List<Failure> failures)
        {
            List<ChangeRequest> changes = new List<ChangeRequest>();
            changes.Add(new ChangeRequest(oldPath, newPath, RequestType.Rename, itemType));
            var getOperations = this.VersionControlService.PendChanges(this, changes, out failures);
            ProcessGetOperations(getOperations, ProcessType.Rename);
            this.RefreshPendingChanges();
        }

        public void PendRenameFile(string oldPath, string newPath, out List<Failure> failures)
        {
            PendRename(oldPath, newPath, ItemType.File, out failures);
        }

        public void PendRenameFolder(string oldPath, string newPath, out List<Failure> failures)
        {
            PendRename(oldPath, newPath, ItemType.Folder, out failures);
        }

        public List<FilePath> Undo(List<ItemSpec> items, IProgressMonitor monitor = null)
        {
            var operations = this.VersionControlService.UndoPendChanges(this, items);
            UndoGetOperations(operations, monitor);
            this.RefreshPendingChanges();
            List<FilePath> undoPaths = new List<FilePath>();
            foreach (var oper in operations)
            {
                undoPaths.Add(oper.TargetLocalItem);
            }
            return undoPaths;
        }

        public void LockFiles(List<string> paths, LockLevel lockLevel)
        {
            SetLock(paths, ItemType.File, lockLevel, RecursionType.None);
        }

        public void LockFolders(List<string> paths, LockLevel lockLevel)
        {
            SetLock(paths, ItemType.File, lockLevel, RecursionType.Full);
        }

        private void SetLock(List<string> paths, ItemType itemType, LockLevel lockLevel, RecursionType recursion)
        {
            if (paths.Count == 0)
                return;

            var changes = paths.Select(p => new ChangeRequest(p, RequestType.Lock, itemType, recursion, lockLevel, VersionSpec.Latest)).ToList();
            List<Failure> failures;
            var getOperations = this.VersionControlService.PendChanges(this, changes, out failures);
            ProcessGetOperations(getOperations, ProcessType.Get);
            this.RefreshPendingChanges();
        }

        public List<Conflict> GetConflicts(IEnumerable<FilePath> paths)
        {
            var itemSpecs = paths.Select(p => new ItemSpec(p, RecursionType.Full)).ToList();
            return this.VersionControlService.QueryConflicts(this, itemSpecs);
        }

        public void Resolve(Conflict conflict, ResolutionType resolutionType)
        {
            var result = this.VersionControlService.Resolve(conflict, resolutionType);
            ProcessGetOperations(result.GetOperations, ProcessType.Get);
            this.Undo(result.UndoOperations.Select(x => new ItemSpec(x.TargetLocalItem, RecursionType.None)).ToList());
        }

        #endregion

        #region Serialization

        internal static Workspace FromXml(RepositoryService versionControl, XElement element)
        {
            string computer = element.Attribute("computer").Value;
            string name = element.Attribute("name").Value;
            string owner = element.Attribute("owner").Value;
            //bool isLocal = Convert.ToBoolean(element.Attribute("islocal").Value);

            string comment = element.Element(XmlNamespaces.GetMessageElementName("Comment")).Value;
            DateTime lastAccessDate = DateTime.Parse(element.Element(XmlNamespaces.GetMessageElementName("LastAccessDate")).Value);
            var folders = new List<WorkingFolder>(element.Element(XmlNamespaces.GetMessageElementName("Folders"))
                                                         .Elements(XmlNamespaces.GetMessageElementName("WorkingFolder"))
                                                         .Select(el => WorkingFolder.FromXml(el)));

            return new Workspace(versionControl, name, owner, comment, folders, computer)
            { 
                LastAccessDate = lastAccessDate 
            };
        }

        internal XElement ToXml(XName elementName)
        {
            var ns = elementName.Namespace;
            XElement element = new XElement(elementName, 
                                   new XAttribute("computer", Computer), 
                                   new XAttribute("name", Name),
                                   new XAttribute("owner", OwnerName), 
                                   new XElement(ns + "Comment", Comment));

            if (Folders != null)
            {
                element.Add(new XElement(ns + "Folders", Folders.Select(f => f.ToXml(ns))));
            }
            return element;
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

        public int CompareTo(Workspace other)
        {
            var nameCompare = string.Compare(Name, other.Name, StringComparison.Ordinal);
            if (nameCompare != 0)
                return nameCompare;
            return string.Compare(OwnerName, other.OwnerName, StringComparison.Ordinal);
        }

        #endregion

        #region IEquatable<Workspace> Members

        public bool Equals(Workspace other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return string.Equals(other.Name, Name) && string.Equals(other.OwnerName, OwnerName);
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            Workspace cast = obj as Workspace;
            if (cast == null)
                return false;
            return Equals(cast);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
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

        private string DownloadFile(GetOperation operation, VersionControlDownloadService downloadService)
        {
            string path = string.IsNullOrEmpty(operation.TargetLocalItem) ? operation.SourceLocalItem : operation.TargetLocalItem;
            if (string.IsNullOrEmpty(path))
                return string.Empty;
            if (operation.ItemType == ItemType.Folder)
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
            if (operation.ItemType == ItemType.File)
            {
                if (!Directory.Exists(Path.GetDirectoryName(path)))
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                return downloadService.Download(path, operation.ArtifactUri);
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

        private UpdateLocalVersion ProcessEdit(GetOperation operation, VersionControlDownloadService downloadService, ProcessDirection processDirection)
        {
            if (processDirection == ProcessDirection.Undo)
            {
                var path = DownloadFile(operation, downloadService);
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

        private UpdateLocalVersion ProcessGet(GetOperation operation, VersionControlDownloadService downloadService, ProcessDirection processDirection)
        {
            if (processDirection == ProcessDirection.Normal)
            {
                var path = DownloadFile(operation, downloadService);
                if (operation.ItemType == ItemType.File)
                    MakeFileReadOnly(path);
                return new UpdateLocalVersion(operation.ItemId, operation.TargetLocalItem, operation.VersionServer);
            }
            return null;
        }

        private UpdateLocalVersion ProcessDelete(GetOperation operation, VersionControlDownloadService downloadService, ProcessDirection processDirection)
        {
            if (processDirection == ProcessDirection.Undo)
            {
                var update = ProcessGet(operation, downloadService, ProcessDirection.Normal);
                var filePath = (FilePath)operation.TargetLocalItem;
                var projects = IdeApp.Workspace.GetAllProjects();
                foreach (var project in projects)
                {
                    if (filePath.IsChildPathOf(project.BaseDirectory))
                    {
                        if (operation.ItemType == ItemType.File)
                            project.AddFile(operation.TargetLocalItem);
                        if (operation.ItemType == ItemType.Folder)
                            project.AddDirectory(operation.TargetLocalItem.Substring(((string)project.BaseDirectory).Length + 1));
                        break;
                    }
                }
                return update;
            }
            else
                return InternalProcessDelete(operation);
        }

        private UpdateLocalVersion InternalProcessDelete(GetOperation operation)
        {
            var path = operation.SourceLocalItem;
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
                LoggingService.Log(MonoDevelop.Core.Logging.LogLevel.Info, "Can not delete path:" + path);
            }
            return new UpdateLocalVersion(operation.ItemId, null, operation.VersionServer);
        }

        private void ProjectMoveFile(MonoDevelop.Projects.Project project, FilePath source, string destination)
        {
            foreach (var file in project.Files)
            {
                if (file.FilePath == source)
                {
                    project.Files.Remove(file);
                    break;
                }
            }
            project.AddFile(destination);
        }

        private Project FindProjectContainingFolder(FilePath folder)
        {
            MonoDevelop.Projects.Project project = null;
            foreach (var prj in IdeApp.Workspace.GetAllProjects())
            {
                foreach (var file in prj.Files)
                {
                    if (file.Subtype == Subtype.Directory && file.FilePath == folder)
                    {
                        project = prj;
                        break;
                    }
                    if (file.Subtype == Subtype.Code && file.FilePath.IsChildPathOf(folder))
                    {
                        project = prj;
                        break;
                    }
                }
            }
            return project;
        }

        private void ProjectMoveFolder(Project project, FilePath source, FilePath destination)
        {
            var filesToMove = new List<ProjectFile>();
            ProjectFile folderFile = null;
            foreach (var file in project.Files)
            {
                if (file.FilePath == source)
                {
                    folderFile = file;
                }
                if (file.FilePath.IsChildPathOf(source))
                {
                    filesToMove.Add(file);
                }
            }
            if (folderFile != null)
                project.Files.Remove(folderFile);

            var relativePath = destination.ToRelative(project.BaseDirectory);
            project.AddDirectory(relativePath);
            foreach (var file in filesToMove)
            {
                project.Files.Remove(file);
                var fileRelativePath = file.FilePath.ToRelative(source);
                var fileToAdd = Path.Combine(destination, fileRelativePath);
                if (FileHelper.HasFolder(fileToAdd))
                {
                    fileRelativePath = ((FilePath)fileToAdd).ToRelative(project.BaseDirectory);
                    project.AddDirectory(fileRelativePath);
                }
                else
                    project.AddFile(fileToAdd);
            }
        }

        private UpdateLocalVersion ProcessRename(GetOperation operation, ProcessDirection processDirection, IProgressMonitor monitor)
        {
            //If the operation is called by Repository OnMoveFile or OnMoveDirectory file/folder is moved before this method.
            //When is called by Source Exporer or By Revert command file is not moved
            bool hasBeenMoved = !FileHelper.Exists(operation.SourceLocalItem) && FileHelper.Exists(operation.TargetLocalItem);
            if (!hasBeenMoved)
            {
                var found = false;
                if (operation.ItemType == ItemType.File)
                {
                    var project = IdeApp.Workspace.GetProjectContainingFile(operation.SourceLocalItem);
                    if (project != null)
                    {
                        found = true;
                        FileHelper.FileMove(operation.SourceLocalItem, operation.TargetLocalItem);
                        this.ProjectMoveFile(project, operation.SourceLocalItem, operation.TargetLocalItem);
                        project.Save(monitor);
                    }
                }
                else
                {
                    var project = FindProjectContainingFolder(operation.SourceLocalItem);
                    if (project != null)
                    {
                        found = true;
                        FileHelper.FolderMove(operation.SourceLocalItem, operation.TargetLocalItem);
                        this.ProjectMoveFolder(project, operation.SourceLocalItem, operation.TargetLocalItem);
                        project.Save(monitor);
                    }
                }
                if (!found)
                {
                    if (operation.ItemType == ItemType.File)
                    {
                        FileHelper.FileMove(operation.SourceLocalItem, operation.TargetLocalItem);
                    }
                    else if (operation.ItemType == ItemType.Folder)
                    {
                        FileHelper.FolderMove(operation.SourceLocalItem, operation.TargetLocalItem);
                    }
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
            Delete
        }

        private void ProcessGetOperations(List<GetOperation> getOperations, ProcessType processType, IProgressMonitor monitor = null)
        {
            if (getOperations == null || getOperations.Count == 0)
                return;
            var downloadService = this.VersionControlService.Collection.GetService<VersionControlDownloadService>();
            IProgressMonitor progress = monitor ?? new MonoDevelop.Ide.ProgressMonitoring.MessageDialogProgressMonitor(DispatchService.IsGuiThread, false, false);
            try
            {
                progress.BeginTask("Process", getOperations.Count);
                UpdateLocalVersionQueue updates = new UpdateLocalVersionQueue(this);
                foreach (var operation in getOperations)
                {
                    try
                    {
                        if (progress.IsCancelRequested)
                            break;
                        progress.BeginTask(processType + " " + operation.TargetLocalItem, 1);
                        UpdateLocalVersion update = null;

                        switch (processType)
                        {
                            case ProcessType.Add:
                                update = ProcessAdd(operation, ProcessDirection.Normal);
                                break;
                            case ProcessType.Edit:
                                update = ProcessEdit(operation, downloadService, ProcessDirection.Normal);
                                break;
                            case ProcessType.Get:
                                update = ProcessGet(operation, downloadService, ProcessDirection.Normal);
                                break;
                            case ProcessType.Rename:
                                update = ProcessRename(operation, ProcessDirection.Normal, progress);
                                break;
                            case ProcessType.Delete:
                                update = ProcessDelete(operation, downloadService, ProcessDirection.Normal);
                                break;
                            default:
                                update = ProcessGet(operation, downloadService, ProcessDirection.Normal);
                                break;
                        }

                        if (update != null)
                            updates.QueueUpdate(update);
                    }
                    finally
                    {
                        progress.EndTask();
                    }
                }
                updates.Flush();
                progress.EndTask();
            }
            finally
            {
                if (monitor == null && progress != null)
                    progress.Dispose();
            }
        }

        private void UndoGetOperations(List<GetOperation> getOperations, IProgressMonitor monitor = null)
        {
            if (getOperations == null || getOperations.Count == 0)
                return;
            var downloadService = this.VersionControlService.Collection.GetService<VersionControlDownloadService>();
            IProgressMonitor progress = monitor ?? new MonoDevelop.Ide.ProgressMonitoring.MessageDialogProgressMonitor(DispatchService.IsGuiThread, false, false);
            try
            {
                progress.BeginTask("Undo", getOperations.Count);
                UpdateLocalVersionQueue updates = new UpdateLocalVersionQueue(this);
                foreach (var operation in getOperations)
                {
                    try
                    {
                        if (progress.IsCancelRequested)
                            break;
                        string stepName = operation.ChangeType == ChangeType.None ? "Undo " : operation.ChangeType.ToString();
                        progress.BeginTask(stepName + " " + operation.TargetLocalItem, 1);
                        UpdateLocalVersion update = null;

                        if (operation.IsAdd)
                            update = ProcessAdd(operation, ProcessDirection.Undo);
                        else if (operation.IsDelete)
                            update = ProcessDelete(operation, downloadService, ProcessDirection.Undo);
                        else if (operation.IsEdit || operation.IsEncoding)
                            update = ProcessEdit(operation, downloadService, ProcessDirection.Undo);
                        else if (operation.IsRename)
                            update = ProcessRename(operation, ProcessDirection.Undo, progress);
                        else
                            update = ProcessGet(operation, downloadService, ProcessDirection.Undo);
                        if (update != null)
                            updates.QueueUpdate(update);
                    }
                    finally
                    {
                        progress.EndTask();
                    }
                }
                updates.Flush();
                progress.EndTask();
            }
            finally
            {
                if (monitor == null && progress != null)
                    progress.Dispose();
            }
        }

        #endregion

        public string GetItemContent(Item item)
        {
            if (item == null || item.ItemType == ItemType.Folder)
                return string.Empty;
            if (item.DeletionId > 0)
                return string.Empty;
            var dowloadService = this.ProjectCollection.GetService<VersionControlDownloadService>();
            var tempName = dowloadService.DownloadToTemp(item.ArtifactUri);
            var text = item.Encoding > 0 ? File.ReadAllText(tempName, Encoding.GetEncoding(item.Encoding)) :
                       File.ReadAllText(tempName);
            FileHelper.FileDelete(tempName);
            return text;
        }

        public string Comment { get; private set; }

        public string Computer { get; private set; }

        public List<WorkingFolder> Folders { get; private set; }

        public string Name { get; private set; }

        public DateTime LastAccessDate { get; private set; }

        public string OwnerName { get; private set; }

        public Microsoft.TeamFoundation.Client.ProjectCollection ProjectCollection { get; set; }

        public RepositoryService VersionControlService { get; set; }

        public override string ToString()
        {
            return "Owner: " + OwnerName + ", Name: " + Name;
        }
    }
}
