using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoDevelop.VersionControl.TFS.VersionControl.Infrastructure
{
    sealed class CommitItem
    {
        public LocalPath LocalPath { get; set; }
        public RepositoryPath RepositoryPath { get; set; }
        public bool NeedUpload { get; set; }
    }
}
