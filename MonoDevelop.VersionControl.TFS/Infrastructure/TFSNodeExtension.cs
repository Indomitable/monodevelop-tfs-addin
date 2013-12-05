//
// TFSNodeExtension.cs
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
using MonoDevelop.Ide.Gui.Components;
using MonoDevelop.Projects;
using MonoDevelop.Components.Commands;
using MonoDevelop.VersionControl.TFS.Commands;
using MonoDevelop.Ide.Gui.Pads.ProjectPad;
using MonoDevelop.Core;
using System.Collections.Generic;
using MonoDevelop.VersionControl.TFS.GUI.VersionControl;
using MonoDevelop.Ide.CodeTemplates;

namespace MonoDevelop.VersionControl.TFS.Infrastructure
{
    public class TFSNodeExtension: NodeBuilderExtension
    {
        public override bool CanBuildNode(Type dataType)
        {
            return typeof(ProjectFile).IsAssignableFrom(dataType)
            || typeof(SystemFile).IsAssignableFrom(dataType)
            || typeof(IWorkspaceFileObject).IsAssignableFrom(dataType);
        }

        public override Type CommandHandlerType
        {
            get { return typeof(TFSCommandHandler); }
        }
    }

    class TFSCommandHandler : VersionControlCommandHandler
    {
        [CommandHandler(TFSCommands.Checkout)]
        protected void OnCheckoutFile()
        {
            foreach (var item in base.GetItems(false))
            {
                var repo = (TFSRepository)item.Repository;
                repo.CheckoutFile(item.Path);
            }
        }

        [CommandUpdateHandler(TFSCommands.Checkout)]
        protected void UpdateCheckoutFile(CommandInfo commandInfo)
        {
            if (VersionControlService.IsGloballyDisabled)
            {
                commandInfo.Visible = false;
                return;
            }
            foreach (var item in GetItems(false))
            {
                if (item.IsDirectory)
                {
                    commandInfo.Visible = false;
                    return;
                }

                var repo = item.Repository as TFSRepository;
                if (repo == null)
                {
                    commandInfo.Visible = false;
                    return;
                }
                if (!item.VersionInfo.IsVersioned || item.VersionInfo.HasLocalChanges || item.VersionInfo.Status.HasFlag(VersionStatus.Locked))
                {
                    commandInfo.Visible = false;
                    return;
                }
            }
        }

        [CommandHandler(TFSCommands.ResolveConflicts)]
        protected void OnResolveConflicts()
        {
            var items = base.GetItems(false);
            if (items.Count == 0)
                return;
            var item = items[0];
            var repo = (TFSRepository)item.Repository;
            ResolveConflictsView.Open(repo, GetWorkingPaths(item));
        }

        private List<FilePath> GetWorkingPaths(VersionControlItem item)
        {
            List<FilePath> paths = new List<FilePath>();
            var solution = item.WorkspaceObject as Solution;
            if (solution != null)
            {
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
            }
            else
            {
                var project = (Project)item.WorkspaceObject;
                paths.Add(project.BaseDirectory);
            }
            return paths;
        }

        [CommandUpdateHandler(TFSCommands.ResolveConflicts)]
        protected void UpdateResolveConflicts(CommandInfo commandInfo)
        {
            if (VersionControlService.IsGloballyDisabled)
            {
                commandInfo.Visible = false;
                return;
            }
            var items = base.GetItems(false);
            if (items.Count == 0)
            {
                commandInfo.Visible = false;
                return;
            }
            var item = items[0];
            var repo = item.Repository as TFSRepository;
            if (repo == null)
            {
                commandInfo.Visible = false;
                return;
            }
            commandInfo.Visible = true;
        }

        [CommandHandler(TFSCommands.LocateInSourceExplorer)]
        protected void OnLocateInSourceExplorer()
        {
            var item = base.GetItems(false)[0];
            var repo = (TFSRepository)item.Repository;
            var path = item.Path;
            string fileName = null;
            if (!item.IsDirectory)
            {
                fileName = path.FileName;
                path = path.ParentDirectory;
            }
            var workspace = repo.GetWorkspaceByLocalPath(path);
            if (workspace == null)
                return;
            var serverPath = workspace.GetServerItemForLocalItem(path);
            SourceControlExplorerView.Open(workspace.ProjectCollection, serverPath, fileName);
        }

        [CommandUpdateHandler(TFSCommands.LocateInSourceExplorer)]
        protected void UpdateLocateInSourceExplorer(CommandInfo commandInfo)
        {
            if (VersionControlService.IsGloballyDisabled)
            {
                commandInfo.Visible = false;
                return;
            }
            var items = base.GetItems(false);
            if (items.Count != 1)
            {
                commandInfo.Visible = false;
                return;
            }
            foreach (var item in items)
            {
                var repo = item.Repository as TFSRepository;
                if (repo == null)
                {
                    commandInfo.Visible = false;
                    return;
                }
                if (!item.VersionInfo.IsVersioned)
                {
                    commandInfo.Visible = false;
                    return;
                }
            }
        }
    }
}

