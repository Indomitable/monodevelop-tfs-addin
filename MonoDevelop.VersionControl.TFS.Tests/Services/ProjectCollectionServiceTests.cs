using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
