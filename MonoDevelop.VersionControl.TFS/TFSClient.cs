using System;

namespace MonoDevelop.VersionControl.TFS
{
    public class TFSClient : VersionControlSystem
    {
        public TFSClient()
        {
        }

        #region implemented abstract members of VersionControlSystem

        protected override Repository OnCreateRepositoryInstance()
        {
            throw new NotImplementedException();
        }

        public override IRepositoryEditor CreateRepositoryEditor(Repository repo)
        {
            throw new NotImplementedException();
        }

        public override string Name
        {
            get
            {
                return "TFS";
            }
        }

        #endregion

        public override bool IsInstalled
        {
            get
            {
                return true;
            }
        }

        public override Repository GetRepositoryReference(MonoDevelop.Core.FilePath path, string id)
        {
            if (path.IsNullOrEmpty)
                return null;
            return null;
        } 
    }
}

