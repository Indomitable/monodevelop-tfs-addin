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
using Microsoft.TeamFoundation.VersionControl.Common;
using System.Xml.Linq;
using System.Linq;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
    public sealed class Workspace : IEquatable<Workspace>, IComparable<Workspace>
    {
        //        public static readonly string XmlElementName = "Workspace";
        //        private string comment;
        //        private string computer;
        //        private string name;
        //        private string ownerName;
        //        private DateTime lastAccessDate;
        //        private WorkingFolder[] folders;
        //        private VersionControlService service;
        internal Workspace()
        {
        }

        internal Workspace(TfsVersionControlService service, string name, 
                           string ownerName, string comment, 
                           WorkingFolder[] folders, string computer) : this()
        {
            this.VersionControlService = service;
            this.Name = name;
            this.OwnerName = ownerName;
            this.Comment = comment;
            this.Folders = folders;
            this.Computer = computer;
        }

        public Workspace(TfsVersionControlService service, WorkspaceData workspaceData) 
            : this(service, workspaceData.Name, workspaceData.Owner, workspaceData.Comment, workspaceData.WorkingFolders.ToArray(), workspaceData.Computer)
        {
        }

        public int CheckIn(PendingChange[] changes, string comment)
        {
            throw new NotImplementedException();
//            if (changes.Length == 0)
//                return 0;
//
//            List<string> serverItems = new List<string>();
//            SortedList<string, PendingChange> changesByServerPath = new SortedList<string, PendingChange>();
//
//            foreach (PendingChange change in changes)
//            {
//                // upload new or changed files only
//                if ((change.ItemType == ItemType.File) &&
//                    (change.IsAdd || change.IsEdit))
//                {
//                    Repository.CheckInFile(Name, OwnerName, change);
//                    SetFileAttributes(change.LocalItem);
//                }
//
//                serverItems.Add(change.ServerItem);
//                changesByServerPath.Add(change.ServerItem, change);
//            }
//
//            SortedList<string, bool> undoneServerItems = new SortedList<string, bool>();
//            int cset = Repository.CheckIn(this, serverItems.ToArray(), comment, ref undoneServerItems);
//			
//            foreach (string undoneItem in undoneServerItems.Keys)
//            {
//                PendingChange change = changesByServerPath[undoneItem];
//                VersionControlServer.OnUndonePendingChange(this, change);
//            }
//
//            return cset;
        }

        #region Get Pending Changes

        public PendingChange[] GetPendingChanges()
        {
            return GetPendingChanges(VersionControlPath.RootFolder, RecursionType.Full);
        }

        public PendingChange[] GetPendingChanges(string item)
        {
            return GetPendingChanges(item, RecursionType.None);
        }

        public PendingChange[] GetPendingChanges(string item, RecursionType rtype)
        {
            return GetPendingChanges(item, rtype, false);
        }

        public PendingChange[] GetPendingChanges(string item, RecursionType rtype,
                                                 bool includeDownloadInfo)
        {
            string[] items = new string[1];
            items[0] = item;
            return GetPendingChanges(items, rtype, includeDownloadInfo);
        }

        public PendingChange[] GetPendingChanges(string[] items, RecursionType rtype)
        {
            return GetPendingChanges(items, rtype, false);
        }

        public PendingChange[] GetPendingChanges(string[] items, RecursionType rtype,
                                                 bool includeDownloadInfo)
        {
            throw new NotImplementedException();
//            List<ItemSpec> itemSpecs = new List<ItemSpec>();
//            foreach (string item in items)
//            {
//                itemSpecs.Add(new ItemSpec(item, rtype));
//            }
//
//            Failure[] failures = null;
//            PendingChange[] changes = Repository.QueryPendingSets(Name, OwnerName, Name, OwnerName,
//                                          itemSpecs.ToArray(), includeDownloadInfo,
//                                          out failures);
//            foreach (Failure failure in failures)
//            {
//                Console.WriteLine(failure.ToString());
//            }
//
//            return changes;
        }

        #endregion

        public void Delete()
        {
            throw new NotImplementedException();
//            Repository.DeleteWorkspace(Name, OwnerName);
            //Workstation.Current.RemoveCachedWorkspaceInfo(VersionControlServer.Uri, Name);
        }

        #region Get

        public GetStatus Get()
        {
            return Get(VersionSpec.Latest, GetOptions.None);
        }

        public GetStatus Get(VersionSpec versionSpec, GetOptions options)
        {
            GetRequest request = new GetRequest(versionSpec);
            return Get(request, options);
        }

        public GetStatus Get(GetRequest request, GetOptions options)
        {
            GetRequest[] requests = new GetRequest[1];
            requests[0] = request;
            return Get(requests, options);
        }

        public GetStatus Get(string[] items, VersionSpec version,
                             RecursionType recursion, GetOptions options)
        {
            List<GetRequest> requests = new List<GetRequest>();
            foreach (string item in items)
            {
                requests.Add(new GetRequest(item, recursion, version));
            }
						
            return Get(requests.ToArray(), options); 
        }

        public GetStatus Get(GetRequest[] requests, GetOptions options)
        {
            throw new NotImplementedException();
//            bool force = ((GetOptions.Overwrite & options) == GetOptions.Overwrite);
//            bool noGet = false; // not implemented below: ((GetOptions.Preview & options) == GetOptions.Preview);
//
//            SortedList<int, DateTime> changesetDates = new SortedList<int, DateTime>();
//            var getOperations = Repository.Get(Name, OwnerName, requests, force, noGet);
//
//            UpdateLocalVersionQueue updates = new UpdateLocalVersionQueue(this);
//            foreach (GetOperation getOperation in getOperations)
//            {
//                GettingEventArgs args = new GettingEventArgs(this, getOperation);
//
//                if (getOperation.DeletionId != 0)
//                {
//                    if ((getOperation.ItemType == ItemType.Folder) &&
//                        (Directory.Exists(getOperation.SourceLocalItem)))
//                    {
//                        UnsetDirectoryAttributes(getOperation.SourceLocalItem);
//                        Directory.Delete(getOperation.SourceLocalItem, true);
//                    }
//                    else if ((getOperation.ItemType == ItemType.File) &&
//                             (File.Exists(getOperation.SourceLocalItem)))
//                    {
//                        UnsetFileAttributes(getOperation.SourceLocalItem);
//                        File.Delete(getOperation.SourceLocalItem);
//                    }
//                    updates.QueueUpdate(getOperation.ItemId, null, getOperation.VersionServer);
//                }
//                else if ((!string.IsNullOrEmpty(getOperation.TargetLocalItem)) &&
//                         (!string.IsNullOrEmpty(getOperation.SourceLocalItem)) &&
//                         (getOperation.SourceLocalItem != getOperation.TargetLocalItem))
//                {
//                    try
//                    {
//                        File.Move(getOperation.SourceLocalItem, getOperation.TargetLocalItem);
//                    }
//                    catch (IOException)
//                    {
//                        args.Status = OperationStatus.TargetIsDirectory;
//                    }
//                    updates.QueueUpdate(getOperation.ItemId, getOperation.TargetLocalItem, getOperation.VersionServer);
//                }
//                else if (getOperation.ChangeType == ChangeType.None &&
//                         getOperation.VersionServer != 0)
//                {
//                    string path = getOperation.TargetLocalItem;
//                    string directory = path;
//
//                    if (getOperation.ItemType == ItemType.File)
//                        directory = Path.GetDirectoryName(path);
//
//                    if (!Directory.Exists(directory))
//                        Directory.CreateDirectory(directory);
//
//                    if (getOperation.ItemType == ItemType.File)
//                    {
//                        DownloadFile.WriteTo(path, Repository, getOperation.ArtifactUri);
//
//                        // ChangesetMtimes functionality : none standard!
//                        if (true)
//                        {
//                            int cid = getOperation.VersionServer;
//                            DateTime modDate;
//
//                            if (!changesetDates.TryGetValue(cid, out modDate))
//                            {
//                                Changeset changeset = VersionControlServer.GetChangeset(cid);
//                                modDate = changeset.CreationDate;
//                                changesetDates.Add(cid, modDate);
//                            }
//
//                            File.SetLastWriteTime(path, modDate);
//                        }
//
//                        // do this after setting the last write time!
//                        SetFileAttributes(path);
//                    }
//                    updates.QueueUpdate(getOperation.ItemId, path, getOperation.VersionServer);
//                }
//                //versionControlServer.OnDownloading(args);
//            }
//
//            updates.Flush();
//            return new GetStatus(getOperations.Count);
        }

        #endregion

        public List<List<ExtendedItem>> GetExtendedItems(ItemSpec[] itemSpecs,
                                                         DeletedState deletedState,
                                                         ItemType itemType)
        {
            throw new NotImplementedException();
//            return Repository.QueryItemsExtended(Name, OwnerName,
//                itemSpecs, deletedState, itemType);
        }

        public string GetServerItemForLocalItem(string localItem)
        {
            throw new NotImplementedException();
//            string item = TryGetServerItemForLocalItem(localItem);
//            if (item == null)
//                throw new ItemNotMappedException(localItem);
//            return item;
        }

        public string GetLocalItemForServerItem(string serverItem)
        {
            throw new NotImplementedException();
//            string item = TryGetLocalItemForServerItem(serverItem);
//            if (item == null)
//                throw new ItemNotMappedException(serverItem);
//            return item;
        }

        public bool IsLocalPathMapped(string localPath)
        {
            throw new NotImplementedException();

//            foreach (WorkingFolder workingFolder in Folders)
//            {
//                if (localPath.StartsWith(workingFolder.LocalItem))
//                    return true;
//            }
//
//            return false;
        }

        public bool IsServerPathMapped(string serverPath)
        {
            throw new NotImplementedException();
            
//            foreach (WorkingFolder workingFolder in Folders)
//            {
//                if (serverPath.StartsWith(workingFolder.ServerItem, StringComparison.OrdinalIgnoreCase))
//                    return true;
//            }
//
//            return false;
        }

        public void Map(string teamProject, string sourceProject)
        {
            throw new NotImplementedException();
//            WorkingFolder[] folders = new WorkingFolder[1];
//            folders[0] = new WorkingFolder(sourceProject, teamProject);
//            Update(Name, OwnerName, folders);
        }

        public int PendAdd(string path)
        {
            throw new NotImplementedException();
            //return PendAdd(path, false);
        }

        public int PendAdd(string path, bool isRecursive)
        {
            throw new NotImplementedException();
//            string[] paths = new string[1];
//            paths[0] = path;
//            return PendAdd(paths, isRecursive);
        }

        public int PendAdd(string[] paths, bool isRecursive)
        {
            throw new NotImplementedException();
//            List<ChangeRequest> changes = new List<ChangeRequest>();
//
//            foreach (string path in paths)
//            {
//                ItemType itemType = ItemType.File;
//                if (Directory.Exists(path))
//                    itemType = ItemType.Folder;
//                changes.Add(new ChangeRequest(path, RequestType.Add, itemType));
//
//                if (!isRecursive || itemType != ItemType.Folder)
//                    continue;
//
//                DirectoryInfo dir = new DirectoryInfo(path);
//                FileInfo[] localFiles = dir.GetFiles("*", SearchOption.AllDirectories);
//					
//                foreach (FileInfo file in localFiles)
//                    changes.Add(new ChangeRequest(file.FullName, RequestType.Add, ItemType.File));
//            }
//
//            if (changes.Count == 0)
//                return 0;
//
//            GetOperation[] operations = Repository.PendChanges(this, changes.ToArray());
//            return operations.Length;
        }

        public int PendDelete(string path)
        {
            throw new NotImplementedException();
//            return PendDelete(path, RecursionType.None);
        }

        public int PendDelete(string path, RecursionType recursionType)
        {
            throw new NotImplementedException();
//            string[] paths = new string[1];
//            paths[0] = path;
//
//            return PendDelete(paths, recursionType);
        }

        public int PendDelete(string[] paths, RecursionType recursionType)
        {
            throw new NotImplementedException();
//            List<ChangeRequest> changes = new List<ChangeRequest>();
//            foreach (string path in paths)
//            {
//                ItemType itemType = ItemType.File;
//                if (Directory.Exists(path))
//                    itemType = ItemType.Folder;
//                changes.Add(new ChangeRequest(path, RequestType.Delete, itemType));
//            }
//
//            if (changes.Count == 0)
//                return 0;
//
//            GetOperation[] operations = Repository.PendChanges(this, changes.ToArray());
//            UpdateLocalVersionQueue updates = new UpdateLocalVersionQueue(this);
//
//            // first delete all files
//            foreach (GetOperation operation in operations)
//            {
//                if (operation.ItemType != ItemType.File)
//                    continue;
//                if (!File.Exists(operation.SourceLocalItem))
//                    continue;
//
//                UnsetFileAttributes(operation.SourceLocalItem);
//                File.Delete(operation.SourceLocalItem);
//                updates.QueueUpdate(operation.ItemId, null, operation.VersionServer);
//            }
//
//            // then any directories
//            foreach (GetOperation operation in operations)
//            {
//                if (operation.ItemType != ItemType.Folder)
//                    continue;
//                if (!Directory.Exists(operation.SourceLocalItem))
//                    continue;
//
//                //DirectoryInfo dir = new DirectoryInfo(operation.SourceLocalItem);
//                //FileInfo[] localFiles = dir.GetFiles("*", SearchOption.AllDirectories);
//                //foreach (FileInfo file in localFiles)
//                //	UnsetFileAttributes(file.FullName);
//
//                Directory.Delete(operation.SourceLocalItem, true);
//                updates.QueueUpdate(operation.ItemId, null, operation.VersionServer);
//            }
//
//            updates.Flush();
//            return operations.Length;
        }

        public int PendEdit(string path)
        {
            //			Annotation[] annotations = Repository.QueryAnnotation("ExclusiveCheckout",
            //																														path, 0);
            //			foreach (Annotation annotation in annotations)
            //				{
            //					Console.WriteLine(annotation.ToString());
            //				}

            return PendEdit(path, RecursionType.None);
        }

        public int PendEdit(string path, RecursionType recursionType)
        {
            string[] paths = new string[1];
            paths[0] = path;

            return PendEdit(paths, recursionType);
        }

        public int PendEdit(string[] paths, RecursionType recursionType)
        {
            throw new NotImplementedException();
//            List<ChangeRequest> changes = new List<ChangeRequest>();
//            foreach (string path in paths)
//            {
//                changes.Add(new ChangeRequest(path, RequestType.Edit, ItemType.File));
//            }
//
//            if (changes.Count == 0)
//                return 0;
//
//            GetOperation[] getOperations = Repository.PendChanges(this, changes.ToArray());
//            foreach (GetOperation getOperation in getOperations)
//            {
//                UnsetFileAttributes(getOperation.TargetLocalItem);
//            }
//
//            return getOperations.Length;
        }

        public int PendRename(string oldPath, string newPath)
        {
            throw new NotImplementedException();
//            string newServerPath;
//            if (VersionControlPath.IsServerItem(newPath))
//                newServerPath = GetServerItemForLocalItem(newPath);
//            else
//                newServerPath = newPath;
//
//            ItemType itemType = ItemType.File;
//            if (Directory.Exists(oldPath))
//                itemType = ItemType.Folder;
//
//            List<ChangeRequest> changes = new List<ChangeRequest>();
//            changes.Add(new ChangeRequest(oldPath, newServerPath, RequestType.Rename, itemType));
//
//            GetOperation[] getOperations = Repository.PendChanges(this, changes.ToArray());
//			
//            if (itemType == ItemType.File)
//                File.Move(oldPath, newPath);
//            else
//                Directory.Move(oldPath, newPath);
//
//            UpdateLocalVersionQueue updates = new UpdateLocalVersionQueue(this);
//            foreach (GetOperation getOperation in getOperations)
//            {
//                updates.QueueUpdate(getOperation.ItemId, getOperation.TargetLocalItem, getOperation.VersionServer);
//            }
//
//            updates.Flush();
//            return 1;
        }

        public int SetLock(string path, LockLevel lockLevel)
        {
            return SetLock(path, lockLevel, RecursionType.None);
        }

        public int SetLock(string path, LockLevel lockLevel, RecursionType recursion)
        {
            string[] paths = new string[1];
            paths[0] = path;
            return SetLock(paths, lockLevel, recursion);
        }

        public int SetLock(string[] paths, LockLevel lockLevel)
        {
            return SetLock(paths, lockLevel, RecursionType.None);
        }

        public int SetLock(string[] paths, LockLevel lockLevel, RecursionType recursion)
        {
            throw new NotImplementedException();
//            List<ChangeRequest> changes = new List<ChangeRequest>();
//
//            foreach (string path in paths)
//            {
//                ItemType itemType = ItemType.File;
//                if (Directory.Exists(path))
//                    itemType = ItemType.Folder;
//                changes.Add(new ChangeRequest(path, RequestType.Lock, itemType, recursion, lockLevel));
//            }
//
//            if (changes.Count == 0)
//                return 0;
//
//            GetOperation[] operations = Repository.PendChanges(this, changes.ToArray());
//            return operations.Length;
        }

        public void Shelve(Shelveset shelveset, PendingChange[] changes,
                           ShelvingOptions options)
        {
            throw new NotImplementedException();
//            List<string> serverItems = new List<string>();
//
//            foreach (PendingChange change in changes)
//            {
//                // upload new or changed files only
//                if ((change.ItemType == ItemType.File) &&
//                    (change.IsAdd || change.IsEdit))
//                {
//                    Repository.ShelveFile(Name, OwnerName, change);
//                }
//
//                serverItems.Add(change.ServerItem);
//            }
//
//            Repository.Shelve(this, shelveset, serverItems.ToArray(), options);
        }

        internal static Workspace FromXml(TfsVersionControlService service, XElement element)
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

            return new Workspace(service, name, owner, comment, folders.ToArray(), computer)
            { 
                LastAccessDate = lastAccessDate 
            };
        }

        internal XElement ToXml(string elementName)
        {
            XElement element = new XElement(XmlNamespaces.GetMessageElementName(elementName), 
                                   new XAttribute("computer", Computer), 
                                   new XAttribute("name", Name),
                                   new XAttribute("owner", OwnerName), 
                                   new XElement(XmlNamespaces.GetMessageElementName("Comment"), Comment));

            if (Folders != null)
            {
                element.Add(new XElement(XmlNamespaces.GetMessageElementName("Folders"), Folders.Select(f => f.ToXml())));
            }
            return element;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Workspace instance ");
            sb.Append(GetHashCode());

            sb.Append("\n	 Comment: ");
            sb.Append(Comment);

            sb.Append("\n	 LastAccessDate: ");
            sb.Append(LastAccessDate);

            sb.Append("\n	 Folders: ");
            if (Folders != null)
            {
                foreach (WorkingFolder folder in Folders)
                {
                    sb.Append(folder.ToString());
                }
            }

            sb.Append("\n	 Computer: ");
            sb.Append(Computer);

            sb.Append("\n	 Name: ");
            sb.Append(Name);

            sb.Append("\n	 OwnerName: ");
            sb.Append(OwnerName);

            return sb.ToString();
        }

        public string TryGetServerItemForLocalItem(string localItem)
        {
            string serverItem = null;
            int longest = 0;

            // find the longest matching serveritem 
            foreach (WorkingFolder folder in Folders)
            {
                //Console.WriteLine("item: {0} =? folder: {1}", localItem, folder.LocalItem);
                if (!localItem.StartsWith(folder.LocalItem, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                int clen = folder.LocalItem.Length;
                if (clen > longest)
                {
                    serverItem = String.Format("{0}{1}", folder.ServerItem, localItem.Substring(clen));
                    longest = clen;
                }
            }

            return serverItem;
        }

        public string TryGetLocalItemForServerItem(string serverItem)
        {
            string localItem = null;
            int longest = 0;

            foreach (WorkingFolder folder in Folders)
            {
                if (!serverItem.StartsWith(folder.ServerItem, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                int clen = folder.ServerItem.Length;
                if (clen > longest)
                {
                    localItem = String.Format("{0}{1}", folder.LocalItem, serverItem.Substring(clen));
                    longest = clen;
                }
            }
				
            return localItem;
        }

        public WorkingFolder TryGetWorkingFolderForServerItem(string serverItem)
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

        public int Undo(string path)
        {
            return Undo(path, RecursionType.None);
        }

        public int Undo(string path, RecursionType recursionType)
        {
            string[] paths = new string[1];
            paths[0] = path;
            return Undo(paths, recursionType);
        }

        public int Undo(string[] paths, RecursionType recursionType)
        {
            throw new NotImplementedException();
//            List<ItemSpec> specs = new List<ItemSpec>();
//
//            foreach (string path in paths)
//            {
//                specs.Add(new ItemSpec(path, recursionType));
//            }
//
//            // is this the same logic as a workspace Get? 
//            // can we make one function to handle both cases?
//
//            GetOperation[] getOperations = Repository.UndoPendingChanges(Name, OwnerName, specs.ToArray());
//            foreach (GetOperation getOperation in getOperations)
//            {
//                if (getOperation.ChangeType == ChangeType.Edit ||
//                    getOperation.ChangeType == ChangeType.Delete)
//                {
//                    string uPath = getOperation.TargetLocalItem;
//                    string directory = uPath;
//
//                    if (getOperation.ItemType == ItemType.File)
//                        directory = Path.GetDirectoryName(uPath);
//
//                    // directory is null if file is deleted on server, you haven't that
//                    // version yet, you've marked it deleted locally, then you try to 
//                    // undo because it won't you let you checkin the delete because
//                    // its already deleted on the server
//                    if (!Directory.Exists(directory) && !String.IsNullOrEmpty(directory))
//                        Directory.CreateDirectory(directory);
//
//                    if (getOperation.ItemType == ItemType.File)
//                    {
//                        DownloadFile.WriteTo(uPath, Repository, getOperation.ArtifactUri);
//                        SetFileAttributes(uPath);
//                    }
//                }
//            }
//
//            return getOperations.Length;
        }

        public void Update(string newName, string newComment, WorkingFolder[] newMappings)
        {
            throw new NotImplementedException();
//            Workspace w1 = new Workspace(VersionControlServer, newName, OwnerName,
//                               newComment, newMappings, Computer);
//            Workspace w2 = Repository.UpdateWorkspace(Name, OwnerName, w1);
//
//            //Workstation.Current.UpdateWorkspaceInfoCache(VersionControlServer, OwnerName);
//            folders = w2.Folders;
        }

        public void RefreshMappings()
        {
            throw new NotImplementedException();
//            Workspace w = Repository.QueryWorkspace(Name, OwnerName);
//            this.folders = w.folders;
        }

        public string Comment { get; private set; }

        public string Computer { get; private set; }

        public WorkingFolder[] Folders { get; private set; }

        public string Name { get; private set; }

        public DateTime LastAccessDate { get; private set; }

        public string OwnerName { get; private set; }

        public TfsVersionControlService VersionControlService { get; set; }

        internal void SetFileAttributes(string path)
        {
            File.SetAttributes(path, FileAttributes.ReadOnly);
        }

        internal void UnsetFileAttributes(string path)
        {
            File.SetAttributes(path, FileAttributes.Normal);
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

    }
}
