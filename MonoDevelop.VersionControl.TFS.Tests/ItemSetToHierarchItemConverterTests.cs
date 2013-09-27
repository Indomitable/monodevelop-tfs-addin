using NUnit.Framework;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Server;

namespace MonoDevelop.VersionControl.TFS.Tests
{
    [TestFixture]
    public class ItemSetToHierarchItemConverterTests : BaseTFSConnectTest
    {
        [Test]
        public void Connect()
        {
            using (var tfsServer = GetServer())
            {
                tfsServer.Authenticate();
            }

        }

        [Test]
        public void TestProject()
        {
            using (var tfsServer = GetServer())
            {
                tfsServer.Authenticate();

                Microsoft.TeamFoundation.VersionControl.Client.VersionControlServer x;
                ICommonStructureService structureService = (ICommonStructureService)tfsServer.GetService(typeof(ICommonStructureService));
                Assert.AreEqual(1, structureService.ListAllProjects().Length);
                Assert.IsNotNull(structureService.GetProjectFromName("TestProjectForTFSPlugin"));
            }
        }
    }
}

