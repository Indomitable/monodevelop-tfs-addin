using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MonoDevelop.VersionControl.TFS.Tests.Services
{
    public class ProjectCollectionServiceTests : ServerFixture
    {
        [Fact]
        public void StructureTest()
        {
            this.Server.LoadStructure();
            Assert.Equal(2, this.Server.ProjectCollections.Count);
        }
    }
}
