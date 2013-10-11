//
// TFSRepository.cs
//
// Author:
//       Ventsislav Mladenov <vmladenov.mladenov@gmail.com>
//
// Copyright (c) 2013 Ventsislav Mladenov
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
using Microsoft.TeamFoundation.VersionControl.Client;
using System.Collections.Generic;
using MonoDevelop.Core;
using MonoDevelop.VersionControl.TFS.Infrastructure.Objects;
using System.IO;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using System.Text;
using System.Linq;

namespace MonoDevelop.VersionControl.TFS
{
    public class TFSRepository : Repository
    {
        readonly Workspace workspace;

        public TFSRepository(Workspace workspace)
        {
            if (workspace == null)
                throw new ArgumentNullException("workspace");
            this.workspace = workspace;
            MonitorFileChanged();
        }

        private void MonitorFileChanged()
        {
            IdeApp.Workbench.DocumentOpened += (sender, de) =>
            {
                if (!de.Document.IsFile)
                    return;
                if ((File.GetAttributes(de.Document.FileName) & FileAttributes.ReadOnly) != FileAttributes.ReadOnly)
                    return;
                var content = de.Document.ActiveView.GetContent<IViewContent>();
                if (content != null)
                {
                    content.DirtyChanged += (s, e) => workspace.CheckOutFile(de.Document.FileName);
                }
            };
        }

        #region implemented abstract members of Repository

        public override string GetBaseText(FilePath localFile)
        {
            var serverPath = this.workspace.TryGetServerItemForLocalItem(localFile);
            if (string.IsNullOrEmpty(serverPath))
                return string.Empty;

            var item = this.workspace.GetItem(serverPath, ItemType.File, true);
            if (item == null)
                return string.Empty;
            if (item.DeletionId > 0)
                return string.Empty;
            var dowloadService = workspace.ProjectCollection.GetService<VersionControlDownloadService>();
            var tempName = dowloadService.DownloadToTemp(item.ArtifactUri);
            var text = item.Encoding > 0 ? File.ReadAllText(tempName, Encoding.GetEncoding(item.Encoding)) :
                       File.ReadAllText(tempName);
            File.Delete(tempName);
            return text;
        }

        protected override Revision[] OnGetHistory(FilePath localFile, Revision since)
        {
            throw new NotImplementedException();
        }

        protected VersionStatus GetLocalVersionStatus(IItem item, FilePath fileName)
        {
            if (item == null)
                return VersionStatus.Unversioned;
            var pendingChanges = workspace.GetPendingChanges(item.ServerPath, RecursionType.None, false);
            var status = VersionStatus.Versioned;
            if (pendingChanges.Count > 0)
            {
                foreach (var change in pendingChanges)
                {
                    if (change.ChangeType.HasFlag(ChangeType.Add))
                        status = status | VersionStatus.ScheduledAdd;

                    if (change.ChangeType.HasFlag(ChangeType.Edit))
                        status = status | VersionStatus.Modified;

                    if (change.ChangeType.HasFlag(ChangeType.Delete))
                        status = status | VersionStatus.ScheduledDelete;

                    if (change.ChangeType.HasFlag(ChangeType.Lock))
                        status = status | VersionStatus.Locked;
                }
            }
            return status;
        }

        protected VersionStatus GetServerVersionStatus(ExtendedItem item)
        {
            if (item == null)
                return VersionStatus.Unversioned;
            if (item.DeletionId > 0)
                return VersionStatus.Missing;
            if (item.VersionLatest > item.VersionLocal)
                return VersionStatus.Modified;
            return VersionStatus.Versioned;
        }

        protected Revision GetLocalRevision(IItem item)
        {
            return new TfsRevision(this);
        }

        protected Revision GetServerRevision(ExtendedItem item)
        {
            return new TfsRevision(this);
        }

        protected override IEnumerable<VersionInfo> OnGetVersionInfo(IEnumerable<FilePath> paths, bool getRemoteStatus)
        {
            foreach (var path in paths)
            {
                if (path.IsDirectory)
                {
                    foreach (var vi in OnGetDirectoryVersionInfo(path, getRemoteStatus, false))
                    {
                        yield return vi;
                    }
                }
                else
                {
                    yield return GetFileVersionInfo(path, getRemoteStatus);
                }
            }
        }

        private VersionInfo GetFileVersionInfo(FilePath path, bool getRemoteStatus)
        {
            var serverPath = this.workspace.TryGetServerItemForLocalItem(path);
            if (string.IsNullOrEmpty(serverPath))
                return new VersionInfo(path, string.Empty, false, VersionStatus.Unversioned, null, VersionStatus.Unversioned, null);

            IItem item = getRemoteStatus ? (IItem)this.workspace.GetExtendedItem(serverPath, ItemType.File) :
                         this.workspace.GetItem(serverPath, ItemType.File);
            if (item == null)
                return new VersionInfo(path, serverPath, false, VersionStatus.Unversioned, null, VersionStatus.Unversioned, null);

            return new VersionInfo(path, item.ServerPath, false, 
                GetLocalVersionStatus(item, path), GetLocalRevision(item), 
                getRemoteStatus ? GetServerVersionStatus((ExtendedItem)item) : VersionStatus.Versioned,
                getRemoteStatus ? GetServerRevision((ExtendedItem)item) : null);
        }

        private VersionInfo GetDirectoryVersionInfo(FilePath path, bool getRemoteStatus)
        {
            var serverPath = this.workspace.TryGetServerItemForLocalItem(path);
            IItem item = getRemoteStatus ? (IItem)this.workspace.GetExtendedItem(serverPath, ItemType.Folder) :
                         this.workspace.GetItem(serverPath, ItemType.Folder);

            return new VersionInfo(path, item.ServerPath, false, 
                GetLocalVersionStatus(item, path), GetLocalRevision(item), 
                getRemoteStatus ? GetServerVersionStatus((ExtendedItem)item) : VersionStatus.Versioned,
                getRemoteStatus ? GetServerRevision((ExtendedItem)item) : null);
        }

        public void CollectDirectoryVersionInfo(List<VersionInfo> infos, FilePath localDirectory, bool getRemoteStatus, bool recursive)
        {
            infos.Add(GetDirectoryVersionInfo(localDirectory, getRemoteStatus));
            foreach (var dir in Directory.EnumerateDirectories(localDirectory))
            {
                infos.Add(GetDirectoryVersionInfo(dir, getRemoteStatus));
                if (recursive)
                {
                    CollectDirectoryVersionInfo(infos, dir, getRemoteStatus, recursive);
                }
            }
            foreach (var file in Directory.EnumerateFiles(localDirectory))
            {
                infos.Add(GetFileVersionInfo(file, getRemoteStatus));
            }
        }

        protected override VersionInfo[] OnGetDirectoryVersionInfo(FilePath localDirectory, bool getRemoteStatus, bool recursive)
        {
            List<VersionInfo> infos = new List<VersionInfo>();
            CollectDirectoryVersionInfo(infos, localDirectory, getRemoteStatus, recursive);
            return infos.ToArray();
        }

        protected override Repository OnPublish(string serverPath, FilePath localPath, FilePath[] files, string message, IProgressMonitor monitor)
        {
            throw new NotImplementedException();
        }

        protected override void OnUpdate(FilePath[] localPaths, bool recurse, IProgressMonitor monitor)
        {
            throw new NotImplementedException();
        }

        protected override void OnCommit(ChangeSet changeSet, IProgressMonitor monitor)
        {
            throw new NotImplementedException();
        }

        protected override void OnCheckout(FilePath targetLocalPath, Revision rev, bool recurse, IProgressMonitor monitor)
        {
//            SourceControlExplorerView view = new SourceControlExplorerView(monitor);
//            view.Load(Url);
//            IdeApp.Workbench.OpenDocument(view, true);
        }

        protected override void OnRevert(FilePath[] localPaths, bool recurse, IProgressMonitor monitor)
        {
            string[] paths = new string[localPaths.Length];
            for (int i = 0; i < localPaths.Length; i++)
            {
                paths[i] = localPaths[i];
            }
            workspace.Undo(paths, recurse ? RecursionType.Full : RecursionType.None);
            FileService.NotifyFilesChanged(localPaths.Where(x => File.Exists(x)));
            FileService.NotifyFilesRemoved(localPaths.Where(x => !File.Exists(x)));
        }

        protected override void OnRevertRevision(FilePath localPath, Revision revision, IProgressMonitor monitor)
        {
            throw new NotImplementedException();
        }

        protected override void OnRevertToRevision(FilePath localPath, Revision revision, IProgressMonitor monitor)
        {
            throw new NotImplementedException();
        }

        protected override void OnAdd(FilePath[] localPaths, bool recurse, IProgressMonitor monitor)
        {
            throw new NotImplementedException();
        }

        protected override void OnDeleteFiles(FilePath[] localPaths, bool force, IProgressMonitor monitor)
        {
            throw new NotImplementedException();
        }

        protected override void OnDeleteDirectories(FilePath[] localPaths, bool force, IProgressMonitor monitor)
        {
            throw new NotImplementedException();
        }

        protected override string OnGetTextAtRevision(FilePath repositoryPath, Revision revision)
        {
            throw new NotImplementedException();
        }

        protected override RevisionPath[] OnGetRevisionChanges(Revision revision)
        {
            throw new NotImplementedException();
        }

        protected override void OnIgnore(FilePath[] localPath)
        {
            throw new NotImplementedException();
        }

        protected override void OnUnignore(FilePath[] localPath)
        {
            throw new NotImplementedException();
        }

        #endregion

        public override bool RequestFileWritePermission(FilePath path)
        {
            if (!File.Exists(path))
                return true;
            if ((File.GetAttributes(path) & FileAttributes.ReadOnly) == 0)
                return true;
//            try
//            {
//                //workspace.
//            }
//            catch
//            {
//
//            }
//            VersionControlService.NotifyFileStatusChanged (new FileUpdateEventArgs (this, path, false));
            return true;
        }
        //        #region implemented abstract members of UrlBasedRepository
        //
        //        public override string[] SupportedProtocols
        //        {
        //            get
        //            {
        //                return new [] { "http", "https" };
        //            }
        //        }
        //
        //        #endregion
    }
}

