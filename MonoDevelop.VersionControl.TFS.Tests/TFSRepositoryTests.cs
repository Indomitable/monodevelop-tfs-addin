using NUnit.Framework;
using System;
using MonoDevelop.VersionControl.TFS;

namespace MonoDevelop.VersionControl.TFS.Tests
{
    [TestFixture]
    public class TFSRepositoryTests
    {
        private TFSRepository _repository;

        [SetUp]
        public void CreateRepository()
        {
            _repository = new TFSRepository();
        }

        [Test]
        public void SupportedProtocols()
        {
//            CollectionAssert.Contains(_repository.SupportedProtocols, "http");
//            CollectionAssert.Contains(_repository.SupportedProtocols, "https");
        }
    }
}

