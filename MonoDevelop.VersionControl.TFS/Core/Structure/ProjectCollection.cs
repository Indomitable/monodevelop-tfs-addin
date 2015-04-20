// ProjectCollection.cs
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using MonoDevelop.VersionControl.TFS.Core.Services;
using MonoDevelop.VersionControl.TFS.Helpers;
using MonoDevelop.VersionControl.TFS.VersionControl.Enums;
using MonoDevelop.VersionControl.TFS.VersionControl.Infrastructure;
using MonoDevelop.VersionControl.TFS.VersionControl.Models;
using MonoDevelop.VersionControl.TFS.VersionControl.Services;
using MonoDevelop.VersionControl.TFS.WorkItemTracking.Services;
using MonoDevelop.VersionControl.TFS.WorkItemTracking.Structure;

namespace MonoDevelop.VersionControl.TFS.Core.Structure
{
    sealed class ProjectCollection : IEquatable<ProjectCollection>, IComparable<ProjectCollection>
    {
        readonly Lazy<RepositoryService> repositoryService;
        readonly Lazy<ClientService> clientService;
        readonly Lazy<CommonStructureService> commonStructureService;

        private readonly List<ProjectInfo> projects = new List<ProjectInfo>();
        private IReadOnlyCollection<WorkspaceData> _localWorkspaces;
        private static readonly object locker = new object();

        private ProjectCollection(TeamFoundationServer server)
        {
            this.Server = server;
            this.ActiveWorkspaceName = string.Empty;
            repositoryService = new Lazy<RepositoryService>(this.GetService<RepositoryService>);
            clientService = new Lazy<ClientService>(this.GetService<ClientService>);
            commonStructureService = new Lazy<CommonStructureService>(this.GetService<CommonStructureService>);
        }

        public Guid Id { get; private set; }

        public string Name { get; private set; }

        public string LocationServicePath { get; private set; }

        public string ActiveWorkspaceName { get; set; }

        private List<ProjectInfo> FetchProjects()
        {
            var projectConfigs = this.commonStructureService.Value.ListAllProjects(this);
            return projectConfigs;
        }

        public void LoadProjects()
        {
            this.Projects.Clear();
            /*s.agostini (2014-01-14) Catch "401 unauthorized" exception, returning an empty list*/
            try
            {
                this.Projects.AddRange(this.FetchProjects());
            }
            catch
            {
                this.Projects.Clear();
            }
            /*s.agostini end*/
        }

        public void LoadProjects(List<string> names)
        {
            this.Projects.Clear();
            this.Projects.AddRange(from pc in this.FetchProjects()
                                   where names.Any(n => string.Equals(pc.Name, n, StringComparison.OrdinalIgnoreCase))
                                   orderby pc.Name
                                   select pc);
        }

        public TeamFoundationServer Server { get; private set; }

        public List<ProjectInfo> Projects { get { return projects; } }

        public TService GetService<TService>()
            where TService : TFSService
        {
            var locationService = new LocationService(this.Server.Uri, this.LocationServicePath) { Server = this.Server };
            return locationService.LoadService<TService>();
        }

        #region Serialization

        public XElement ToConfigXml()
        {
            var element = new XElement("ProjectCollection",
                                new XAttribute("Id", this.Id),
                                new XAttribute("Name", this.Name),
                                new XAttribute("LocationServicePath", this.LocationServicePath),
                                new XAttribute("ActiveWorkspaceName", this.ActiveWorkspaceName));

            element.Add(Projects.Select(p => p.ToConfigXml()));
            return element;
        }

        public static ProjectCollection FromServerXml(XElement element, TeamFoundationServer server)
        {
            var projectCollection = new ProjectCollection(server);
            projectCollection.Id = element.GetGuidAttribute("Identifier");
            projectCollection.Name = element.GetAttributeValue("DisplayName");

            var locationServiceElement = element.GetDescendants("ServiceDefinition")
                                                .Single(el => string.Equals(el.GetAttributeValue("serviceType"), "LocationService"));

            projectCollection.LocationServicePath = locationServiceElement.GetAttributeValue("relativePath");

            return projectCollection;
        }

        public static ProjectCollection FromConfigXml(XElement element, TeamFoundationServer server)
        {
            if (!string.Equals(element.Name.LocalName, "ProjectCollection", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Invalid xml element");

            var projectCollection = new ProjectCollection(server);
            projectCollection.Id = element.GetGuidAttribute("Id");
            projectCollection.Name = element.GetAttributeValue("Name");
            projectCollection.LocationServicePath = element.GetAttributeValue("LocationServicePath");
            projectCollection.ActiveWorkspaceName = element.GetAttributeValue("ActiveWorkspaceName");
            projectCollection.Projects.AddRange(element.Elements("Project").Select(e => ProjectInfo.FromConfigXml(e, projectCollection)));

            return projectCollection;
        }


        public ProjectCollection Copy()
        {
            return new ProjectCollection(this.Server)
            {
                Id = this.Id,
                Name = this.Name,
                LocationServicePath =  this.LocationServicePath,
            };
        }

        #endregion

        #region Workspace Management

        public IReadOnlyCollection<WorkspaceData> GetLocalWorkspaces()
        {
            lock (locker)
            {
                if (_localWorkspaces != null)
                    return _localWorkspaces;
                _localWorkspaces = repositoryService.Value.QueryWorkspaces(Server.UserName, Environment.MachineName);
                return _localWorkspaces;
            }
        }

        public List<WorkspaceData> GetRemoteWorkspaces()
        {
            return repositoryService.Value.QueryWorkspaces(Server.UserName, string.Empty);
        }

        public WorkspaceData GetWorkspace(string workspaceName)
        {
            return repositoryService.Value.QueryWorkspace(Server.UserName, workspaceName);
        }

        public void DeleteWorkspace(string name, string owner)
        {
            repositoryService.Value.DeleteWorkspace(name, owner);
        }

        public void CreateWorkspace(WorkspaceData workspaceData)
        {
            repositoryService.Value.CreateWorkspace(workspaceData);
        }

        public void UpdateWorkspace(string name, string ownerName, WorkspaceData workspaceData)
        {
            repositoryService.Value.UpdateWorkspace(name, ownerName, workspaceData);
        }

        #endregion

        #region Version Control Repository

        public void UploadFile(WorkspaceData workspaceData, CommitItem commitItem)
        {
            this.repositoryService.Value.UploadFile(workspaceData, commitItem);
        }

        public CheckInResult CheckIn(WorkspaceData workspaceData, IEnumerable<RepositoryPath> repositoryPaths, string comment, Dictionary<int, WorkItemCheckinAction> workItems)
        {
            return this.repositoryService.Value.CheckIn(workspaceData, repositoryPaths, comment, workItems);
        }

        public List<PendingChange> QueryPendingChangesForWorkspace(WorkspaceData workspaceData, IEnumerable<ItemSpec> itemSpecs, bool includeDownloadInfo)
        {
            return this.repositoryService.Value.QueryPendingChangesForWorkspace(workspaceData, itemSpecs, includeDownloadInfo);
        }

        public List<GetOperation> PendChanges(WorkspaceData workspaceData, IEnumerable<ChangeRequest> changes, out ICollection<Failure> failures)
        {
            return this.repositoryService.Value.PendChanges(workspaceData, changes, out failures);
        }

        public List<GetOperation> UndoPendChanges(WorkspaceData workspaceData, IEnumerable<ItemSpec> itemSpecs)
        {
            return this.repositoryService.Value.UndoPendChanges(workspaceData, itemSpecs);
        }

        public List<GetOperation> Get(WorkspaceData workspaceData, IEnumerable<GetRequest> requests, bool force, bool noGet)
        {
            return this.repositoryService.Value.Get(workspaceData, requests, force, noGet);
        }

        public List<PendingSet> QueryPendingSets(string localWorkspaceName, string localWorkspaceOwner,
                                                 string queryWorkspaceName, string ownerName,
                                                 ItemSpec[] itemSpecs, bool generateDownloadUrls)
        {
            return this.repositoryService.Value.QueryPendingSets(localWorkspaceName, localWorkspaceOwner, queryWorkspaceName, ownerName,
                itemSpecs, generateDownloadUrls);
        }

        public List<Item> QueryItems(WorkspaceData workspaceData, IEnumerable<ItemSpec> itemSpecs, VersionSpec versionSpec, DeletedState deletedState, ItemType itemType, bool includeDownloadUrl)
        {
            return this.repositoryService.Value.QueryItems(workspaceData, itemSpecs, versionSpec, deletedState, itemType, includeDownloadUrl);
        }

        public List<ExtendedItem> QueryItemsExtended(WorkspaceData workspaceData, IEnumerable<ItemSpec> itemSpecs, DeletedState deletedState, ItemType itemType)
        {
            return this.repositoryService.Value.QueryItemsExtended(workspaceData, itemSpecs, deletedState, itemType);
        }


        public List<Changeset> QueryHistory(ItemSpec item, VersionSpec versionItem, VersionSpec versionFrom, VersionSpec versionTo, short maxCount)
        {
            return this.repositoryService.Value.QueryHistory(item, versionItem, versionFrom, versionTo, maxCount);
        }

        public string Download(string path, string downloadUrl)
        {
            return this.repositoryService.Value.DownloadService.Download(path, downloadUrl);
        }

        public LocalPath DownloadToTemp(string downloadUrl)
        {
            return this.repositoryService.Value.DownloadService.DownloadToTemp(downloadUrl);
        }

        public LocalPath DownloadToTempWithName(string downloadUrl, string fileName)
        {
            return this.repositoryService.Value.DownloadService.DownloadToTempWithName(downloadUrl, fileName);
        }

        public List<Conflict> QueryConflicts(WorkspaceData workspaceData, IEnumerable<ItemSpec> itemSpecs)
        {
            return this.repositoryService.Value.QueryConflicts(workspaceData, itemSpecs);
        }

        public Changeset QueryChangeset(int changeSetId, bool includeChanges = false, bool includeDownloadUrls = false, bool includeSourceRenames = true)
        {
            return this.repositoryService.Value.QueryChangeset(changeSetId, includeChanges, includeDownloadUrls, includeSourceRenames);
        }

        public ResolveResult Resolve(WorkspaceData workspaceData, Conflict conflict, ResolutionType resolutionType)
        {
            return this.repositoryService.Value.Resolve(workspaceData, conflict, resolutionType);
        }

        public void UpdateLocalVersion(WorkspaceData workspaceData, UpdateLocalVersionQueue updateLocalVersionQueue)
        {
            this.repositoryService.Value.UpdateLocalVersion(workspaceData, updateLocalVersionQueue);
        }

        #endregion

        #region Work Items Tracking

        public List<StoredQuery> GetStoredQueries(WorkItemProject project)
        {
            return this.clientService.Value.GetStoredQueries(project);
        }

        public void AssociateWorkItemWithChangeset(int workItemId, int changeSet, string comment)
        {
            this.clientService.Value.Associate(workItemId, changeSet, comment);
        }

        public void ResolveWorkItemWithChangeset(int workItemId, int changeSet, string comment)
        {
            this.clientService.Value.Resolve(workItemId, changeSet, comment);
        }

        public List<int> GetWorkItemIds(StoredQuery query, FieldList fields)
        {
            return this.clientService.Value.GetWorkItemIds(query, fields);
        }

        public WorkItem GetWorkItem(int id)
        {
            return this.clientService.Value.GetWorkItem(id);
        }

        public List<WorkItem> PageWorkitemsByIds(StoredQuery query, List<int> idList)
        {
            return this.clientService.Value.PageWorkitemsByIds(query, idList);
        }

        #region Metadata

        public List<Hierarchy> GetHierarchy()
        {
            return this.clientService.Value.GetHierarchy();
        }

        public List<Field> GetFields()
        {
            return this.clientService.Value.GetFields();
        }

        public List<Constant> GetConstants()
        {
            return this.clientService.Value.GetConstants();
        }

        public List<WorkItemType> GetWorkItemTypes()
        {
            return this.clientService.Value.GetWorkItemTypes();
        }

        public List<WorkItemTracking.Structure.Action> GetActions()
        {
            return this.clientService.Value.GetActions();
        }

        #endregion

        #endregion

        #region Equal

        #region IComparable<ProjectCollection> Members

        public int CompareTo(ProjectCollection other)
        {
            return Id.CompareTo(other.Id);
        }

        #endregion

        #region IEquatable<ProjectCollection> Members

        public bool Equals(ProjectCollection other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return other.Id == Id;
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            ProjectCollection cast = obj as ProjectCollection;
            if (cast == null)
                return false;
            return Equals(cast);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(ProjectCollection left, ProjectCollection right)
        {
            return ReferenceEquals(null, left) ? ReferenceEquals(null, right) : left.Equals(right);
        }

        public static bool operator !=(ProjectCollection left, ProjectCollection right)
        {
            return !(left == right);
        }

        #endregion Equal

    }
}
