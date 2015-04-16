// RepositoryService.cs
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
using System.Xml.XPath;
using MonoDevelop.VersionControl.TFS.Core.Services;
using MonoDevelop.VersionControl.TFS.Core.Services.Resolvers;
using MonoDevelop.VersionControl.TFS.Helpers;
using MonoDevelop.VersionControl.TFS.Infrastructure;
using MonoDevelop.VersionControl.TFS.VersionControl.Enums;
using MonoDevelop.VersionControl.TFS.VersionControl.Infrastructure;
using MonoDevelop.VersionControl.TFS.VersionControl.Models;
using MonoDevelop.VersionControl.TFS.VersionControl.Services.Resolvers;
using MonoDevelop.VersionControl.TFS.WorkItemTracking.Structure;

namespace MonoDevelop.VersionControl.TFS.VersionControl.Services
{
    [ServiceResolver(typeof(RepositoryServiceResolver))]
    sealed class RepositoryService : TFSService
    {
        private RepositoryService(Uri baseUri, string servicePath)
            : base(baseUri, servicePath)
        {
            
        }

        [RequireService]
        public UploadService UploadService { get; set; }

        [RequireService]
        public DownloadService DownloadService { get; set; }

        #region TfsService

        public override XNamespace MessageNs
        {
            get
            {
                return "http://schemas.microsoft.com/TeamFoundation/2005/06/VersionControl/ClientServices/03";
            }
        }

        #endregion

        #region Workspaces

        public WorkspaceData QueryWorkspace(string ownerName, string workspaceName)
        {
            var invoker = GetSoapInvoker();
            XElement msg = invoker.CreateEnvelope("QueryWorkspace");
            msg.AddElement("workspaceName", workspaceName);
            msg.AddElement("ownerName", ownerName);

            XElement result = invoker.InvokeResult();
            return WorkspaceData.FromServerXml(result);
        }

        public List<WorkspaceData> QueryWorkspaces(string ownerName, string computer)
        {
            var invoker = GetSoapInvoker();
            XElement msg = invoker.CreateEnvelope("QueryWorkspaces");
            if (!string.IsNullOrEmpty(ownerName))
                msg.AddElement("ownerName", ownerName);
            if (!string.IsNullOrEmpty(computer))
                msg.AddElement("computer", computer);

            XElement result = invoker.InvokeResult();
            return result.GetElements("Workspace").Select(WorkspaceData.FromServerXml).OrderBy(x => x.Name).ToList();
        }

        public WorkspaceData UpdateWorkspace(string oldWorkspaceName, string ownerName,
                                         WorkspaceData newWorkspace)
        {
            var invoker = GetSoapInvoker();
            XElement msg = invoker.CreateEnvelope("UpdateWorkspace");

            msg.AddElement("oldWorkspaceName", oldWorkspaceName);
            msg.AddElement("ownerName", ownerName);
            msg.AddElement(newWorkspace.ToXml("newWorkspace"));

            XElement result = invoker.InvokeResult();
            return WorkspaceData.FromServerXml(result);
        }

        public WorkspaceData CreateWorkspace(WorkspaceData workspace)
        {
            var invoker = GetSoapInvoker();
            XElement msg = invoker.CreateEnvelope("CreateWorkspace");
            msg.AddElement(workspace.ToXml("workspace"));
            XElement result = invoker.InvokeResult();
            return WorkspaceData.FromServerXml(result);
        }

        public void DeleteWorkspace(string workspaceName, string ownerName)
        {
            var invoker = GetSoapInvoker();
            var msg = invoker.CreateEnvelope("DeleteWorkspace");
            msg.AddElement("workspaceName", workspaceName);
            msg.AddElement("ownerName", ownerName);
            invoker.InvokeResult();
        }

        #endregion

        internal void UpdateLocalVersion(WorkspaceData workspaceData, UpdateLocalVersionQueue updateLocalVersionQueue)
        {
            var invoker = GetSoapInvoker();
            var msg = invoker.CreateEnvelope("UpdateLocalVersion");
            msg.AddElement("workspaceName", workspaceData.Name);
            msg.AddElement("ownerName", workspaceData.Owner);
            msg.AddElement("updates", updateLocalVersionQueue.Select(x => x.ToXml()));
            invoker.InvokeResult();
        }

        #region Query Items

        public List<Item> QueryItems(WorkspaceData workspaceData, IEnumerable<ItemSpec> itemSpecs, VersionSpec versionSpec,
                                     DeletedState deletedState, ItemType itemType, 
                                     bool includeDownloadInfo)
        {
            var invoker = GetSoapInvoker();
            var msg = invoker.CreateEnvelope("QueryItems");
            if (workspaceData != null)
            {
                msg.AddElement("workspaceName", workspaceData.Name);
                msg.AddElement("workspaceOwner", workspaceData.Owner);
            }
            msg.AddElement("items", itemSpecs.Select(itemSpec => itemSpec.ToXml("ItemSpec")));
            msg.AddElement(versionSpec.ToXml("version"));
            msg.AddElement("deletedState", deletedState);
            msg.AddElement("itemType", itemType);
            msg.AddElement("generateDownloadUrls", includeDownloadInfo);

            var result = invoker.InvokeResult();
            return result.GetDescendants("Item").Select(Item.FromXml).ToList();
        }

        public List<Item> QueryFolders()
        {
            var itemSpecs = new[] { new ItemSpec(RepositoryPath.RootPath, RecursionType.Full) };
            return QueryItems(null, itemSpecs, VersionSpec.Latest, DeletedState.NonDeleted, ItemType.Folder, false);
        }

        public List<ExtendedItem> QueryItemsExtended(WorkspaceData workspaceData, IEnumerable<ItemSpec>  itemSpecs,
                                                     DeletedState deletedState, ItemType itemType)
        {
            var invoker = GetSoapInvoker();
            var msg = invoker.CreateEnvelope("QueryItemsExtended");
            if (workspaceData != null)
            {
                msg.AddElement("workspaceName", workspaceData.Name);
                msg.AddElement("workspaceOwner", workspaceData.Owner);
            }
            msg.AddElement("items", itemSpecs.Select(itemSpec => itemSpec.ToXml("ItemSpec")));
            msg.AddElement("deletedState", deletedState);
            msg.AddElement("itemType", itemType);

            var result = invoker.InvokeResult();
            return result.GetDescendants("ExtendedItem").Select(ExtendedItem.FromXml).ToList();
        }

        #endregion

        internal List<GetOperation> Get(WorkspaceData workspaceData,
                                        IEnumerable<GetRequest> requests, bool force, bool noGet)
        {
            if (workspaceData == null)
                throw new System.ArgumentNullException("workspaceData");
            var invoker = GetSoapInvoker();
            var msg = invoker.CreateEnvelope("Get");
            msg.AddElement("workspaceName", workspaceData.Name);
            msg.AddElement("ownerName", workspaceData.Owner);
            msg.AddElement("requests", requests.Select(r => r.ToXml()));
            if (force)
                msg.AddElement("force", force);
            if (noGet)
                msg.AddElement("noGet", noGet);

            List<GetOperation> operations = new List<GetOperation>();
            var result = invoker.InvokeResult();

            foreach (var operation in result.XPathSelectElements("msg:ArrayOfGetOperation/msg:GetOperation", NsResolver))
            {
                operations.Add(GetOperation.FromXml(operation));
            }
            return operations;
        }

        internal List<PendingSet> QueryPendingSets(string localWorkspaceName, string localWorkspaceOwner,
                                                 string queryWorkspaceName, string ownerName,
                                                 IEnumerable<ItemSpec> itemSpecs, bool generateDownloadUrls)
        {
            var invoker = GetSoapInvoker();
            var msg = invoker.CreateEnvelope("QueryPendingSets");
            msg.AddElement("localWorkspaceName", localWorkspaceName);
            msg.AddElement("localWorkspaceOwner", localWorkspaceOwner);
            msg.AddElement("queryWorkspaceName", queryWorkspaceName);
            msg.AddElement("ownerName", ownerName);
            msg.AddElement("itemSpecs", itemSpecs.Select(i => i.ToXml("ItemSpec")));
            msg.AddElement("generateDownloadUrls", generateDownloadUrls);

            var result = invoker.InvokeResult();
            return new List<PendingSet>(result.GetElements("PendingSet").Select(PendingSet.FromXml));
//            var pendingChangesElements = result.Descendants(MessageNs + "PendingChange");
//            var failuresElements = result.Descendants(MessageNs + "PendingChange");
//
//            var changes = new List<PendingChange>(pendingChangesElements.Select(el => PendingChange.FromXml(el)));
//            var faillist = new List<Failure>(failuresElements.Select(el => Failure.FromXml(el)));
//            return new Tuple<List<PendingChange>, List<Failure>>(changes, faillist);
        }

        internal List<GetOperation> PendChanges(WorkspaceData workspaceData, IEnumerable<ChangeRequest> changeRequest, out ICollection<Failure> failures)
        {
            var invoker = GetSoapInvoker();
            var msg = invoker.CreateEnvelope("PendChanges");
            msg.AddElement("workspaceName", workspaceData.Name);
            msg.AddElement("ownerName", workspaceData.Owner);
            msg.AddElement("changes", changeRequest.Select(x => x.ToXml()));
            var response = invoker.InvokeResponse();
            failures = FailuresExtractor(response);
            var result = invoker.MethodResultExtractor(response);
            return result.GetElements("GetOperation").Select(GetOperation.FromXml).ToList();
        }

        internal List<GetOperation> UndoPendChanges(WorkspaceData workspaceData, IEnumerable<ItemSpec> itemSpecs)
        {
            var invoker = GetSoapInvoker();
            var msg = invoker.CreateEnvelope("UndoPendingChanges");
            msg.AddElement("workspaceName", workspaceData.Name);
            msg.AddElement("ownerName", workspaceData.Owner);
            msg.AddElement("items", itemSpecs.Select(x => x.ToXml("ItemSpec")));
            var result = invoker.InvokeResult();
            return GetOperationExtractor(result);
        }

        internal List<PendingChange> QueryPendingChangesForWorkspace(WorkspaceData workspaceData, IEnumerable<ItemSpec> itemSpecs, bool includeDownloadInfo)
        {
            var invoker = GetSoapInvoker();
            var msg = invoker.CreateEnvelope("QueryPendingChangesForWorkspace");
            msg.AddElement("workspaceName", workspaceData.Name);
            msg.AddElement("workspaceOwner", workspaceData.Owner);
            msg.AddElement("itemSpecs", itemSpecs.Select(i => i.ToXml("ItemSpec")));
            msg.AddElement("generateDownloadUrls", includeDownloadInfo);
            var result = invoker.InvokeResult();
            return result.GetElements("PendingChange").Select(PendingChange.FromXml).ToList();
        }

        public List<Changeset> QueryHistory(ItemSpec item, VersionSpec versionItem, 
                                            VersionSpec versionFrom, VersionSpec versionTo, short maxCount)
        {
            var invoker = GetSoapInvoker();
            var msg = invoker.CreateEnvelope("QueryHistory");
            msg.AddElement(item.ToXml("itemSpec"));
            msg.AddElement(versionItem.ToXml("versionItem"));
            if (versionFrom != null)
                msg.AddElement(versionFrom.ToXml("versionFrom"));
            if (versionTo != null)
                msg.AddElement(versionTo.ToXml("versionTo"));
            msg.AddElement("maxCount", maxCount);
            msg.AddElement("includeFiles", false);
            msg.AddElement("generateDownloadUrls", false);
            msg.AddElement("slotMode", false);
            msg.AddElement("sortAscending", false);

            var result = invoker.InvokeResult();
            return result.Elements(MessageNs + "Changeset").Select(Changeset.FromXml).ToList();
        }

        public Changeset QueryChangeset(int changeSetId, bool includeChanges, bool includeDownloadUrls, bool includeSourceRenames)
        {
            var invoker = GetSoapInvoker();
            var msg = invoker.CreateEnvelope("QueryChangeset");
            msg.Add(new XElement(MessageNs + "changesetId", changeSetId));
            msg.Add(new XElement(MessageNs + "includeChanges", includeChanges));
            msg.Add(new XElement(MessageNs + "generateDownloadUrls", includeDownloadUrls));
            msg.Add(new XElement(MessageNs + "includeSourceRenames", includeSourceRenames));

            var result = invoker.InvokeResult();
            return Changeset.FromXml(result);
        }

        internal void UploadFile(WorkspaceData workspaceData, PendingChange change)
        {
            if (change.ItemType != ItemType.File)
                return;
            if (change.ChangeType.HasFlag(ChangeType.Edit) || change.ChangeType.HasFlag(ChangeType.Add))
                UploadFile(workspaceData.Name, workspaceData.Owner, change);
        }

        #region Result Extractors

        private List<Failure> FailuresExtractor(XElement response)
        {
            var failures = response.GetElement("failures");
            if (failures != null)
                return failures.GetElements("Failure").Select(Failure.FromXml).ToList();
            return new List<Failure>();
        }

        private List<GetOperation> GetOperationExtractor(XElement element)
        {
            return element.GetElements("GetOperation").Select(GetOperation.FromXml).ToList();
        }

        private List<Conflict> ConflictExtractor(XElement element)
        {
            return element.GetElements("Conflict").Select(Conflict.FromXml).ToList();
        }

        #endregion

        internal CheckInResult CheckIn(WorkspaceData workspaceData, IEnumerable<PendingChange> changes, string comment, Dictionary<int, WorkItemCheckinAction> workItems)
        {
            var invoker = GetSoapInvoker();
            var msg = invoker.CreateEnvelope("CheckIn");
            msg.Add(new XElement(MessageNs + "workspaceName", workspaceData.Name));
            msg.Add(new XElement(MessageNs + "ownerName", workspaceData.Owner));
            msg.Add(new XElement(MessageNs + "serverItems", changes.Select(x => x.ServerItem).Distinct().Select(s => new XElement(MessageNs + "string", s))));
            msg.Add(new XElement(MessageNs + "info",
                new XAttribute("date", new DateTime(0).ToString("s")),
                new XAttribute("cset", 0),
                new XAttribute("owner", workspaceData.Owner),
                new XElement(MessageNs + "Comment", comment),
                new XElement(MessageNs + "CheckinNote", string.Empty),
                new XElement(MessageNs + "PolicyOverride", string.Empty)));

            if (workItems != null && workItems.Count > 0)
            {
                msg.Add(new XElement(MessageNs + "checkinNotificationInfo",
                    new XElement(MessageNs + "WorkItemInfo",
                        workItems.Select(wi => new XElement(MessageNs + "CheckinNotificationWorkItemInfo", 
                            new XElement(MessageNs + "Id", wi.Key), 
                            new XElement(MessageNs + "CheckinAction", wi.Value))))));
            }
            var response = invoker.InvokeResponse();
            var resultElement = invoker.MethodResultExtractor(response);

            var result = CheckInResult.FromXml(resultElement);
            result.Failures = FailuresExtractor(response);
            return result;
        }

        internal List<Conflict> QueryConflicts(WorkspaceData workspaceData, IEnumerable<ItemSpec> items)
        {
            var invoker = GetSoapInvoker();
            var msg = invoker.CreateEnvelope("QueryConflicts");
            msg.AddElement("workspaceName", workspaceData.Name);
            msg.AddElement("ownerName", workspaceData.Owner);
            msg.AddElement("items", items.Select(itemSpec => itemSpec.ToXml("ItemSpec")));

            var result = invoker.InvokeResult();
            return ConflictExtractor(result);
        }

        internal ResolveResult Resolve(WorkspaceData workspaceData, Conflict conflict, ResolutionType resolutionType)
        {
            var invoker = GetSoapInvoker();
            var msg = invoker.CreateEnvelope("Resolve");
            msg.Add(new XElement(MessageNs + "workspaceName", workspaceData.Name));
            msg.Add(new XElement(MessageNs + "ownerName", workspaceData.Owner));
            msg.Add(new XElement(MessageNs + "conflictId", conflict.ConflictId));
            msg.Add(new XElement(MessageNs + "resolution", resolutionType));
            var response = invoker.InvokeResponse();
            ResolveResult result = new ResolveResult();
            result.GetOperations = GetOperationExtractor(invoker.MethodResultExtractor(response));
            result.UndoOperations = GetOperationExtractor(response.Element(MessageNs + "undoOperations"));
            result.ResolvedConflicts = ConflictExtractor(response.Element(MessageNs + "resolvedConflicts"));
            return result;
        }
        //		public void ShelveFile(string workspaceName, string ownerName, PendingChange change)
        //		{
        //			UploadFile(workspaceName, ownerName, change);
        //		}
        private void UploadFile(string workspaceName, string ownerName, PendingChange change)
        {
            UploadService.UploadFile(workspaceName, ownerName, change);
        }
    }
}

