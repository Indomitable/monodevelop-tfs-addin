using System;
using MonoDevelop.VersionControl.TFS.VersionControl.Infrastructure;

namespace MonoDevelop.VersionControl.TFS.Infrastructure.Services
{
    internal interface IFileKeepSession : IDisposable
    {
        void Save(LocalPath[] localPaths, bool recursive);
    }
}