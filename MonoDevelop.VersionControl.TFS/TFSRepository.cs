using System;
using MonoDevelop.VersionControl.TFS.GUI;
using MonoDevelop.Ide;

namespace MonoDevelop.VersionControl.TFS
{
    public class TFSRepository : UrlBasedRepository
    {
        public TFSRepository()
        {
            var builder = new UriBuilder();
            builder.Scheme = "http";
            builder.Host = "server";
            builder.Port = 8080;
            builder.Path = "tfs";
            builder.UserName = "user";
            builder.Password = "password";
            Url = builder.ToString();
        }

        #region implemented abstract members of Repository

        public override string GetBaseText(MonoDevelop.Core.FilePath localFile)
        {
            throw new NotImplementedException();
        }

        protected override Revision[] OnGetHistory(MonoDevelop.Core.FilePath localFile, Revision since)
        {
            throw new NotImplementedException();
        }

        protected override System.Collections.Generic.IEnumerable<VersionInfo> OnGetVersionInfo(System.Collections.Generic.IEnumerable<MonoDevelop.Core.FilePath> paths, bool getRemoteStatus)
        {
            throw new NotImplementedException();
        }

        protected override VersionInfo[] OnGetDirectoryVersionInfo(MonoDevelop.Core.FilePath localDirectory, bool getRemoteStatus, bool recursive)
        {
            throw new NotImplementedException();
        }

        protected override Repository OnPublish(string serverPath, MonoDevelop.Core.FilePath localPath, MonoDevelop.Core.FilePath[] files, string message, MonoDevelop.Core.IProgressMonitor monitor)
        {
            throw new NotImplementedException();
        }

        protected override void OnUpdate(MonoDevelop.Core.FilePath[] localPaths, bool recurse, MonoDevelop.Core.IProgressMonitor monitor)
        {
            throw new NotImplementedException();
        }

        protected override void OnCommit(ChangeSet changeSet, MonoDevelop.Core.IProgressMonitor monitor)
        {
            throw new NotImplementedException();
        }

        protected override void OnCheckout(MonoDevelop.Core.FilePath targetLocalPath, Revision rev, bool recurse, MonoDevelop.Core.IProgressMonitor monitor)
        {
            SourceControlExplorerView view = new SourceControlExplorerView(monitor);
            view.Load(Url);
            IdeApp.Workbench.OpenDocument(view, true);
        }

        protected override void OnRevert(MonoDevelop.Core.FilePath[] localPaths, bool recurse, MonoDevelop.Core.IProgressMonitor monitor)
        {
            throw new NotImplementedException();
        }

        protected override void OnRevertRevision(MonoDevelop.Core.FilePath localPath, Revision revision, MonoDevelop.Core.IProgressMonitor monitor)
        {
            throw new NotImplementedException();
        }

        protected override void OnRevertToRevision(MonoDevelop.Core.FilePath localPath, Revision revision, MonoDevelop.Core.IProgressMonitor monitor)
        {
            throw new NotImplementedException();
        }

        protected override void OnAdd(MonoDevelop.Core.FilePath[] localPaths, bool recurse, MonoDevelop.Core.IProgressMonitor monitor)
        {
            throw new NotImplementedException();
        }

        protected override void OnDeleteFiles(MonoDevelop.Core.FilePath[] localPaths, bool force, MonoDevelop.Core.IProgressMonitor monitor)
        {
            throw new NotImplementedException();
        }

        protected override void OnDeleteDirectories(MonoDevelop.Core.FilePath[] localPaths, bool force, MonoDevelop.Core.IProgressMonitor monitor)
        {
            throw new NotImplementedException();
        }

        protected override string OnGetTextAtRevision(MonoDevelop.Core.FilePath repositoryPath, Revision revision)
        {
            throw new NotImplementedException();
        }

        protected override RevisionPath[] OnGetRevisionChanges(Revision revision)
        {
            throw new NotImplementedException();
        }

        protected override void OnIgnore(MonoDevelop.Core.FilePath[] localPath)
        {
            throw new NotImplementedException();
        }

        protected override void OnUnignore(MonoDevelop.Core.FilePath[] localPath)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region implemented abstract members of UrlBasedRepository

        public override string[] SupportedProtocols
        {
            get
            {
                return new [] { "http", "https" };
            }
        }

        #endregion

    }
}

