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

            //Situations:
            //1. New file in local workspace and remote repository: ChangeType == ChangeType.Add
            //2. File deleted in past, now we add with same name: ChangeType == ChangeType.None, but VersionLocal == 0 and DeleteId > 0
            if (item.ChangeType.HasFlag(ChangeType.Add) || (item.VersionLocal == 0 && item.DeletionId > 0))
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

            if (item.ChangeType.HasFlag(ChangeType.Rename))
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
                            let path = p.Exists ? (string)p : (string)_repository.Workspace.Data.GetServerPathForLocalPath(p)
                            select new ItemSpec(path, recursion);
            var items = _repository.Workspace.GetExtendedItems(itemSpecs, DeletedState.Any, ItemType.Any);

            var result = new Dictionary<LocalPath, VersionInfoStatus>();
            foreach (var localPath in paths)
            {
                var serverPath = _repository.Workspace.Data.GetServerPathForLocalPath(localPath);
                //Compare server paths, When file is deleted and then is added again Local Path will be empty
                var item = items.SingleOrDefault(i => i.ServerPath == serverPath);
                var versionInfo = item == null ? VersionInfoStatus.Unversioned : new VersionInfoStatus
                {
                    RemotePath = item.ServerPath,
                    LocalStatus = GetLocalVersionStatus(item),
                    LocalRevision = GetLocalRevision(item),
                    //Bug in Monodevelop ? When refresh does not update Remote Status on new items.
                    RemoteStatus = VersionStatus.Versioned, // GetServerVersionStatus(item1),
                    RemoteRevision = GetServerRevision(item)
                };
                result.Add(localPath, versionInfo);
            }
            return result;
        }

    }
}