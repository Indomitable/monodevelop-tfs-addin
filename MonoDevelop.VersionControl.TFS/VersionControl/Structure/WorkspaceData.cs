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

        public bool IsLocalPathMapped(string localPath)
        {
            if (localPath == null)
                throw new ArgumentNullException("localPath");
            return WorkingFolders.Any(f => localPath.StartsWith(f.LocalItem, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsServerPathMapped(RepositoryFilePath serverPath)
        {
            return WorkingFolders.Any(f => serverPath.IsChildOrEqualTo(f.ServerItem));
        }

        public RepositoryFilePath GetServerPathForLocalPath(string localItem)
        {
            var mappedFolder = WorkingFolders.FirstOrDefault(f => localItem.StartsWith(f.LocalItem, StringComparison.OrdinalIgnoreCase));
            if (mappedFolder == null)
                return null;
            if (string.Equals(mappedFolder.LocalItem, localItem, StringComparison.OrdinalIgnoreCase))
                return mappedFolder.ServerItem;
            else
            {
                string rest = TfsPathHelper.LocalToServerPath(localItem.Substring(mappedFolder.LocalItem.Length));
                if (mappedFolder.ServerItem == RepositoryFilePath.RootFolder)
                    return "$" + rest;
                else
                    return mappedFolder.ServerItem + rest;
            }
        }

        public string GetLocalPathForServerPath(RepositoryFilePath serverItem)
        {
            var mappedFolder = WorkingFolders.FirstOrDefault(f => serverItem.IsChildOrEqualTo(f.ServerItem));
            if (mappedFolder == null)
                return null;
            if (serverItem == mappedFolder.ServerItem)
                return mappedFolder.LocalItem;
            else
            {
                string rest = TfsPathHelper.ServerToLocalPath(serverItem.ChildPart(mappedFolder.ServerItem));
                return Path.Combine(mappedFolder.LocalItem, rest);
            }
        }

        public WorkingFolder GetWorkingFolderForServerItem(string serverItem)
        {
            int maxPath = 0;
            WorkingFolder workingFolder = null;

            foreach (WorkingFolder folder in WorkingFolders)
            {
                if (!serverItem.StartsWith(folder.ServerItem, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                if (folder.LocalItem.Length > maxPath)
                {
                    workingFolder = folder;
                    maxPath = folder.LocalItem.Length;
                }
            }

            return workingFolder;
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

