// RepositoryTests.cs
// 
// Author:
//       Ventsislav Mladenov
// 
// The MIT License (MIT)
// 
// Copyright (c) 2013-2015 Ventsislav Mladenov
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using MonoDevelop.VersionControl.TFS.Core.Structure;
using MonoDevelop.VersionControl.TFS.Tests.Core;
using MonoDevelop.VersionControl.TFS.VersionControl.Enums;
using MonoDevelop.VersionControl.TFS.VersionControl.Infrastructure;
using MonoDevelop.VersionControl.TFS.VersionControl.Models;
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
            ICollection<Failure> failures;
            workspace.PendAdd(new [] { (LocalPath)fileName}, false, out failures);
            
            Assert.Equal(1, workspace.PendingChanges.Count);

            var checkinResult = workspace.CheckIn(workspace.PendingChanges, "Test Check In", null);

            Assert.Equal(0, workspace.PendingChanges.Count);

            var item = workspace.GetItem(ItemSpec.FromLocalPath(fileName), ItemType.File, true);

            Assert.Equal(fileName, workspace.Data.GetLocalPathForServerPath(item.ServerPath));
            Assert.Equal(checkinResult.ChangeSet, item.ChangesetId);
            var downloadFile = workspace.DownloadToTemp(item.ArtifactUri);
            var downloadFileContent = File.ReadAllText(downloadFile);

            Assert.Equal(content, downloadFileContent);

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