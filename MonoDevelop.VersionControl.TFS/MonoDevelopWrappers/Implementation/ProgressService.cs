namespace MonoDevelop.VersionControl.TFS.MonoDevelopWrappers.Implementation
{
    internal sealed class ProgressService : IProgressService
    {
        public IProgressDisplay CreateProgress()
        {
            return new ProgressDisplay();
        }
    }
}