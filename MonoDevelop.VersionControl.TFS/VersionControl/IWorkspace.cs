using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Client.Enums;
using Microsoft.TeamFoundation.VersionControl.Client.Objects;
using MonoDevelop.VersionControl.TFS.Core.Structure;
using MonoDevelop.VersionControl.TFS.VersionControl.Models;
using MonoDevelop.VersionControl.TFS.VersionControl.Structure;
using MonoDevelop.VersionControl.TFS.WorkItemTracking.Structure;

namespace MonoDevelop.VersionControl.TFS.VersionControl
{
    internal interface IWorkspace : IEquatable<IWorkspace>, IComparable<IWorkspace>
    {
        CheckInResult CheckIn(List<PendingChange> changes, string comment, Dictionary<int, WorkItemCheckinAction> workItems);
        List<PendingChange> PendingChanges { get; }
        WorkspaceData Data { get; }
        ProjectCollection ProjectCollection { get; }
        List<PendingChange> GetPendingChanges(IEnumerable<ServerItem> items, bool includeDownloadInfo);
        List<PendingChange> GetPendingChanges(IEnumerable<LocalPath> paths, bool includeDownloadInfo);
        List<PendingSet> GetPendingSets(string item, RecursionType recurse);
        Item GetItem(string path, ItemType itemType);
        Item GetItem(string path, ItemType itemType, bool includeDownloadUrl);
        List<Item> GetItems(IEnumerable<ItemSpec> itemSpecs, VersionSpec versionSpec, DeletedState deletedState, ItemType itemType, bool includeDownloadUrl);
        ExtendedItem GetExtendedItem(string path, ItemType itemType);
        List<ExtendedItem> GetExtendedItems(IEnumerable<ItemSpec> itemSpecs, DeletedState deletedState, ItemType itemType);
        void Map(string serverPath, string localPath);
        void ResetDownloadStatus(int itemId);
        GetStatus Get(GetRequest request, GetOptions options);
        GetStatus Get(List<GetRequest> requests, GetOptions options);
        int PendAdd(IEnumerable<LocalPath> paths, bool isRecursive);
        void PendDelete(IEnumerable<LocalPath> paths, RecursionType recursionType, bool keepLocal, out List<Failure> failures);
        List<Failure> PendEdit(IEnumerable<BasePath> paths, RecursionType recursionType, CheckOutLockLevel checkOutlockLevel);
        void PendRename(LocalPath oldPath, LocalPath newPath, out List<Failure> failures);
        List<LocalPath> Undo(IEnumerable<ItemSpec> items);
        void LockItems(IEnumerable<BasePath> paths, LockLevel lockLevel);
        List<Changeset> QueryHistory(ItemSpec item, VersionSpec versionItem, VersionSpec versionFrom, VersionSpec versionTo, short maxCount);
        Changeset QueryChangeset(int changeSetId, bool includeChanges = false, bool includeDownloadUrls = false, bool includeSourceRenames = true);
        List<Conflict> GetConflicts(IEnumerable<LocalPath> paths);
        void Resolve(Conflict conflict, ResolutionType resolutionType);
        string DownloadToTempWithName(string downloadUrl, string fileName);
        string DownloadToTemp(string downloadUrl);
        void UpdateLocalVersion(UpdateLocalVersionQueue updateLocalVersionQueue);
        string GetItemContent(Item item);
        void RefreshPendingChanges();
    }
}