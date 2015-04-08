using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using MonoDevelop.VersionControl.TFS.Helpers;
using MonoDevelop.VersionControl.TFS.VersionControl.Helpers;
using System.IO;

namespace MonoDevelop.VersionControl.TFS.VersionControl.Structure
{
    sealed class WorkspaceData
    {
        public WorkspaceData()
        {
            WorkingFolders = new List<WorkingFolder>();
        }

        public string Name { get; set; }

        public string Owner { get; set; }

        public string Computer { get; set; }

        public string Comment { get; set; }

        public bool IsLocal { get; set; }

        public List<WorkingFolder> WorkingFolders { get; set; }

        #region Working Folders

        public bool IsLocalPathMapped(LocalPath localPath)
        {
            if (localPath == null)
                throw new ArgumentNullException("localPath");
            return WorkingFolders.Any(f => localPath.IsChildOrEqualOf(f.LocalItem));
        }

        public bool IsServerPathMapped(RepositoryPath serverPath)
        {
            return WorkingFolders.Any(f => serverPath.IsChildOrEqualOf(f.ServerItem));
        }

        public RepositoryPath GetServerPathForLocalPath(LocalPath localPath)
        {
            var mappedFolder = WorkingFolders.FirstOrDefault(f => localPath.IsChildOrEqualOf(f.LocalItem));
            if (mappedFolder == null)
                return null;
            if (localPath == mappedFolder.LocalItem)
                return mappedFolder.ServerItem;
            var relativePath = localPath.ToRelativeOf(mappedFolder.LocalItem);
            if (!EnvironmentHelper.IsRunningOnUnix)
                relativePath = relativePath.Replace(Path.DirectorySeparatorChar, RepositoryPath.Separator);
            return new RepositoryPath(mappedFolder.ServerItem + relativePath, localPath.IsDirectory);
        }

        public LocalPath GetLocalPathForServerPath(RepositoryPath serverPath)
        {
            var mappedFolder = WorkingFolders.FirstOrDefault(f => serverPath.IsChildOrEqualOf(f.ServerItem));
            if (mappedFolder == null)
                return null;
            if (serverPath == mappedFolder.ServerItem)
                return mappedFolder.LocalItem;
            else
            {
                string relativePath = serverPath.ToRelativeOf(mappedFolder.ServerItem);
                if (!EnvironmentHelper.IsRunningOnUnix)
                {
                    relativePath = relativePath.Replace(RepositoryPath.Separator, Path.DirectorySeparatorChar);
                }
                return Path.Combine(mappedFolder.LocalItem, relativePath);
            }
        }

        public WorkingFolder GetWorkingFolderForServerItem(RepositoryPath serverPath)
        {
            return this.WorkingFolders.SingleOrDefault(wf => serverPath.IsChildOrEqualOf(wf.ServerItem));
        }

        #endregion

        #region Serialization

        public static WorkspaceData FromServerXml(XElement element)
        {
            var data = new WorkspaceData();
            data.Name = element.GetAttributeValue("name");
            data.Owner = element.GetAttributeValue("owner");
            data.Computer = element.GetAttributeValue("computer");
            data.IsLocal = element.GetBooleanAttribute("islocal");
            data.Comment = element.GetElement("Comment").Value;
            //DateTime lastAccessDate = DateTime.Parse(element.Element(XmlNamespaces.GetMessageElementName("LastAccessDate")).Value);
            data.WorkingFolders.AddRange(element.GetDescendants("WorkingFolder").Select(WorkingFolder.FromXml));
            return data;
        }

        internal XElement ToXml(string elementName)
        {
            XElement element = new XElement(elementName, 
                new XAttribute("computer", Computer), 
                new XAttribute("name", Name),
                new XAttribute("owner", Owner), 
                new XElement("Comment", Comment)
            );

            element.Add(new XElement("Folders", WorkingFolders.Select(f => f.ToXml())));
            return element;
        }

        #endregion
    }
}

