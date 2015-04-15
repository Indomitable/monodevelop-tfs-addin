using System.Collections.Generic;
using Microsoft.TeamFoundation.VersionControl.Client;
using MonoDevelop.VersionControl.TFS.Core.Structure;

namespace MonoDevelop.VersionControl.TFS.Infrastructure.Settings
{
    sealed class Configuration
    {
        public Configuration()
        {
            Servers = new List<TeamFoundationServer>();
            MergeToolInfo = new MergeToolInfo();
        }

        public List<TeamFoundationServer> Servers { get; set; }

        public MergeToolInfo MergeToolInfo { get; set; }

        public LockLevel DefaultLockLevel { get; set; }

        public bool IsDebugMode { get; set; }

        public static Configuration Default()
        {
            return new Configuration
            {
                DefaultLockLevel = LockLevel.Unchanged,
                IsDebugMode = false
            };
        }
    }
}
