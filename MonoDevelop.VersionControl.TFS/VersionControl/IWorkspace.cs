// IWorkspace.cs
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
using MonoDevelop.VersionControl.TFS.Core.Structure;
using MonoDevelop.VersionControl.TFS.VersionControl.Enums;
using MonoDevelop.VersionControl.TFS.VersionControl.Infrastructure;
using MonoDevelop.VersionControl.TFS.VersionControl.Models;
using MonoDevelop.VersionControl.TFS.WorkItemTracking.Structure;

namespace MonoDevelop.VersionControl.TFS.VersionControl
{
    internal interface IWorkspace : IEquatable<IWorkspace>, IComparable<IWorkspace>
    {
        CheckInResult CheckIn(ICollection<PendingChange> changes, string comment, Dictionary<int, WorkItemCheckinAction> workItems);
        CheckInResult CheckIn(CommitItem[] changes, string comment, Dictionary<int, WorkItemCheckinAction> workItems);
        List<PendingChange> PendingChanges { get; }
        WorkspaceData Data { get; }
        ProjectCollection ProjectCollection { get; }

        void Map(string serverPath, string localPath);
        void ResetDownloadStatus(int itemId);

        List<PendingChange> GetPendingChanges(IEnumerable<BaseItem> items);
        List<PendingSet> GetPendingSets(string item, RecursionType recurse);

        Item GetItem(ItemSpec item, ItemType itemType, bool includeDownloadUrl);
        List<Item> GetItems(IEnumerable<ItemSpec> itemSpecs, VersionSpec versionSpec, DeletedState deletedState, ItemType itemType, bool includeDownloadUrl);

        ExtendedItem GetExtendedItem(ItemSpec item, ItemType itemType);
        List<ExtendedItem> GetExtendedItems(IEnumerable<ItemSpec> itemSpecs, DeletedState deletedState, ItemType itemType);
        
        void Get(GetRequest request, GetOptions options);
        void Get(IEnumerable<GetRequest> requests, GetOptions options);

        void PendAdd(IEnumerable<LocalPath> paths, bool isRecursive, out ICollection<Failure> failures);
        void PendDelete(IEnumerable<LocalPath> paths, RecursionType recursionType, bool keepLocal, out ICollection<Failure> failures);
        void PendEdit(IEnumerable<BasePath> paths, RecursionType recursionType, LockLevel lockLevel, out ICollection<Failure> failures);
        void PendRename(LocalPath oldPath, LocalPath newPath, out ICollection<Failure> failures);
        void CheckOut(IEnumerable<LocalPath> paths, out ICollection<Failure> failures);

        List<LocalPath> Undo(IEnumerable<ItemSpec> items);
        
        void UnLockItems(IEnumerable<BasePath> paths);
        void LockItems(IEnumerable<BasePath> paths, LockLevel lockLevel);
        
        List<Changeset> QueryHistory(ItemSpec item, VersionSpec versionItem, VersionSpec versionFrom, VersionSpec versionTo, short maxCount);
        Changeset QueryChangeset(int changeSetId, bool includeChanges = false, bool includeDownloadUrls = false, bool includeSourceRenames = true);

        List<Conflict> GetConflicts(IEnumerable<LocalPath> paths);
        void Resolve(Conflict conflict, ResolutionType resolutionType);

        LocalPath DownloadToTempWithName(string downloadUrl, string fileName);
        LocalPath DownloadToTemp(string downloadUrl);
        void UpdateLocalVersion(UpdateLocalVersionQueue updateLocalVersionQueue);
        string GetItemContent(Item item);
        void RefreshPendingChanges();
    }
}