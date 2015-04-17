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
        private readonly Dictionary<LocalPath, VersionInfo> _cache = new Dictionary<LocalPath, VersionInfo>();
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
                        _cache.RemoveAll(x => x.Key.IsChildOrEqualOf(path));
                    }
                    _cache.Remove(localPath);
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
                _cache.Clear();
            }
        }

        private void UpdateCache(LocalPath path, VersionInfo status)
        {
            lock (Locker)
            {
                if (_cache.ContainsKey(path))
                    _cache[path] = status;
                else
                    _cache.Add(path, status);
            }
        }

        private VersionInfo Get(LocalPath path)
        {
            lock (Locker)
            {
                if (_cache.ContainsKey(path))
                    return _cache[path];
                return null;
            }
        }

        #endregion

        #region Version Status Info

        private VersionStatus GetLocalVersionStatus(ExtendedItem item)
        {
            var status = VersionStatus.Versioned;

            if (item.IsLocked) //Locked
            {
                if (item.HasOtherPendingChange) //Locked by someone else
                    status |= VersionStatus.Locked;
                else
                    status |= VersionStatus.LockOwned; //Locked by me
            }

            if (item.ChangeType.HasFlag(ChangeType.Add))
            {
                status |= VersionStatus.ScheduledAdd;
                return status;
            }

            if (item.ChangeType.HasFlag(ChangeType.Delete))
            {
                status = status | VersionStatus.ScheduledDelete;
                return status;
            }

            if (item.ChangeType.HasFlag(ChangeType.Edit) || item.ChangeType.HasFlag(ChangeType.Encoding))
            {
                status = status | VersionStatus.Modified;
                return status;
            }

            var changes = _repository.Workspace.PendingChanges.Where(ch => ch.ServerItem == item.ServerPath).ToList();

            if (changes.Any(change => change.IsRename))
            {
                status = status | VersionStatus.ScheduledAdd;
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

        #endregion



        public Dictionary<LocalPath, VersionInfo> GetStatus(IEnumerable<LocalPath> paths, bool recursive)
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
                var statuses = ProcessUnCachedItems(pathsWhichNeedServerRequest, recursive);
                foreach (var status in statuses)
                {
                    var info = ConvertToInfo(status.Key, status.Value);
                    result.Add(status.Key, info);
                    UpdateCache(status.Key, info);
                }
            }
            return result;
        }

        private Dictionary<LocalPath, VersionInfoStatus> ProcessUnCachedItems(List<LocalPath> paths, bool recursive)
        {
            var itemSpecs = from p in paths
                            where _repository.Workspace.Data.IsLocalPathMapped(p)
                            let recursion = p.IsDirectory ? (recursive ? RecursionType.Full : RecursionType.OneLevel) : RecursionType.None
                            select new ItemSpec(p, recursion);
            var items = _repository.Workspace.GetExtendedItems(itemSpecs, DeletedState.Any, ItemType.Any);

            var result = new Dictionary<LocalPath, VersionInfoStatus>();
            foreach (var localPath in paths)
            {
                var item = items.SingleOrDefault(i => i.LocalPath == localPath);
                var versionInfo = item == null ? VersionInfoStatus.Unversioned  : new VersionInfoStatus
                {
                    RemotePath = item.ServerPath,
                    LocalStatus = GetLocalVersionStatus(item),
                    LocalRevision = GetLocalRevision(item),
                    RemoteStatus = GetServerVersionStatus(item),
                    RemoteRevision = GetServerRevision(item)
                };
                result.Add(localPath, versionInfo);
            }
            return result;
        }

    }
}