// ProjectService.cs
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
using System.IO;
using System.Linq;
using MonoDevelop.Core;
using MonoDevelop.Core.ProgressMonitoring;
using MonoDevelop.Ide;
using MonoDevelop.Projects;
using MonoDevelop.VersionControl.TFS.VersionControl.Helpers;
using MonoDevelop.VersionControl.TFS.VersionControl.Infrastructure;

namespace MonoDevelop.VersionControl.TFS.MonoDevelopWrappers.Implementation
{
    internal sealed class ProjectService : IProjectService
    {
        private void ProjectMoveFile(Project project, LocalPath source, string destination)
        {
            var file = project.Files.FirstOrDefault(f => f.FilePath == source);
            if (file != null)
                project.Files.Remove(file);
            project.AddFile(destination);
        }

        private Project FindProjectContainingFolder(LocalPath folder)
        {
            Project project = null;
            foreach (var prj in IdeApp.Workspace.GetAllProjects())
            {
                foreach (var file in prj.Files)
                {
                    if (file.FilePath.IsDirectory && file.FilePath == folder)
                    {
                        project = prj;
                        break;
                    }
                    if (!file.FilePath.IsDirectory && file.FilePath.IsChildPathOf(new FilePath(folder)))
                    {
                        project = prj;
                        break;
                    }
                }
            }
            return project;
        }

        private void ProjectMoveFolder(Project project, LocalPath source, LocalPath destination)
        {
            var filesToMove = new List<ProjectFile>();
            ProjectFile folderFile = null;
            foreach (var file in project.Files)
            {
                if (file.FilePath == source)
                {
                    folderFile = file;
                }
                if (file.FilePath.IsChildPathOf(new FilePath(source)))
                {
                    filesToMove.Add(file);
                }
            }
            if (folderFile != null)
                project.Files.Remove(folderFile);
        
            var relativePath = destination.ToRelativeOf(new LocalPath(project.BaseDirectory));
            project.AddDirectory(relativePath);
            foreach (var file in filesToMove)
            {
                project.Files.Remove(file);
                var fileRelativePath = file.FilePath.ToRelative(new FilePath(source));
                var fileToAdd = new LocalPath(Path.Combine(destination, fileRelativePath));
                if (fileToAdd.IsDirectory)
                {
                    fileRelativePath = fileToAdd.ToRelativeOf((string)project.BaseDirectory);
                    project.AddDirectory(fileRelativePath);
                }
                else
                    project.AddFile(fileToAdd);
            }
        }

        public void MoveFile(LocalPath fromPath, LocalPath toPath)
        {
            var projects = IdeApp.Workspace.GetProjectsContainingFile(fromPath);
            foreach (var project in projects)
            {
                ProjectMoveFile(project, fromPath, toPath);
                project.Save(new NullProgressMonitor());
            }
        }

        public void MoveFolder(LocalPath fromPath, LocalPath toPath)
        {
            var project = FindProjectContainingFolder(fromPath);
            if (project != null)
            {
                ProjectMoveFolder(project, fromPath, toPath);
                project.Save(new NullProgressMonitor());
            }
        }

        public void AddFile(LocalPath path)
        {
            var projects = IdeApp.Workspace.GetAllProjects();
            foreach (var project in projects)
            {
                if (path.IsChildOrEqualOf(new LocalPath(project.BaseDirectory)))
                {
                    if (path.IsDirectory)
                        project.AddDirectory(path.ToRelativeOf(new LocalPath(project.BaseDirectory)));
                    else
                        project.AddFile(path);
                }
            }
        }
    }
}