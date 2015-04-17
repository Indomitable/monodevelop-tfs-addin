using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MonoDevelop.VersionControl.TFS.MonoDevelopWrappers;
using MonoDevelop.VersionControl.TFS.VersionControl.Infrastructure;

namespace MonoDevelop.VersionControl.TFS.Infrastructure.Services.Implementation
{
    sealed class FileKeepSession : IFileKeepSession
    {
        private readonly ILoggingService _loggingService;
        private readonly Dictionary<LocalPath, byte[]> _store = new Dictionary<LocalPath, byte[]>();

        public FileKeepSession(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        public void Save(LocalPath[] localPaths, bool recursive)
        {
            var paths = recursive ? localPaths.SelectMany(p => p.CollectSubPathsAndSelf()) : localPaths;
            foreach (var path in paths)
            {
                _store.Add(path, path.IsDirectory ? null : File.ReadAllBytes(path));
            }
        }

        private void Restore()
        {
            foreach (var map in _store)
            {
                try
                {
                    if (map.Value == null) //Directory
                    {
                        Directory.CreateDirectory(map.Key);
                    }
                    else
                    {
                        File.WriteAllBytes(map.Key, map.Value);
                    }
                }
                catch (Exception ex)
                {
                    _loggingService.LogToError(ex.Message);
                }
            }
            _store.Clear();
        }

        private bool isDisposed;


        public void Dispose()
        {
            if (isDisposed)
                return;

            Restore();

            isDisposed = true;
        }
    }
}