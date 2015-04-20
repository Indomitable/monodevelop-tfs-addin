// NotificationService.cs
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

using System.Collections.Generic;
using System.Linq;
using MonoDevelop.Core;
using MonoDevelop.VersionControl.TFS.VersionControl.Infrastructure;

namespace MonoDevelop.VersionControl.TFS.MonoDevelopWrappers.Implementation
{
    internal sealed class NotificationService : INotificationService
    {
        private readonly TFSRepository _repository;

        public NotificationService(TFSRepository repository)
        {
            _repository = repository;
        }

        public void NotifyFileChanged(string path)
        {
            var fp = new FilePath(path);
            VersionControlService.NotifyFileStatusChanged(new FileUpdateEventArgs(_repository, fp, fp.IsDirectory));
            FileService.NotifyFileChanged(fp);
        }

        public void NotifyFilesChanged(IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                NotifyFileChanged(path);
            }
        }

        public void NotifyFilesChanged(IEnumerable<LocalPath> paths)
        {
            NotifyFilesChanged(paths.Select(x => (string)x));
        }

        public void NotifyFileRemoved(string path)
        {
            var fp = new FilePath(path);
            VersionControlService.NotifyFileStatusChanged(new FileUpdateEventArgs(_repository, fp, fp.IsDirectory));
            FileService.NotifyFileRemoved(fp);
        }

        public void NotifyFilesRemoved(IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                NotifyFileRemoved(path);
            }
        }

        public void NotifyFilesRemoved(IEnumerable<LocalPath> paths)
        {
            NotifyFilesRemoved(paths.Select(x => (string)x));
        }
    }
}