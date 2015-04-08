//
// TfsRevision.cs
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
using Microsoft.TeamFoundation.VersionControl.Client.Objects;
using MonoDevelop.VersionControl.TFS.VersionControl.Models;

namespace MonoDevelop.VersionControl.TFS.Infrastructure.Objects
{
    sealed class TFSRevision : Revision
    {
        public int Version { get; set; }

        public string ItemPath { get; set; }

        public TFSRevision(Repository repo, int version, string itemPath) : base(repo)
        {
            this.Version = version;
            this.ItemPath = itemPath;
        }

        public TFSRevision(Repository repo, string itemPath, Changeset changeset) : 
            this(repo, changeset.ChangesetId, itemPath)
        {
            this.Author = changeset.Committer;
            this.Message = changeset.Comment;
            this.Time = changeset.CreationDate;
        }

        public void Load()
        {
            var repo = (TFSRepository)this.Repository;
            var changeset = repo.Workspace.QueryChangeset(this.Version);
            this.Author = changeset.Committer;
            this.Message = changeset.Comment;
            this.Time = changeset.CreationDate;
        }

        #region implemented abstract members of Revision

        public override Revision GetPrevious()
        {
            if (this.Version <= 0)
                return null;
            var repo = (TFSRepository)this.Repository;
            var changeSets = repo.Workspace.QueryHistory(new ItemSpec(ItemPath, RecursionType.None), 
                                 new ChangesetVersionSpec(this.Version), null, 
                                 new ChangesetVersionSpec(this.Version), 2);
            if (changeSets.Count == 2)
            {
                return new TFSRevision(repo, ItemPath, changeSets[1]);
            }
            return null;
        }

        #endregion

        public override string ToString()
        {
            return Convert.ToString(this.Version);
        }
    }
}

