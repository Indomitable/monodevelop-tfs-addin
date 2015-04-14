using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Client.Enums;
using Microsoft.TeamFoundation.VersionControl.Client.Objects;
using MonoDevelop.VersionControl.TFS.Core.Structure;
using MonoDevelop.VersionControl.TFS.Tests.Core;
using MonoDevelop.VersionControl.TFS.VersionControl.Models;
using MonoDevelop.VersionControl.TFS.VersionControl.Structure;
using Xunit;

namespace MonoDevelop.VersionControl.TFS.Tests.Services.VersionControl
{
    [Collection("Server")]
    public sealed class RepositoryTests
    {
        private readonly ServerFixture _fixture;
        private ProjectInfo _project;

        public RepositoryTests(ServerFixture fixture)
        {
            _fixture = fixture;
            _project = _fixture.Server.ProjectCollections.SelectMany(pc => pc.Projects).Single();
        }

        [Fact]
        public void GetRoot()
        {
            var workspace = _fixture.GetWorkspace(_project.Collection);
            var item = workspace.GetExtendedItem(ItemSpec.FromServerPath(RepositoryPath.RootPath), ItemType.Folder);
            Assert.NotNull(item);
            Assert.Equal(item.ServerPath, RepositoryPath.RootPath);
        }

        [Fact]
        public void AddAndDeleteFile()
        {
            var workspace = _fixture.GetWorkspace(_project.Collection);
            //Prepare file.
            var directory = _fixture.GetWorkspaceTopFolder();


            var fileName = Path.Combine(directory, Path.GetRandomFileName());
            string content;
            using (var cryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                var buffer = new byte[200];
                cryptoServiceProvider.GetBytes(buffer);
                content = Convert.ToBase64String(buffer);
            }
            File.WriteAllText(fileName, content, Encoding.UTF8);
            workspace.PendAdd(new [] { (LocalPath)fileName}, false);
            
            Assert.Equal(1, workspace.PendingChanges.Count);

            var checkinResult = workspace.CheckIn(workspace.PendingChanges, "Test Check In", null);

            Assert.Equal(0, workspace.PendingChanges.Count);

            var item = workspace.GetItem(ItemSpec.FromLocalPath(fileName), ItemType.File, true);

            Assert.Equal(fileName, workspace.Data.GetLocalPathForServerPath(item.ServerPath));
            Assert.Equal(checkinResult.ChangeSet, item.ChangesetId);
            var downloadFile = workspace.DownloadToTemp(item.ArtifactUri);
            var downloadFileContent = File.ReadAllText(downloadFile);

            Assert.Equal(content, downloadFileContent);

            List<Failure> failures;
            workspace.PendDelete(new [] { (LocalPath)fileName }, RecursionType.None, false, out failures);
            Assert.Empty(failures);
            Assert.False(File.Exists(fileName));
            item = workspace.GetItem(ItemSpec.FromLocalPath(fileName), ItemType.File, false);
            Assert.Null(item);
        }

        [Fact]
        public void GetFile()
        {
            var workspace = _fixture.GetWorkspace(_project.Collection);
            var directory = _fixture.GetWorkspaceTopFolder();
            Directory.Delete(directory, true);
            var files = workspace.GetItems(new [] { new ItemSpec(RepositoryPath.RootPath, RecursionType.Full) }, new LatestVersionSpec(), DeletedState.NonDeleted, ItemType.File, false);
            workspace.Get(new GetRequest(RepositoryPath.RootPath, RecursionType.Full, new LatestVersionSpec()), GetOptions.GetAll);
            foreach (var file in files)
            {
                var path = workspace.Data.GetLocalPathForServerPath(file.ServerPath);
                Assert.True(File.Exists(path));
                Assert.True(File.GetAttributes(path).HasFlag(FileAttributes.ReadOnly));
            }
        }


        [Fact]
        public void EditFile()
        {
            var workspace = _fixture.GetWorkspace(_project.Collection);
            var files = workspace.GetItems(new[] { new ItemSpec(RepositoryPath.RootPath, RecursionType.Full) }, new LatestVersionSpec(), DeletedState.NonDeleted, ItemType.File, false);
            var file = files.First();
            
        }


        [Fact]
        public void GetItem()
        {
            var workspace = _fixture.GetWorkspace(_project.Collection);
            var directory = _fixture.GetWorkspaceTopFolder();
            foreach (var file in Directory.EnumerateFiles(directory))
            {
                var item = workspace.GetItem(ItemSpec.FromLocalPath(file), ItemType.File, false);
                Assert.NotNull(item);
            }
        }
    }
}