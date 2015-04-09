using MonoDevelop.VersionControl.TFS.VersionControl.Structure;

namespace MonoDevelop.VersionControl.TFS.MonoDevelopWrappers
{
    interface IProjectService
    {
        void MoveFile(LocalPath fromPath, LocalPath toPath);
        void MoveFolder(LocalPath fromPath, LocalPath toPath);
        void AddFile(LocalPath path);
    }
}