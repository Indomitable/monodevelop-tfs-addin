//
// Microsoft.TeamFoundation.VersionControl.Client.VersionControlServer
//
// Authors:
//	Joel Reed (joelwreed@gmail.com)
//
// Copyright (C) 2007 Joel Reed
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;
using System.Web.Services;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Common;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
	public sealed class VersionControlServer 
	{
		private Repository repository;
		private string authenticatedUser;
		private TeamFoundationServer teamFoundationServer;
		internal Uri uri;

		public event ExceptionEventHandler NonFatalError;
		public event GettingEventHandler Getting;
		public event ProcessingChangeEventHandler BeforeCheckinPendingChange;
		public event PendingChangeEventHandler NewPendingChange;
		public event ConflictEventHandler Conflict;
		public event PendingChangeEventHandler UndonePendingChange;

		//internal event FileTransferEventHandler Uploading;

		public VersionControlServer(TeamFoundationServer teamFoundationServer) 
		{
			ICredentials credentials = teamFoundationServer.Credentials;
			this.teamFoundationServer = teamFoundationServer;
			this.uri = teamFoundationServer.Uri;
			this.repository = new Repository(this, uri, credentials);

			if (credentials != null)
				this.authenticatedUser = credentials.GetCredential(uri, "").UserName;
		}

		public LabelResult[] CreateLabel (VersionControlLabel label,
																			LabelItemSpec[] labelSpecs,
																			LabelChildOption childOption)
		{
			Workspace workspace = GetWorkspace(labelSpecs[0].ItemSpec.Item);
			return repository.LabelItem(workspace, label, labelSpecs, childOption);
		}

		public LabelResult[] UnlabelItem (string labelName, string labelScope,
																			ItemSpec[] itemSpecs, VersionSpec version)
		{
			Workspace workspace = GetWorkspace(itemSpecs[0].Item);
			return repository.UnlabelItem(workspace, labelName, labelScope, 
																		itemSpecs, version);
		}

		public Workspace CreateWorkspace(string name, string owner)
		{
			return CreateWorkspace(name, owner, null, new WorkingFolder[0], Environment.MachineName);
		}

		public Workspace CreateWorkspace(string name, string owner, string comment,
																		 WorkingFolder[] folders, string computer)
		{
			Workspace w1 = new Workspace(this, name, owner, comment, folders, computer);
			Workspace w2 = repository.CreateWorkspace(w1);
			Workstation.Current.AddCachedWorkspaceInfo(ServerGuid, Uri, w2);
			return w2;
		}

		public void DeleteWorkspace(string workspaceName, string workspaceOwner) 
		{
			repository.DeleteWorkspace(workspaceName, workspaceOwner);
			Workstation.Current.RemoveCachedWorkspaceInfo(Uri, workspaceName);
		}

		public void DeleteShelveset(Shelveset shelveset) 
		{
			DeleteShelveset(shelveset.Name, shelveset.OwnerName);
		}

		public void DeleteShelveset(string shelvesetName, string shelvesetOwner) 
		{
			repository.DeleteShelveset(shelvesetName, shelvesetOwner);
		}

		public BranchHistoryTreeItem[][] GetBranchHistory(ItemSpec[] itemSpecs,
																											VersionSpec version)
		{
			if (itemSpecs.Length == 0) return null;

			string workspaceName = String.Empty;
			string workspaceOwner = String.Empty;
			string item = itemSpecs[0].Item;

			if (!VersionControlPath.IsServerItem(item))
				{
					WorkspaceInfo info = Workstation.Current.GetLocalWorkspaceInfo(item);
					if (info != null)
						{
							workspaceName = info.Name;
							workspaceOwner = info.OwnerName;
						}
				}

			return repository.QueryBranches(workspaceName, workspaceOwner,
																			itemSpecs, version);
		}

		public Changeset GetChangeset (int changesetId)
		{
			return GetChangeset(changesetId, false, false);
		}

		public Changeset GetChangeset (int changesetId, bool includeChanges,
																	 bool includeDownloadInfo)
		{
			return repository.QueryChangeset(changesetId, includeChanges, includeDownloadInfo);
		}

		public Item GetItem(int id, int changeSet)
		{
			return GetItem(id, changeSet, false);
		}

		public Item GetItem(int id, int changeSet, bool includeDownloadInfo)
		{
			int[] ids = new int[1];
			ids[0] = id;

			Item[] items = GetItems(ids, changeSet, includeDownloadInfo);
			if (items.Length > 0) return items[0];
			return null;
		}

		public Item[] GetItems(int[] ids, int changeSet)
		{
			return GetItems(ids, changeSet, false);
		}

		public Item GetItem(string path)
		{
			return GetItem(path, VersionSpec.Latest);
		}

		public Item GetItem(string path, VersionSpec versionSpec)
		{
			return GetItem(path, versionSpec, 0, false);
		}

		public Item GetItem(string path, VersionSpec versionSpec,
												int deletionId, bool includeDownloadInfo)
		{
			ItemSpec itemSpec = new ItemSpec(path, RecursionType.None);
			ItemSet itemSet = GetItems(itemSpec, versionSpec, DeletedState.NonDeleted,
																 ItemType.Any, includeDownloadInfo);

			Item[] items = itemSet.Items;
			if (items.Length > 0) return items[0];
			return null;
		}

		public Item[] GetItems(int[] ids, int changeSet, bool includeDownloadInfo)
		{
			return repository.QueryItemsById(ids, changeSet, includeDownloadInfo);
		}

		public ItemSet GetItems(string path, RecursionType recursionType)
		{
			ItemSpec itemSpec = new ItemSpec(path, recursionType);
			return GetItems(itemSpec, VersionSpec.Latest, DeletedState.NonDeleted,
											ItemType.Any, false);
		}

		public ItemSet GetItems(string path, VersionSpec versionSpec, 
														RecursionType recursionType)
		{
			ItemSpec itemSpec = new ItemSpec(path, recursionType);
			return GetItems(itemSpec, versionSpec, DeletedState.NonDeleted,
											ItemType.Any, false);
		}

		public ItemSet GetItems(ItemSpec itemSpec, VersionSpec versionSpec,
														DeletedState deletedState, ItemType itemType, 
														bool includeDownloadInfo)
		{
			List<ItemSpec> itemSpecs = new List<ItemSpec>();
			itemSpecs.Add(itemSpec);
			ItemSet[] itemSet = GetItems(itemSpecs.ToArray(), versionSpec, deletedState,
																	 itemType, includeDownloadInfo);
			return itemSet[0];
		}

		public ItemSet[] GetItems(ItemSpec[] itemSpecs, VersionSpec versionSpec,
															DeletedState deletedState, ItemType itemType, 
															bool includeDownloadInfo)
		{
			if (itemSpecs.Length == 0) return null;

			string workspaceName = String.Empty;
			string workspaceOwner = String.Empty;

			string item = itemSpecs[0].Item;
			if (!VersionControlPath.IsServerItem(item))
				{
					WorkspaceInfo info = Workstation.Current.GetLocalWorkspaceInfo(item);
					if (info != null)
						{
							workspaceName = info.Name;
							workspaceOwner = info.OwnerName;
						}
				}

			return repository.QueryItems(workspaceName, workspaceOwner,
																	 itemSpecs, versionSpec, deletedState,
																	 itemType, includeDownloadInfo);
		}

		public ExtendedItem[] GetExtendedItems(string path,
																					 DeletedState deletedState,
																					 ItemType itemType)
		{
			List<ItemSpec> itemSpecs = new List<ItemSpec>();
			itemSpecs.Add(new ItemSpec(path, RecursionType.OneLevel));
			ExtendedItem[][] items = GetExtendedItems(itemSpecs.ToArray(), deletedState, itemType);
			
			if (items.Length == 0) return null;
			return items[0];
		}

		public ExtendedItem[][] GetExtendedItems (ItemSpec[] itemSpecs,
																							DeletedState deletedState,
																							ItemType itemType)
		{
			return Repository.QueryItemsExtended(null, null,
																					 itemSpecs, deletedState, itemType);
		}

		public int GetLatestChangesetId()
		{
			RepositoryProperties properties = Repository.GetRepositoryProperties();
			return properties.LatestChangesetId;
		}

		public Workspace GetWorkspace(string localPath)
		{
			string path = Path.GetFullPath(localPath);

			WorkspaceInfo info = Workstation.Current.GetLocalWorkspaceInfo(path);
			if (info == null) throw new ItemNotMappedException(path);

			return new Workspace(this, info.Name, info.OwnerName,
													 info.Comment, new WorkingFolder[0], Workstation.Current.Name);
		}

		public Workspace GetWorkspace(string workspaceName, string workspaceOwner)
		{
			return repository.QueryWorkspace(workspaceName, workspaceOwner);
		}

		public IEnumerable QueryHistory (string path, VersionSpec version,
																		 int deletionId, RecursionType recursion,
																		 string user, VersionSpec versionFrom,
																		 VersionSpec versionTo, int maxCount,
																		 bool includeChanges, bool slotMode)
		{
			return QueryHistory(path, version, deletionId, recursion,
													user, versionFrom, versionTo, maxCount,
													includeChanges, slotMode, false);
		}

		public IEnumerable QueryHistory (string path, VersionSpec version,
																		 int deletionId, RecursionType recursion,
																		 string user, VersionSpec versionFrom,
																		 VersionSpec versionToOrig, int maxCount,
																		 bool includeChanges, bool slotMode,
																		 bool includeDownloadInfo)
		{
			ItemSpec itemSpec = new ItemSpec(path, recursion, deletionId);

			string workspaceName = String.Empty;
			string workspaceOwner = String.Empty;

			if (!VersionControlPath.IsServerItem(itemSpec.Item))
				{
					WorkspaceInfo info = Workstation.Current.GetLocalWorkspaceInfo(itemSpec.Item);
					if (info != null)
						{
							workspaceName = info.Name;
							workspaceOwner = info.OwnerName;
						}
				}

			List<Changeset> changes = new List<Changeset>();
			int total = maxCount;
			VersionSpec versionTo = versionToOrig;

			while (total > 0)
				{
					int batchMax = Math.Min(256, total);
					int batchCnt = repository.QueryHistory(workspaceName, workspaceOwner, itemSpec, 
																								 version, user, versionFrom, versionTo,
																								 batchMax, includeChanges, slotMode, 
																								 includeDownloadInfo, ref changes);

					if (batchCnt < batchMax) break;

					total -= batchCnt;
					Changeset lastChangeset = changes[changes.Count - 1];
					versionTo = new ChangesetVersionSpec(lastChangeset.ChangesetId - 1);
				}

			return changes.ToArray();
		}

		public ChangesetMerge[] QueryMerges (string sourcePath, VersionSpec sourceVersion,
																				 string targetPath, VersionSpec targetVersion,
																				 VersionSpec versionFrom, VersionSpec versionTo,
																				 RecursionType recursion)
		{
			string workspaceName = String.Empty;
			string workspaceOwner = String.Empty;

			if (!VersionControlPath.IsServerItem(targetPath))
				{
					WorkspaceInfo info = Workstation.Current.GetLocalWorkspaceInfo(targetPath);
					if (info != null)
						{
							workspaceName = info.Name;
							workspaceOwner = info.OwnerName;
						}
				}

			ItemSpec sourceItem = null;
			if (!String.IsNullOrEmpty(sourcePath)) sourceItem = new ItemSpec(sourcePath, recursion);

			ItemSpec targetItem = new ItemSpec(targetPath, recursion);
			ChangesetMerge[] merges = repository.QueryMerges(workspaceName, workspaceOwner,
																											 sourceItem, sourceVersion,
																											 targetItem,  targetVersion,
																											 versionFrom,  versionTo,
																											 Int32.MaxValue);
			return merges;
		}

		public Workspace GetWorkspace(WorkspaceInfo workspaceInfo)
		{
			if (workspaceInfo == null)
				throw new ArgumentNullException("workspaceInfo");

			return new Workspace(this, workspaceInfo); 
		}

		public VersionControlLabel[] QueryLabels(string labelName, string labelScope, 
																						 string owner, bool includeItems)
		{
			return repository.QueryLabels(null, null, labelName, labelScope, owner, null,
																		VersionSpec.Latest, includeItems, false);
		}

		public VersionControlLabel[] QueryLabels(string labelName, string labelScope, 
																						 string owner, bool includeItems,
																						 string filterItem, VersionSpec versionFilterItem)
		{
			return repository.QueryLabels(null, null, labelName, labelScope, owner, filterItem, 
																		versionFilterItem, includeItems, false);
		}

		public VersionControlLabel[] QueryLabels(string labelName, string labelScope, 
																						 string owner, bool includeItems,
																						 string filterItem, VersionSpec versionFilterItem,
																						 bool generateDownloadUrls)
		{
			return repository.QueryLabels(null, null, labelName, labelScope, owner, filterItem, 
																		versionFilterItem, includeItems, generateDownloadUrls);
		}

		public ItemSecurity[] GetPermissions(string[] items, RecursionType recursion)
		{
			return GetPermissions(null, items, recursion);
		}

		public ItemSecurity[] GetPermissions(string[] identityNames, string[] items, 
																				 RecursionType recursion)
		{
			return Repository.QueryItemPermissions(identityNames, items, recursion);
		}

		public Shelveset[] QueryShelvesets (string shelvesetName, string shelvesetOwner)
		{
			return repository.QueryShelvesets(shelvesetName, shelvesetOwner);
		}

		public Workspace[] QueryWorkspaces(string workspaceName, string ownerName,
																			 string computer) 
		{
			return repository.QueryWorkspaces(workspaceName, ownerName, computer);
		}

		internal void OnDownloading(GettingEventArgs args)
		{
			if (null != Getting) Getting(this, args);
		}

		internal void OnNonFatalError(Workspace workspace, Failure failure)
		{
			if (null != NonFatalError) NonFatalError(workspace, new ExceptionEventArgs(workspace, failure));
		}

		internal void OnUndonePendingChange(Workspace workspace, PendingChange change)
		{
			if (null != UndonePendingChange) UndonePendingChange(workspace, new PendingChangeEventArgs(workspace, change));
		}

		internal void OnUploading()
		{
			//if (null != Uploading) Uploading(null, null);
		}

		public string AuthenticatedUser 
		{
			get { return authenticatedUser; }
		}

		public Guid ServerGuid 
		{
			get { return new Guid(); }
		}

		public TeamFoundationServer TeamFoundationServer
		{
			get { return teamFoundationServer; }
		}

		internal Repository Repository 
		{
			get { return repository; }
		}

		internal Uri Uri
		{
			get { return uri; }
		}

	}

}
