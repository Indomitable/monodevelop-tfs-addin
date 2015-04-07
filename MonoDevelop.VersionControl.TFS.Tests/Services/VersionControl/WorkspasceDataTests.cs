using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoDevelop.VersionControl.TFS.VersionControl.Structure;
using Xunit;

namespace MonoDevelop.VersionControl.TFS.Tests.Services.VersionControl
{
    public sealed class WorkspasceDataTests
    {
        private readonly WorkspaceData workspaceData;

        public WorkspasceDataTests()
        {
            workspaceData = new WorkspaceData();
            workspaceData.WorkingFolders.AddRange(new []
            {
                new WorkingFolder("$/Path1", "C:\\Path1"), 
                new WorkingFolder("$/Path2/Path2", "C:\\Path2\\Path2"), 
            });
        }

        [Fact]
        public void GetServerPathForLocalPathTest()
        {
            var serverPath = workspaceData.GetServerPathForLocalPath("C:\\Path1\\Path2\\Path3\\File.x");
            Assert.Equal("$/Path1/Path2/Path3/File.x", serverPath);
        }

        [Fact]
        public void LocalPathIsNotMappedTest()
        {
            var serverPath = workspaceData.GetServerPathForLocalPath("C:\\Path3\\File.x");
            Assert.Null(serverPath);
        }

        [Fact]
        public void ServerPathIsNotMappedTest()
        {
            var localPath = workspaceData.GetLocalPathForServerPath("$/Path2/File.x");
            Assert.Null(localPath);
        }

        [Fact]
        public void GetLocalPathForServerPath()
        {
            var localPath = workspaceData.GetLocalPathForServerPath("$/Path2/Path2/Path3/File.x");
            Assert.Equal("C:\\Path2\\Path2\\Path3\\File.x", localPath);
        }
    }
}
