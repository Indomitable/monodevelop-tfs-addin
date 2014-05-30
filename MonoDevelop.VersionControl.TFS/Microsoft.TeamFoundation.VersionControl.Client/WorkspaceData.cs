using System;
using System.Collections.Generic;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
    public sealed class WorkspaceData
    {
        public WorkspaceData()
        {
            WorkingFolders = new List<WorkingFolder>();
        }

        public string Name { get; set; }

        public string Owner { get; set; }

        public string Computer { get; set; }

        public string Comment { get; set; }

        public List<WorkingFolder> WorkingFolders { get; set; }
    }
}

