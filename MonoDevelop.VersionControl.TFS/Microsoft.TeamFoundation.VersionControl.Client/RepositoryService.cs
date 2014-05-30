//
// VersionControlService.cs
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
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using System;
using Microsoft.TeamFoundation.VersionControl.Client.Objects;
using Microsoft.TeamFoundation.VersionControl.Client.Helpers;
using Microsoft.TeamFoundation.VersionControl.Client.Enums;
using SoapInvoker = Microsoft.TeamFoundation.Client.SoapInvoker;
using Microsoft.TeamFoundation.Client.Services;
using Microsoft.TeamFoundation.WorkItemTracking.Client.Enums;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
    public class RepositoryService : TFSCollectionService
    {
        #region TfsService

        public override XNamespace MessageNs
        {
            get
            {
                return "http://schemas.microsoft.com/TeamFoundation/2005/06/VersionControl/ClientServices/03";
            }
        }

        public override Microsoft.TeamFoundation.Client.IServiceResolver ServiceResolver
        {
            get
            {
                return new VersionControlServiceResolver();
            }
        }

        #endregion

        #region Workspaces

        public Workspace QueryWorkspace(string ownerName, string workspaceName)
        {
            var invoker = new SoapInvoker(this);
            XElement msg = invoker.CreateEnvelope("QueryWorkspace");
            msg.Add(new XElement(MessageNs + "workspaceName", workspaceName));
            msg.Add(new XElement(MessageNs + "ownerName", ownerName));

            XElement result = invoker.InvokeResult();
            return Workspace.FromXml(this, result);
        }

        public List<Workspace> QueryWorkspaces(string ownerName, string computer)
        {
            var invoker = new SoapInvoker(this);
            XElement msg = invoker.CreateEnvelope("QueryWorkspaces");
            if (!string.IsNullOrEmpty(ownerName))
                msg.Add(new XElement(MessageNs + "ownerName", ownerName));
            if (!string.IsNullOrEmpty(computer))
                msg.Add(new XElement(MessageNs + "computer", computer));

            List<Workspace> workspaces = new List<Workspace>();
            XElement result = invoker.InvokeResult();
            workspaces.AddRange(result.Elements(MessageNs + "Workspace").Select(el => Workspace.FromXml(this, el)));
            workspaces.Sort();
            return workspaces;
        }

        public Workspace UpdateWorkspace(string oldWorkspaceName, string ownerName,
                                         Workspace newWorkspace)
        {
            var invoker = new SoapInvoker(this);
            XElement msg = invoker.CreateEnvelope("UpdateWorkspace");

            msg.Add(new XElement(MessageNs + "oldWorkspaceName", oldWorkspaceName));
            msg.Add(new XElement(MessageNs + "ownerName", ownerName));
            msg.Add(newWorkspace.ToXml(MessageNs + "newWorkspace"));

            XElement result = invoker.InvokeResult();
            return Workspace.FromXml(this, result);
        }

        public Workspace CreateWorkspace(Workspace workspace)
        {
            var invoker = new SoapInvoker(this);
            XElement msg = invoker.CreateEnvelope("CreateWorkspace");
            msg.Add(workspace.ToXml(MessageNs + "workspace"));
            XElement result = invoker.InvokeResult();
            return Workspace.FromXml(this, result);
        }
        //    <DeleteWorkspace xmlns="http://schemas.microsoft.com/TeamFoundation/2005/06/VersionControl/ClientServices/03">
        //      <workspaceName>string</workspaceName>
        //      <ownerName>string</ownerName>
        //    </DeleteWorkspace>
        public void DeleteWorkspace(string workspaceName, string ownerName)
        {
            var invoker = new SoapInvoker(this);
            var msg = invoker.CreateEnvelope("DeleteWorkspace");
            msg.Add(new XElement(MessageNs + "workspaceName", workspaceName));
            msg.Add(new XElement(MessageNs + "ownerName", ownerName));
            invoker.InvokeResult();
        }

        #endregion

        internal void UpdateLocalVersion(UpdateLocalVersionQueue updateLocalVersionQueue)
        {
            var invoker = new SoapInvoker(this);
            var msg = invoker.CreateEnvelope("UpdateLocalVersion");
            foreach (var el in updateLocalVersionQueue.ToXml(MessageNs))
            {
                msg.Add(el);
            }
            invoker.InvokeResult();
        }

        #region Query Items

        //    <QueryItems xmlns="http://schemas.microsoft.com/TeamFoundation/2005/06/VersionControl/ClientServices/03">
        //      <workspaceName>string</workspaceName>
        //      <workspaceOwner>string</workspaceOwner>
        //      <items>
        //        <ItemSpec item="string" recurse="None or OneLevel or Full" did="int" />
        //        <ItemSpec item="string" recurse="None or OneLevel or Full" did="int" />
        //      </items>
        //      <version />
        //      <deletedState>NonDeleted or Deleted or Any</deletedState>
        //      <itemType>Any or Folder or File</itemType>
        //      <generateDownloadUrls>boolean</generateDownloadUrls>
        //      <options>int</options>
        //    </QueryItems>
        public List<Item> QueryItems(string workspaceName, string workspaceOwner, ItemSpec[] itemSpecs, VersionSpec versionSpec,
                                     DeletedState deletedState, ItemType itemType, 
                                     bool includeDownloadInfo)
        {
            var invoker = new SoapInvoker(this);
            var msg = invoker.CreateEnvelope("QueryItems");
            if (!string.IsNullOrEmpty(workspaceName))
                msg.Add(new XElement(MessageNs + "workspaceName", workspaceName));
            if (!string.IsNullOrEmpty(workspaceOwner))
                msg.Add(new XElement(MessageNs + "workspaceOwner", workspaceOwner));
            msg.Add(new XElement(MessageNs + "items", itemSpecs.Select(itemSpec => itemSpec.ToXml(MessageNs + "ItemSpec"))));
            msg.Add(versionSpec.ToXml(MessageNs + "version"));
            msg.Add(new XElement(MessageNs + "deletedState", deletedState));
            msg.Add(new XElement(MessageNs + "itemType", itemType));
            msg.Add(new XElement(MessageNs + "generateDownloadUrls", includeDownloadInfo.ToLowString()));

            var result = invoker.InvokeResult();
            return result.Descendants(MessageNs + "Item").Select(Item.FromXml).ToList();
        }

        public List<Item> QueryItems(ItemSpec itemSpec, VersionSpec versionSpec,
                                     DeletedState deletedState, ItemType itemType, 
                                     bool includeDownloadInfo)
        {
            return QueryItems(string.Empty, string.Empty, new [] { itemSpec }, versionSpec, deletedState, itemType, includeDownloadInfo);
        }

        public List<Item> QueryItems(Workspace workspace, ItemSpec itemSpec, VersionSpec versionSpec,
                                     DeletedState deletedState, ItemType itemType, 
                                     bool includeDownloadInfo)
        {
            if (workspace == null)
                return QueryItems(itemSpec, versionSpec, deletedState, itemType, includeDownloadInfo);
            return QueryItems(workspace.Name, workspace.OwnerName, new [] { itemSpec }, versionSpec, deletedState, itemType, includeDownloadInfo);
        }
        //    <QueryItemsExtended xmlns="http://schemas.microsoft.com/TeamFoundation/2005/06/VersionControl/ClientServices/03">
        //      <workspaceName>string</workspaceName>
        //      <workspaceOwner>string</workspaceOwner>
        //      <items>
        //        <ItemSpec item="string" recurse="None or OneLevel or Full" did="int" />
        //        <ItemSpec item="string" recurse="None or OneLevel or Full" did="int" />
        //      </items>
        //      <deletedState>NonDeleted or Deleted or Any</deletedState>
        //      <itemType>Any or Folder or File</itemType>
        //      <options>int</options>
        //    </QueryItemsExtended>
        public List<ExtendedItem> QueryItemsExtended(string workspaceName, string workspaceOwner, List<ItemSpec>  itemSpecs,
                                                     DeletedState deletedState, ItemType itemType)
        {
            var invoker = new SoapInvoker(this);
            var msg = invoker.CreateEnvelope("QueryItemsExtended");
            msg.Add(new XElement(MessageNs + "workspaceName", workspaceName));
            msg.Add(new XElement(MessageNs + "workspaceOwner", workspaceOwner));
            msg.Add(new XElement(MessageNs + "items", itemSpecs.Select(itemSpec => itemSpec.ToXml(MessageNs + "ItemSpec"))));
            msg.Add(new XElement(MessageNs + "deletedState", deletedState));
            msg.Add(new XElement(MessageNs + "itemType", itemType));

            var result = invoker.InvokeResult();
            return result.Descendants(MessageNs + "ExtendedItem").Select(ExtendedItem.FromXml).ToList();
        }

        public List<ExtendedItem> QueryItemsExtended(Workspace workspace, ItemSpec itemSpec,
                                                     DeletedState deletedState, ItemType itemType)
        {
            if (workspace == null)
                return QueryItemsExtended(string.Empty, string.Empty, new List<ItemSpec> { itemSpec }, deletedState, itemType);
            return QueryItemsExtended(workspace.Name, workspace.OwnerName, new List<ItemSpec> { itemSpec }, deletedState, itemType);
        }

        #endregion

        internal List<GetOperation> Get(Workspace workspace,
                                        List<GetRequest> requests, bool force, bool noGet)
        {
            if (workspace == null)
                throw new System.ArgumentNullException("workspace");
            var invoker = new SoapInvoker(this);
            var msg = invoker.CreateEnvelope("Get");
            msg.Add(new XElement(MessageNs + "workspaceName", workspace.Name));
            msg.Add(new XElement(MessageNs + "ownerName", workspace.OwnerName));
            msg.Add(new XElement(MessageNs + "requests", requests.Select(r => r.ToXml(MessageNs))));
            if (force)
                msg.Add(new XElement(MessageNs + "force", force.ToLowString()));
            if (noGet)
                msg.Add(new XElement(MessageNs + "noGet", noGet.ToLowString()));

            List<GetOperation> operations = new List<GetOperation>();
            var result = invoker.InvokeResult();

            foreach (var operation in result.XPathSelectElements("msg:ArrayOfGetOperation/msg:GetOperation", NsResolver))
            {
                operations.Add(GetOperation.FromXml(operation));
            }
            return operations;
        }

        public List<PendingSet> QueryPendingSets(string localWorkspaceName, string localWorkspaceOwner,
                                                 string queryWorkspaceName, string ownerName,
                                                 ItemSpec[] itemSpecs, bool generateDownloadUrls)
        {
            var invoker = new SoapInvoker(this);
            var msg = invoker.CreateEnvelope("QueryPendingSets");
            msg.Add(new XElement(MessageNs + "localWorkspaceName", localWorkspaceName));
            msg.Add(new XElement(MessageNs + "localWorkspaceOwner", localWorkspaceOwner));
            msg.Add(new XElement(MessageNs + "queryWorkspaceName", queryWorkspaceName));
            msg.Add(new XElement(MessageNs + "ownerName", ownerName));
            msg.Add(new XElement(MessageNs + "itemSpecs", itemSpecs.Select(i => i.ToXml(MessageNs + "ItemSpec"))));
            msg.Add(new XElement(MessageNs + "generateDownloadUrls", generateDownloadUrls.ToLowString()));

            var result = invoker.InvokeResult();
            return new List<PendingSet>(result.Elements(MessageNs + "PendingSet").Select(PendingSet.FromXml));
//            var pendingChangesElements = result.Descendants(MessageNs + "PendingChange");
//            var failuresElements = result.Descendants(MessageNs + "PendingChange");
//
//            var changes = new List<PendingChange>(pendingChangesElements.Select(el => PendingChange.FromXml(el)));
//            var faillist = new List<Failure>(failuresElements.Select(el => Failure.FromXml(el)));
//            return new Tuple<List<PendingChange>, List<Failure>>(changes, faillist);
        }
        //    <PendChanges xmlns="http://schemas.microsoft.com/TeamFoundation/2005/06/VersionControl/ClientServices/03">
        //      <workspaceName>string</workspaceName>
        //      <ownerName>string</ownerName>
        //      <changes>
        //        <ChangeRequest req="None or Add or Branch or Encoding or Edit or Delete or Lock or Rename or Undelete or Property" did="int" enc="int" type="Any or Folder or File" lock="None or Checkin or CheckOut or Unchanged" target="string" targettype="Any or Folder or File">
        //          <item item="string" recurse="None or OneLevel or Full" did="int" />
        //          <vspec />
        //          <Properties>
        //            <PropertyValue xsi:nil="true" />
        //            <PropertyValue xsi:nil="true" />
        //          </Properties>
        //        </ChangeRequest>
        //        <ChangeRequest req="None or Add or Branch or Encoding or Edit or Delete or Lock or Rename or Undelete or Property" did="int" enc="int" type="Any or Folder or File" lock="None or Checkin or CheckOut or Unchanged" target="string" targettype="Any or Folder or File">
        //          <item item="string" recurse="None or OneLevel or Full" did="int" />
        //          <vspec />
        //          <Properties>
        //            <PropertyValue xsi:nil="true" />
        //            <PropertyValue xsi:nil="true" />
        //          </Properties>
        //        </ChangeRequest>
        //      </changes>
        //      <pendChangesOptions>int</pendChangesOptions>
        //      <supportedFeatures>int</supportedFeatures>
        //    </PendChanges>
        //      <PendChangesResult>
        //        <GetOperation type="Any or Folder or File" itemid="int" slocal="string" tlocal="string" titem="string" sitem="string" sver="int" vrevto="int" lver="int" did="int" chgEx="int" chg="None or Add or Edit or Encoding or Rename or Delete or Undelete or Branch or Merge or Lock or Rollback or SourceRename or Property" lock="None or Checkin or CheckOut or Unchanged" il="boolean" pcid="int" cnflct="boolean" cnflctchg="None or Add or Edit or Encoding or Rename or Delete or Undelete or Branch or Merge or Lock or Rollback or SourceRename or Property" cnflctchgEx="int" cnflctitemid="int" nmscnflct="unsignedByte" durl="string" enc="int" vsd="dateTime">
        //          <HashValue>base64Binary</HashValue>
        //          <Properties>
        //            <PropertyValue xsi:nil="true" />
        //            <PropertyValue xsi:nil="true" />
        //          </Properties>
        //          <PropertyValues>
        //            <PropertyValue xsi:nil="true" />
        //            <PropertyValue xsi:nil="true" />
        //          </PropertyValues>
        //        </GetOperation>
        //        <GetOperation type="Any or Folder or File" itemid="int" slocal="string" tlocal="string" titem="string" sitem="string" sver="int" vrevto="int" lver="int" did="int" chgEx="int" chg="None or Add or Edit or Encoding or Rename or Delete or Undelete or Branch or Merge or Lock or Rollback or SourceRename or Property" lock="None or Checkin or CheckOut or Unchanged" il="boolean" pcid="int" cnflct="boolean" cnflctchg="None or Add or Edit or Encoding or Rename or Delete or Undelete or Branch or Merge or Lock or Rollback or SourceRename or Property" cnflctchgEx="int" cnflctitemid="int" nmscnflct="unsignedByte" durl="string" enc="int" vsd="dateTime">
        //          <HashValue>base64Binary</HashValue>
        //          <Properties>
        //            <PropertyValue xsi:nil="true" />
        //            <PropertyValue xsi:nil="true" />
        //          </Properties>
        //          <PropertyValues>
        //            <PropertyValue xsi:nil="true" />
        //            <PropertyValue xsi:nil="true" />
        //          </PropertyValues>
        //        </GetOperation>
        //      </PendChangesResult>
        internal List<GetOperation> PendChanges(Workspace workspace, List<ChangeRequest> changeRequest, out List<Failure> failures)
        {
            var invoker = new SoapInvoker(this);
            var msg = invoker.CreateEnvelope("PendChanges");
            msg.Add(new XElement(MessageNs + "workspaceName", workspace.Name));
            msg.Add(new XElement(MessageNs + "ownerName", workspace.OwnerName));
            msg.Add(new XElement(MessageNs + "changes", changeRequest.Select(x => x.ToXml(MessageNs))));
            var response = invoker.InvokeResponse();
            failures = FailuresExtractor(response);
            var result = invoker.MethodResultExtractor(response);
            return result.Elements(MessageNs + "GetOperation").Select(GetOperation.FromXml).ToList();
        }

        internal List<GetOperation> UndoPendChanges(Workspace workspace, List<ItemSpec> itemSpecs)
        {
            var invoker = new SoapInvoker(this);
            var msg = invoker.CreateEnvelope("UndoPendingChanges");
            msg.Add(new XElement(MessageNs + "workspaceName", workspace.Name));
            msg.Add(new XElement(MessageNs + "ownerName", workspace.OwnerName));
            msg.Add(new XElement(MessageNs + "items", itemSpecs.Select(x => x.ToXml(MessageNs + "ItemSpec"))));
            var result = invoker.InvokeResult();
            return GetOperationExtractor(result);
        }

        internal List<PendingChange> QueryPendingChangesForWorkspace(Workspace workspace, List<ItemSpec> itemSpecs, bool includeDownloadInfo)
        {
            var invoker = new SoapInvoker(this);
            var msg = invoker.CreateEnvelope("QueryPendingChangesForWorkspace");
            msg.Add(new XElement(MessageNs + "workspaceName", workspace.Name));
            msg.Add(new XElement(MessageNs + "workspaceOwner", workspace.OwnerName));
            msg.Add(new XElement(MessageNs + "itemSpecs", itemSpecs.Select(i => i.ToXml(MessageNs + "ItemSpec"))));
            msg.Add(new XElement(MessageNs + "generateDownloadUrls", includeDownloadInfo.ToLowString()));
            var result = invoker.InvokeResult();
            return result.Elements(MessageNs + "PendingChange").Select(PendingChange.FromXml).ToList();
        }

        public List<Changeset> QueryHistory(ItemSpec item, VersionSpec versionItem, 
                                            VersionSpec versionFrom, VersionSpec versionTo, int maxCount = short.MaxValue)
        {
            var invoker = new SoapInvoker(this);
            var msg = invoker.CreateEnvelope("QueryHistory");
            msg.Add(item.ToXml(MessageNs + "itemSpec"));
            msg.Add(versionItem.ToXml(MessageNs + "versionItem"));
            if (versionFrom != null)
                msg.Add(versionFrom.ToXml(MessageNs + "versionFrom"));
            if (versionTo != null)
                msg.Add(versionTo.ToXml(MessageNs + "versionTo"));
            msg.Add(new XElement(MessageNs + "maxCount", maxCount));
            msg.Add(new XElement(MessageNs + "includeFiles", false));
            msg.Add(new XElement(MessageNs + "generateDownloadUrls", false));
            msg.Add(new XElement(MessageNs + "slotMode", false));
            msg.Add(new XElement(MessageNs + "sortAscending", false));

            var result = invoker.InvokeResult();
            return result.Elements(MessageNs + "Changeset").Select(Changeset.FromXml).ToList();
        }

        public Changeset QueryChangeset(int changeSetId, bool includeChanges = false, bool includeDownloadUrls = false, bool includeSourceRenames = true)
        {
            var invoker = new SoapInvoker(this);
            var msg = invoker.CreateEnvelope("QueryChangeset");
            msg.Add(new XElement(MessageNs + "changesetId", changeSetId));
            msg.Add(new XElement(MessageNs + "includeChanges", includeChanges));
            msg.Add(new XElement(MessageNs + "generateDownloadUrls", includeDownloadUrls));
            msg.Add(new XElement(MessageNs + "includeSourceRenames", includeSourceRenames));

            var result = invoker.InvokeResult();
            return Changeset.FromXml(result);
        }

        internal void UploadFile(Workspace workspace, PendingChange change)
        {
            if (change.ItemType != ItemType.File)
                return;
            if (change.ChangeType.HasFlag(ChangeType.Edit) || change.ChangeType.HasFlag(ChangeType.Add))
                UploadFile(workspace.Name, workspace.OwnerName, change);
        }

        #region Result Extractors

        private List<Failure> FailuresExtractor(XElement response)
        {
            var failures = response.Element(MessageNs + "failures");
            if (failures != null)
                return failures.Elements(MessageNs + "Failure").Select(x => Failure.FromXml(x)).ToList();
            return new List<Failure>();
        }

        private List<GetOperation> GetOperationExtractor(XElement element)
        {
            return element.Elements(MessageNs + "GetOperation").Select(GetOperation.FromXml).ToList();
        }

        private List<Conflict> ConflictExtractor(XElement element, Workspace wsp)
        {
            return element.Elements(MessageNs + "Conflict").Select(x => Conflict.FromXml(x, wsp)).ToList();
        }

        #endregion

        internal CheckInResult CheckIn(Workspace workspace, List<PendingChange> changes, string comment, Dictionary<int, WorkItemCheckinAction> workItems)
        {
            var invoker = new SoapInvoker(this);
            var msg = invoker.CreateEnvelope("CheckIn");
            msg.Add(new XElement(MessageNs + "workspaceName", workspace.Name));
            msg.Add(new XElement(MessageNs + "ownerName", workspace.OwnerName));
            msg.Add(new XElement(MessageNs + "serverItems", changes.Select(x => x.ServerItem).Distinct().Select(s => new XElement(MessageNs + "string", s))));
            msg.Add(new XElement(MessageNs + "info",
                new XAttribute("date", new DateTime(0).ToString("s")),
                new XAttribute("cset", 0),
                new XAttribute("owner", workspace.OwnerName),
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

        internal List<Conflict> QueryConflicts(Workspace workspace, List<ItemSpec> items)
        {
            var invoker = new SoapInvoker(this);
            var msg = invoker.CreateEnvelope("QueryConflicts");
            msg.Add(new XElement(MessageNs + "workspaceName", workspace.Name));
            msg.Add(new XElement(MessageNs + "ownerName", workspace.OwnerName));
            msg.Add(new XElement(MessageNs + "items", items.Select(itemSpec => itemSpec.ToXml(MessageNs + "ItemSpec"))));

            var result = invoker.InvokeResult();
            return ConflictExtractor(result, workspace);
        }

        internal ResolveResult Resolve(Conflict conflict, ResolutionType resolutionType)
        {
            var invoker = new SoapInvoker(this);
            var msg = invoker.CreateEnvelope("Resolve");
            msg.Add(new XElement(MessageNs + "workspaceName", conflict.Workspace.Name));
            msg.Add(new XElement(MessageNs + "ownerName", conflict.Workspace.OwnerName));
            msg.Add(new XElement(MessageNs + "conflictId", conflict.ConflictId));
            msg.Add(new XElement(MessageNs + "resolution", resolutionType));
            var response = invoker.InvokeResponse();
            ResolveResult result = new ResolveResult();
            result.GetOperations = GetOperationExtractor(invoker.MethodResultExtractor(response));
            result.UndoOperations = GetOperationExtractor(response.Element(MessageNs + "undoOperations"));
            result.ResolvedConflicts = ConflictExtractor(response.Element(MessageNs + "resolvedConflicts"), conflict.Workspace);
            return result;
        }
        //		public void ShelveFile(string workspaceName, string ownerName, PendingChange change)
        //		{
        //			UploadFile(workspaceName, ownerName, change);
        //		}
        private void UploadFile(string workspaceName, string ownerName, PendingChange change)
        {
            var uploadService = this.Collection.GetService<UploadService>();
            uploadService.UploadFile(workspaceName, ownerName, change);
        }
    }
}

