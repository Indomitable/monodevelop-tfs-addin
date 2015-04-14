using MonoDevelop.VersionControl.TFS.MonoDevelopWrappers;

namespace MonoDevelop.VersionControl.TFS.Tests.Core.MonoDevelopServices
{
    internal sealed class TestProgressService : IProgressService
    {
        public IProgressDisplay CreateProgress()
        {
            return new TestProgressDisplay();
        }
    }
}