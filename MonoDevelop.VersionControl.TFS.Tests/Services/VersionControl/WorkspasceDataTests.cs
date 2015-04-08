using System;
using MonoDevelop.VersionControl.TFS.VersionControl.Structure;
using Xunit;

namespace MonoDevelop.VersionControl.TFS.Tests.Services.VersionControl
{
    public sealed class PathTests
    {
        private readonly WorkspaceData workspaceData;

        public PathTests()
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
            Assert.Equal(new RepositoryPath("$/Path1/Path2/Path3/File.x", false), serverPath);
            serverPath = workspaceData.GetServerPathForLocalPath("C:\\Path1");
            Assert.Equal(new RepositoryPath("$/Path1", true), serverPath);
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
            var localPath = workspaceData.GetLocalPathForServerPath(new RepositoryPath("$/Path2/File.x", false));
            Assert.Null(localPath);
        }

        [Fact]
        public void GetLocalPathForServerPath()
        {
            var localPath = workspaceData.GetLocalPathForServerPath(new RepositoryPath("$/Path2/Path2/Path3/File.x", false));
            Assert.Equal("C:\\Path2\\Path2\\Path3\\File.x", localPath);
        }

        [Fact]
        public void RepositoryPath1()
        {
            var path = new RepositoryPath("$/Project1/Something", true);
            Assert.False(path.IsRoot);
            Assert.True(path.IsDirectory);
            Assert.Equal("Something", path.ItemName);
            Assert.Equal("$/Project1/", path.ParentPath);
            Assert.True(path.IsChildOrEqualOf(new RepositoryPath("$/Project1", true)));
            Assert.True(path.IsChildOrEqualOf(RepositoryPath.RootPath));
            Assert.False(path.IsChildOrEqualOf(new RepositoryPath("$/Project1", false)));
            Assert.Equal("$/", RepositoryPath.RootPath);

            Assert.Equal("Project1/Something/", path.ToRelativeOf(RepositoryPath.RootPath));
        }

        [Fact]
        public void RepositoryPath2()
        {
            var path = new RepositoryPath("$/Project1/Something", false);
            Assert.False(path.IsRoot);
            Assert.False(path.IsDirectory);
            Assert.Equal("Something", path.ItemName);
            Assert.Equal("$/Project1/", path.ParentPath);
        }

        [Fact]
        public void RepositoryPath3()
        {
            var path = new RepositoryPath("$", true);
            Assert.True(path.IsRoot);
            Assert.True(path.IsDirectory);
            Assert.Equal(RepositoryPath.RootPath, path.ItemName);
            Assert.Null(path.ParentPath);
        }

        [Fact]
        public void RepositoryPath4()
        {
            var path = new RepositoryPath("$/Project1/Something", true);
            Assert.Equal("Project1/Something/", path.ToRelativeOf(RepositoryPath.RootPath));
            path = new RepositoryPath("$/Project1/Something", false);
            Assert.Equal("Project1/Something", path.ToRelativeOf(RepositoryPath.RootPath));
        }


        [Fact]
        public void LocalPath1()
        {
            var path = new LocalPath("C:\\bgfdxhgbfdfcsz\\dsldflgdgdjhui");
            Assert.False(path.IsDirectory);
            Assert.False(path.IsEmpty);
            Assert.True(path.IsChildOrEqualOf("C:\\bgfdxhgbfdfcsz"));
            Assert.False(path.IsChildOrEqualOf("C:\\bgfdxhgbfd"));
            Assert.True(path.IsChildOrEqualOf("C:\\bgfdxhgbfdfcsz\\dsldflgdgdjhui"));
            Assert.Equal("dsldflgdgdjhui", path.ToRelativeOf("C:\\bgfdxhgbfdfcsz"));
            Assert.Throws<Exception>(() => path.ToRelativeOf("C:\\bgfdxhgbf"));
        }

    }
}
