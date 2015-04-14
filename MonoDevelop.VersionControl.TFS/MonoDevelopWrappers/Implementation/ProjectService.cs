using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.TeamFoundation.VersionControl.Client;
using MonoDevelop.Core;
using MonoDevelop.Core.ProgressMonitoring;
using MonoDevelop.Ide;
using MonoDevelop.Projects;
using MonoDevelop.VersionControl.TFS.VersionControl.Structure;

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
                var fileToAdd = Path.Combine(destination, fileRelativePath);
                if (FileHelper.HasFolder(fileToAdd))
                {
                    fileRelativePath = ((FilePath)fileToAdd).ToRelative(project.BaseDirectory);
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