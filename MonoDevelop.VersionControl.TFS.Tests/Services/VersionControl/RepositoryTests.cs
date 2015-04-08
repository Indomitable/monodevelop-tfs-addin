using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Client.Enums;
using Microsoft.TeamFoundation.VersionControl.Client.Objects;
using MonoDevelop.Core;
using MonoDevelop.Core.ProgressMonitoring;
using MonoDevelop.VersionControl.TFS.Core.Structure;
using MonoDevelop.VersionControl.TFS.Tests.Core;
using MonoDevelop.VersionControl.TFS.VersionControl;
using MonoDevelop.VersionControl.TFS.VersionControl.Structure;
using MonoDevelop.VersionControl.TFS.WorkItemTracking.Structure;
using Xunit;
using Xunit.Abstractions;

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
            var item = workspace.GetExtendedItem(RepositoryPath.RootPath, ItemType.Folder);
            Assert.NotNull(item);
            Assert.Equal(item.ServerPath, RepositoryPath.RootPath);
        }

        [Fact]
        public void AddFile()
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
            workspace.PendAdd(new [] { (LocalPath)fileName}, false, new NullProgressMonitor());
            
            Assert.Equal(1, workspace.PendingChanges.Count);

            var checkinResult = workspace.CheckIn(workspace.PendingChanges, "Test Check In", null, new NullProgressMonitor());

            Assert.Equal(0, workspace.PendingChanges.Count);

            var item = workspace.GetItem(fileName, ItemType.File, true);

            Assert.Equal(fileName, workspace.Data.GetLocalPathForServerPath(item.ServerPath));
            Assert.Equal(checkinResult.ChangeSet, item.ChangesetId);
            var downloadFile = workspace.DownloadToTemp(item.ArtifactUri);
            var downloadFileContent = File.ReadAllText(downloadFile);

            Assert.Equal(content, downloadFileContent);
        }

        [Fact]
        public void GetFile()
        {
            var workspace = _fixture.GetWorkspace(_project.Collection);
            var directory = _fixture.GetWorkspaceTopFolder();
            Directory.Delete(directory, true);
            Directory.CreateDirectory(directory);
            workspace.Get(new GetRequest(RepositoryPath.RootPath, RecursionType.Full, new LatestVersionSpec()), GetOptions.GetAll, new NullProgressMonitor());
        }
    }
}