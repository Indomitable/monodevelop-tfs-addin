//
// SourceControlExplorerMenuHandler.cs
//
// Author:
//       Ventsislav Mladenov <vmladenov.mladenov@gmail.com>
//
// Copyright (c) 2013 Ventsislav Mladenov
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
using System;
using MonoDevelop.Components.Commands;
using MonoDevelop.VersionControl.TFS.GUI.VersionControl;
using System.Linq;

namespace MonoDevelop.VersionControl.TFS.Commands
{
    public class SourceControlExplorerMenuHandler : CommandHandler
    {
        protected override void Update(CommandInfo info)
        {
            var collectionsCount = TFSVersionControlService.Instance.Servers.SelectMany(x => x.ProjectCollections).Count();
            if (collectionsCount != 1)
            {
                info.Visible = false;
                return;
            }
        }

        protected override void Run()
        {
            var collection = TFSVersionControlService.Instance.Servers.SelectMany(x => x.ProjectCollections).First();
            var project = collection.Projects.FirstOrDefault();
            if (project != null)
            {
                SourceControlExplorerView.Open(project);
            }
        }
    }
}

