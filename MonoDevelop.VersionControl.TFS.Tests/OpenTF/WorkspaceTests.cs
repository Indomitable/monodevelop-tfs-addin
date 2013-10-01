using System;
using Microsoft.TeamFoundation.VersionControl.Client;
using System.Xml.Linq;
using NUnit.Framework;

namespace MonoDevelop.VersionControl.TFS.Tests.OpenTF
{
    [TestFixture]
    public class WorkspaceTests : BaseTFSConnectTest
    {
        [Test]
        public void CreateWorkspace()
        {
            using (var server = GetServer())
            {
                server.Authenticate();
                var versionControl = server.GetService<VersionControlServer>();
                versionControl.CreateWorkspace("TestWorkSpace1", "mono_tfs_plugin_cp");
            }
        }
    }
}

