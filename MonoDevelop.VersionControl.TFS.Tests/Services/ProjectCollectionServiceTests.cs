// ProjectCollectionServiceTests.cs
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

using System.Linq;
using MonoDevelop.VersionControl.TFS.Tests.Core;
using Xunit;

namespace MonoDevelop.VersionControl.TFS.Tests.Services
{
    [Collection("Server")]
    public class ProjectCollectionServiceTests
    {
        private readonly ServerFixture _fixture;

        public ProjectCollectionServiceTests(ServerFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void StructureTest()
        {
            Assert.Equal(3, _fixture.Server.ProjectCollections.Count);
        }

        [Fact]
        public void ProjectIntegrityTest()
        {
            var project = _fixture.Server.ProjectCollections.SelectMany(pc => pc.Projects).SingleOrDefault();
            Assert.NotNull(project);
            Assert.NotNull(project.Collection);
            Assert.NotNull(project.Collection.Server);
        }
    }
}
