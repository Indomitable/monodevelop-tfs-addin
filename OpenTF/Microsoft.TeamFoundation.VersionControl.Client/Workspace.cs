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
using Microsoft.TeamFoundation.Client;
using MonoDevelop.Core;
using System.Diagnostics;
using MonoDevelop.Ide.Gui;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
	public sealed class Workspace : IEquatable<Workspace>, IComparable<Workspace>
	{
		private Workspace(string name, 
		                  string ownerName, string comment, 
		                  WorkingFolder[] folders, string computer)
		{
			this.Name = name;
			this.OwnerName = ownerName;
			this.Comment = comment;
			this.Folders = folders;
			this.Computer = computer;
			PendingChanges = new List<PendingChange>();
		}

		public Workspace(TfsVersionControlService versionControl, string name, 
		                 string ownerName, string comment, 
		                 WorkingFolder[] folders, string computer) 
            : this(name, ownerName, comment, folders, computer)
		{
			this.ProjectCollection = versionControl.Collection;
			this.VersionControlService = versionControl;
		}

		public Workspace(ProjectCollection collection, string name, 
		                 string ownerName, string comment, 
		                 WorkingFolder[] folders, string computer)
            : this(name, ownerName, comment, folders, computer)
		{
			this.ProjectCollection = collection;
			this.VersionControlService = collection.GetService<TfsVersionControlService>();
		}

		public Workspace(TfsVersionControlService versionControl, WorkspaceData workspaceData) 
            : this(versionControl, workspaceData.Name, workspaceData.Owner, workspaceData.Comment, workspaceData.WorkingFolders.ToArray(), workspaceData.Computer)
		{
		}

		public Workspace(ProjectCollection collection, WorkspaceData workspaceData) 
            : this(collection, workspaceData.Name, workspaceData.Owner, workspaceData.Comment, workspaceData.WorkingFolders.ToArray(), workspaceData.Computer)
		{
		}

		public void CheckIn(List<PendingChange> changes, string comment)
		{
			foreach (var change in changes)
			{
				this.VersionControlService.UploadFile(this, change);
			}

			this.VersionControlService.CheckIn(this, changes, comment);
			this.RefreshPendingChanges();
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

		public List<PendingSet> GetPendingSets(string item, RecursionType recurse)
		{
			ItemSpec[] items = { new ItemSpec(item, recurse) };
			return this.VersionControlService.QueryPendingSets(this.Name, this.OwnerName, string.Empty, string.Empty, items, false);
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
			var requests = new List<GetRequest> { request };
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
						
			return Get(requests, options); 
		}

		public GetStatus Get(List<GetRequest> requests, GetOptions options)
		{
			bool force = ((GetOptions.Overwrite & options) == GetOptions.Overwrite);
			bool noGet = ((GetOptions.Preview & options) == GetOptions.Preview);

			var getOperations = this.VersionControlService.Get(this, requests, force, noGet);           
			ProcessGetOperations(getOperations, false);
			return new GetStatus(getOperations.Count);
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

		public bool IsServerPathMapped(string serverPath)
		{
			return Folders.Any(f => serverPath.StartsWith(f.ServerItem, StringComparison.OrdinalIgnoreCase));
		}

		public VersionControlPath TryGetServerItemForLocalItem(string localItem)
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

		public string TryGetLocalItemForServerItem(string serverItem)
		{
			var mappedFolder = Folders.FirstOrDefault(f => serverItem.StartsWith(f.ServerItem, StringComparison.OrdinalIgnoreCase));
			if (mappedFolder == null)
				return null;
			if (string.Equals(serverItem, mappedFolder.ServerItem, StringComparison.OrdinalIgnoreCase))
				return mappedFolder.LocalItem;
			else
			{
				string rest = TfsPath.ServerToLocalPath(serverItem.Substring(mappedFolder.ServerItem.Length + 1));
				return Path.Combine(mappedFolder.LocalItem, rest);
			}
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

		public void Map(string teamProject, string sourceProject)
		{
			throw new NotImplementedException();
//            WorkingFolder[] folders = new WorkingFolder[1];
//            folders[0] = new WorkingFolder(sourceProject, teamProject);
//            Update(Name, OwnerName, folders);
		}

		#region Pend Add

		public int PendAdd(FilePath path)
		{
			return PendAdd(path, false);
		}

		public int PendAdd(FilePath path, bool isRecursive)
		{
			var paths = new List<FilePath> { path };
			return PendAdd(paths, isRecursive);
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

			foreach (string path in paths)
			{
				ItemType itemType = ItemType.File;
				if (Directory.Exists(path))
					itemType = ItemType.Folder;
				changes.Add(new ChangeRequest(path, RequestType.Add, itemType));
				if (isRecursive && itemType == ItemType.Folder)
				{
					CollectPaths(path, changes);
				}
			}
			var operations = this.VersionControlService.PendChanges(this, changes);
			ProcessGetOperations(operations, false);
			this.RefreshPendingChanges();
			return operations.Count;
		}

		#endregion

		#region Pend Delete

		public void PendDelete(FilePath path)
		{
			PendDelete(path, RecursionType.None);
		}

		public void PendDelete(FilePath path, RecursionType recursionType)
		{
			var paths = new List<FilePath> { path };
			PendDelete(paths, recursionType);
		}
		//Delete from Version Control, but don't delete file from file system - Monodevelop Logic.
		public void PendDelete(List<FilePath> paths, RecursionType recursionType)
		{
			if (paths.Count == 0)
				return;

			var changes = paths.Select(p => new ChangeRequest(p, RequestType.Delete, p.IsDirectory ? ItemType.Folder : ItemType.File, recursionType, LockLevel.None, VersionSpec.Latest)).ToList();
			var getOperations = this.VersionControlService.PendChanges(this, changes);
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

		#endregion

		#region Pend Edit

		public int PendEdit(FilePath path)
		{
			return PendEdit(path, RecursionType.None);
		}

		public int PendEdit(FilePath path, RecursionType recursionType)
		{
			var paths = new List<FilePath> { path };
			return PendEdit(paths, recursionType);
		}

		public int PendEdit(List<FilePath> paths, RecursionType recursionType)
		{
			if (paths.Count == 0)
				return 0;

			var changes = paths.Select(p => new ChangeRequest(p, RequestType.Edit, ItemType.File, recursionType, LockLevel.None, VersionSpec.Latest)).ToList();
			var getOperations = this.VersionControlService.PendChanges(this, changes);
			ProcessGetOperations(getOperations, false);
			foreach (GetOperation getOperation in getOperations)
			{
				MakeFileWritable(getOperation.TargetLocalItem);
			}
			this.RefreshPendingChanges();
			return getOperations.Count;
		}

		#endregion

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

		internal static Workspace FromXml(TfsVersionControlService versionControl, XElement element)
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

			return new Workspace(versionControl, name, owner, comment, folders.ToArray(), computer)
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
			return "Owner: " + OwnerName + ", Name: " + Name;
		}

		public List<FilePath> Undo(string path)
		{
			return Undo(path, RecursionType.None);
		}

		public List<FilePath> Undo(string path, RecursionType recursionType)
		{
			var paths = new List<string> { path };
			return Undo(paths, recursionType);
		}

		public List<FilePath> Undo(List<string> paths, RecursionType recursionType)
		{
			List<ItemSpec> specs = new List<ItemSpec>();

			foreach (string path in paths)
			{
				specs.Add(new ItemSpec(path, recursionType));
			}
			var operations = this.VersionControlService.UndoPendChanges(this, specs);
			ProcessGetOperations(operations, true);
			this.RefreshPendingChanges();
			List<FilePath> undoPaths = new List<FilePath>();
			foreach (var oper in operations)
			{
				undoPaths.Add(oper.SourceLocalItem);
			}
			return undoPaths;
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

		public ProjectCollection ProjectCollection { get; set; }

		public TfsVersionControlService VersionControlService { get; set; }

		internal void MakeFileReadOnly(string path)
		{
			File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.ReadOnly);
		}

		internal void MakeFileWritable(string path)
		{
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

		private void DownloadFile(GetOperation operation, VersionControlDownloadService downloadService)
		{
			string path = string.IsNullOrEmpty(operation.TargetLocalItem) ? operation.SourceLocalItem : operation.TargetLocalItem;
			if (operation.ItemType == ItemType.Folder && !Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			if (operation.ItemType == ItemType.File && !Directory.Exists(Path.GetDirectoryName(path)))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(path));
			}
			if (operation.ItemType == ItemType.File)
			{
				downloadService.Download(path, operation.ArtifactUri);
			}
		}

		private void ProcessEdit(GetOperation operation, VersionControlDownloadService downloadService, bool reverse)
		{
			if (reverse)
			{
				DownloadFile(operation, downloadService);
				if (operation.ItemType == ItemType.File)
					MakeFileReadOnly(operation.TargetLocalItem);
			}
			else
			{
				MakeFileWritable(operation.TargetLocalItem);
			}
		}

		private void ProcessAdd(GetOperation operation, VersionControlDownloadService downloadService, bool reverse)
		{
			//Nothing to implement ???
		}

		private void ProcessGet(GetOperation operation, VersionControlDownloadService downloadService)
		{
			DownloadFile(operation, downloadService);
			if (operation.ItemType == ItemType.File)
				MakeFileReadOnly(operation.TargetLocalItem);
		}

		void ProcessDelete(GetOperation operation)
		{
			var path = operation.SourceLocalItem;
			try
			{
				if (operation.ItemType == ItemType.File)
				{
					if (File.Exists(path))
						File.Delete(path);
				}
				else
				{
					if (Directory.Exists(path))
						Directory.Delete(path, true);
				}
			}
			catch
			{
				LoggingService.Log(MonoDevelop.Core.Logging.LogLevel.Info, "Can not delete path:" + path);
			}
		}

		private void ProcessGetOperations(List<GetOperation> getOperations, bool reverse)
		{
			var downloadService = this.VersionControlService.Collection.GetService<VersionControlDownloadService>();
			UpdateLocalVersionQueue updates = new UpdateLocalVersionQueue(this);
			foreach (var operation in getOperations)
			{
				if (operation.ChangeType.HasFlag(ChangeType.Add))
				{
					ProcessAdd(operation, downloadService, reverse);
					continue; //Add changetype has Edit and Encoding changetypes, no need to process more.
					//updates.QueueUpdate(operation.ItemId, operation.TargetLocalItem, operation.VersionServer);
				}
				if (operation.ChangeType.HasFlag(ChangeType.Edit))
				{
					ProcessEdit(operation, downloadService, reverse);
					updates.QueueUpdate(operation.ItemId, operation.TargetLocalItem, operation.VersionServer);
					continue;
				}
				if (operation.ChangeType.HasFlag(ChangeType.Delete) || operation.DeletionId > 0)
				{
					if (reverse)
					{
						ProcessGet(operation, downloadService);
						updates.QueueUpdate(operation.ItemId, operation.SourceLocalItem, operation.VersionServer);
					}
					else
					{
						ProcessDelete(operation);
						updates.QueueUpdate(operation.ItemId, null, operation.VersionServer);
					}
					continue;
				}
				if (operation.ChangeType.HasFlag(ChangeType.None))
				{
					ProcessGet(operation, downloadService);
					updates.QueueUpdate(operation.ItemId, operation.TargetLocalItem, operation.VersionServer);
				}
			}
			updates.Flush();
		}

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
			File.Delete(tempName);
			return text;
		}
	}
}
