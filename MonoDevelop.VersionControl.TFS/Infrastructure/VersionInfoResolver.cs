// VersionInfoResolver.cs
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

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MonoDevelop.Core;
using MonoDevelop.VersionControl.TFS.Helpers;
using MonoDevelop.VersionControl.TFS.Infrastructure.Models;
using MonoDevelop.VersionControl.TFS.VersionControl.Enums;
using MonoDevelop.VersionControl.TFS.VersionControl.Infrastructure;
using MonoDevelop.VersionControl.TFS.VersionControl.Models;

namespace MonoDevelop.VersionControl.TFS.Infrastructure
{
    internal sealed class VersionInfoResolver
    {
        private readonly TFSRepository _repository;
        private static readonly Dictionary<LocalPath, VersionInfo> Cache = new Dictionary<LocalPath, VersionInfo>();
        private static readonly object Locker = new object();
        private readonly PropertyInfo _requiredRefresh;

        public VersionInfoResolver(TFSRepository repository)
        {
            _repository = repository;
            //Dirty hack.
            _requiredRefresh = (typeof (VersionInfo)).GetProperty("RequiresRefresh", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        #region Cache

        public void InvalidateCache(IEnumerable<LocalPath> paths, bool recurse = false)
        {
            lock (Locker)
            {
                foreach (var localPath in paths)
                {
                    if (recurse)
                    {
                        var path = localPath;
                        Cache.RemoveAll(x => x.Key.IsChildOrEqualOf(path));
                    }
                    Cache.Remove(localPath);
                }
            }
        }

        public void InvalidateCache(string path)
        {
            InvalidateCache((new LocalPath(path)).ToEnumerable());
        }

        public void InvalidateCache()
        {
            lock (Locker)
            {
                Cache.Clear();
            }
        }

        private void UpdateCache(LocalPath path, VersionInfo status)
        {
            lock (Locker)
            {
                if (Cache.ContainsKey(path))
                    Cache[path] = status;
                else
                    Cache.Add(path, status);
            }
        }

        private VersionInfo Get(LocalPath path)
        {
            lock (Locker)
            {
                if (Cache.ContainsKey(path))
                    return Cache[path];
                return null;
            }
        }

        #endregion

        #region Version Status Info

        private VersionStatus GetLocalVersionStatus(ExtendedItem item, IList<PendingChange> pendingChanges)
        {
            var status = VersionStatus.Versioned;

            if (item.IsLocked) //Locked
            {
                if (item.HasOtherPendingChange) //Locked by someone else
                    status |= VersionStatus.Locked;
                else
                    status |= VersionStatus.LockOwned; //Locked by me
            }

            var changesForItem = pendingChanges.Where(ch => ch.ServerItem == item.ServerPath).ToArray();
            //Situations:
            //1. New file in local workspace and remote repository: ChangeType == ChangeType.Add
            //2. File deleted in past, now we add with same name: ChangeType == ChangeType.None, but VersionLocal == 0 and DeleteId > 0
            if (changesForItem.Any(ch => ch.IsAdd))
            {
                status |= VersionStatus.ScheduledAdd;
                return status;
            }

            if (changesForItem.Any(ch => ch.IsDelete))
            {
                status = status | VersionStatus.ScheduledDelete;
                return status;
            }

            if (changesForItem.Any(ch => ch.IsRename))
            {
                status = status | VersionStatus.ScheduledAdd;
                return status;
            }

            if (changesForItem.Any(ch => ch.IsEdit || ch.IsEncoding))
            {
                status = status | VersionStatus.Modified;
                return status;
            }

            
            return status;
        }

        private VersionStatus GetServerVersionStatus(ExtendedItem item)
        {
            var status = VersionStatus.Versioned;
            if (item.IsLocked)
                status = status | VersionStatus.Locked;
            if (item.DeletionId > 0 || item.VersionLatest == 0)
                return status | VersionStatus.Missing;
            if (item.VersionLatest > item.VersionLocal)
                return status | VersionStatus.Modified;

            return status;
        }

        private TFSRevision GetLocalRevision(ExtendedItem item)
        {
            return new TFSRevision(_repository, item.VersionLocal, item.SourceServerItem);
        }

        private TFSRevision GetServerRevision(ExtendedItem item)
        {
            return new TFSRevision(_repository, item.VersionLatest, item.SourceServerItem);
        }



        #endregion

        public Dictionary<LocalPath, VersionInfo> GetFileStatus(IEnumerable<LocalPath> paths)
        {
            var result = new Dictionary<LocalPath, VersionInfo>();
            var pathsWhichNeedServerRequest = new List<LocalPath>();
            foreach (var path in paths)
            {
                var cachedStatus = Get(path);
                if (cachedStatus == null || (bool)_requiredRefresh.GetValue(cachedStatus))
                {
                    if (!_repository.Workspace.Data.IsLocalPathMapped(path))
                    {
                        var info = ConvertToInfo(path, VersionInfoStatus.Unversioned);
                        result.Add(path, info);
                        UpdateCache(path, info);
                    }
                    else
                    {
                        pathsWhichNeedServerRequest.Add(path);
                    }
                }
                else
                {
                    result.Add(path, cachedStatus);
                }
            }
            if (pathsWhichNeedServerRequest.Any())
            {
                var statuses = ProcessUnCachedItems(pathsWhichNeedServerRequest);
                result.AddRange(statuses);
            }
            return result;
        }


        public Dictionary<LocalPath, VersionInfo> GetDirectoryStatus(LocalPath localPath)
        {
            //For folder status we won't use cache.
            var localItems = localPath.CollectSubPathsAndSelf().ToArray();
            var itemSpecs = new[] { new ItemSpec(_repository.Workspace.Data.GetServerPathForLocalPath(localPath), RecursionType.Full) };
            var pendingChanges = _repository.Workspace.GetPendingChanges(itemSpecs);
            var serverItems = _repository.Workspace.GetExtendedItems(itemSpecs, DeletedState.NonDeleted, ItemType.Any);

            return ExtractVersionInfo(localItems, serverItems, pendingChanges);
        }

        private Dictionary<LocalPath, VersionInfo> ProcessUnCachedItems(List<LocalPath> paths)
        {
            var itemSpecs = (from p in paths
                             where _repository.Workspace.Data.IsLocalPathMapped(p)
                             let recursion = p.IsDirectory ? RecursionType.OneLevel : RecursionType.None
                             let path = p.Exists ? (string)p : (string)_repository.Workspace.Data.GetServerPathForLocalPath(p)
                             select new ItemSpec(path, recursion)).ToList();
            var items = _repository.Workspace.GetExtendedItems(itemSpecs, DeletedState.NonDeleted, ItemType.Any);
            var pendingChanges = _repository.Workspace.GetPendingChanges(itemSpecs);

            return ExtractVersionInfo(paths, items, pendingChanges);
        }

        private Dictionary<LocalPath, VersionInfo> ExtractVersionInfo(IList<LocalPath> localItems, IList<ExtendedItem> serverItems, IList<PendingChange> pendingChanges)
        {
            var result = new Dictionary<LocalPath, VersionInfo>();
            //Fetch all server Items with changes
            foreach (var serverItem in serverItems.Where(si => pendingChanges.Any(pc => pc.ServerItem == si.ServerPath)))
            {
                var info = ConvertToInfo(serverItem.LocalPath, GetVersionInfoStatus(serverItem, pendingChanges));
                result.Add(serverItem.LocalPath, info);
                UpdateCache(serverItem.LocalPath, info);
            }
            //All Mapped w/o changes
            foreach (var serverItem in serverItems.Where(si => !si.LocalPath.IsEmpty && pendingChanges.All(pc => pc.ServerItem != si.ServerPath)))
            {
                var info = ConvertToInfo(serverItem.LocalPath, VersionInfoStatus.Versioned(serverItem.ServerPath));
                result.Add(serverItem.LocalPath, info);
                UpdateCache(serverItem.LocalPath, info);
            }
            //For all local files not mapped with no pending changes.
            foreach (var localItem in localItems.Where(i => serverItems.All(s => s.LocalPath != i) && pendingChanges.All(pc => pc.LocalItem != i)))
            {
                //Check if Item exists on server and local but is market as not downloaded.
                var serverPath = _repository.Workspace.Data.GetServerPathForLocalPath(localItem);
                var item = serverItems.SingleOrDefault(si => si.ServerPath == serverPath);
                VersionInfo info;
                if (item == null)
                {
                    info = ConvertToInfo(localItem, VersionInfoStatus.Unversioned);
                }
                else
                {
                    var status = VersionInfoStatus.Versioned(serverPath);
                    info = ConvertToInfo(localItem, status);
                }
                result.Add(localItem, info);
                UpdateCache(localItem, info);
            }
            return result;
        }


        private VersionInfoStatus GetVersionInfoStatus(ExtendedItem item, IList<PendingChange> pendingChanges)
        {
            if (item == null)
                return VersionInfoStatus.Unversioned;
            return new VersionInfoStatus
            {
                RemotePath = item.ServerPath,
                LocalStatus = GetLocalVersionStatus(item, pendingChanges),
                LocalRevision = GetLocalRevision(item),
                //Bug in Monodevelop ? When refresh does not update Remote Status on new items.
                RemoteStatus = VersionStatus.Versioned, // GetServerVersionStatus(item1),
                RemoteRevision = GetServerRevision(item)
            };
        }

        private VersionInfo ConvertToInfo(LocalPath path, VersionInfoStatus status)
        {
            if (status.IsUnversioned)
                return VersionInfo.CreateUnversioned(new FilePath(path), path.IsDirectory);
            else
            {
                return new VersionInfo(new FilePath(path), status.RemotePath, path.IsDirectory,
                        status.LocalStatus, status.LocalRevision,
                        status.RemoteStatus, status.RemoteRevision);
            }
        }
    }
}