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

namespace MonoDevelop.VersionControl.TFS
{
    public class TFSRepository : Repository
    {
        public TFSRepository()
        {
//            var builder = new UriBuilder();
//            builder.Scheme = "http";
//            builder.Host = "server";
//            builder.Port = 8080;
//            builder.Path = "tfs";
//            builder.UserName = "user";
//            builder.Password = "password";
//            Url = builder.ToString();
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
//            SourceControlExplorerView view = new SourceControlExplorerView(monitor);
//            view.Load(Url);
//            IdeApp.Workbench.OpenDocument(view, true);
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

