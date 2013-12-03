//
// ResolveConflictsHandler.cs
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
using MonoDevelop.Ide;
using System.Collections.Generic;
using MonoDevelop.Core;
using MonoDevelop.VersionControl.TFS.GUI.VersionControl;

namespace MonoDevelop.VersionControl.TFS.Commands
{
    public class ResolveConflictsHandler : CommandHandler
    {
        protected override void Update(CommandInfo info)
        {
            if (VersionControlService.IsGloballyDisabled)
            {
                info.Visible = false;
                return;
            }

            var solution = IdeApp.ProjectOperations.CurrentSelectedSolution;
            if (solution == null)
            {
                info.Visible = false;
                return;
            }

            var repo = VersionControlService.GetRepository(solution) as TFSRepository;
            if (repo == null)
            {
                info.Visible = false;
                return;
            }
        }

        protected override void Run()
        {
            var solution = IdeApp.ProjectOperations.CurrentSelectedSolution;
            List<FilePath> paths = new List<FilePath>();
            //Add Solution
            paths.Add(solution.BaseDirectory);
            //Add linked files.
            foreach (var path in solution.GetItemFiles(true))
            {
                if (!path.IsChildPathOf(solution.BaseDirectory))
                {
                    paths.Add(path);
                }
            }
            var repo = (TFSRepository)VersionControlService.GetRepository(solution);
            ResolveConflictsView.Open(repo, paths);
        }
    }
}

