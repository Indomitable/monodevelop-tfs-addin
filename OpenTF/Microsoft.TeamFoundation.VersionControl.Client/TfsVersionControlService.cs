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
using Microsoft.TeamFoundation.Common;
using System.Xml.XPath;
using Microsoft.TeamFoundation.Client;
using System.IO;
using System;
using System.Runtime.InteropServices;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
    public class TfsVersionControlService : TfsService
    {
        public override System.Xml.Linq.XNamespace MessageNs
        {
            get
            {
                return "http://schemas.microsoft.com/TeamFoundation/2005/06/VersionControl/ClientServices/03";
            }
        }

        public override IServiceResolver ServiceResolver
        {
            get
            {
                return new VersionControlServiceResolver();
            }
        }

        #region Workspaces

        public Workspace QueryWorkspace(string workspaceName, string ownerName)
        {
            XElement msg = Invoker.CreateEnvelope("QueryWorkspace");
            msg.Add(new XElement(MessageNs + "workspaceName", workspaceName));
            msg.Add(new XElement(MessageNs + "ownerName", ownerName));

            XElement result = Invoker.Invoke();
            return Workspace.FromXml(this, result);
        }

        public List<Workspace> QueryWorkspaces(string ownerName, string computer)
        {
            XElement msg = Invoker.CreateEnvelope("QueryWorkspaces");
            if (!string.IsNullOrEmpty(ownerName))
                msg.Add(new XElement(MessageNs + "ownerName", ownerName));
            if (!string.IsNullOrEmpty(computer))
                msg.Add(new XElement(MessageNs + "computer", computer));

            List<Workspace> workspaces = new List<Workspace>();
            XElement result = Invoker.Invoke();
            workspaces.AddRange(result.Elements(XmlNamespaces.GetMessageElementName("Workspace")).Select(el => Workspace.FromXml(this, el)));
            workspaces.Sort();
            return workspaces;
        }

        public Workspace UpdateWorkspace(string oldWorkspaceName, string ownerName,
                                         Workspace newWorkspace)
        {
            XElement msg = Invoker.CreateEnvelope("UpdateWorkspace");

            msg.Add(new XElement(MessageNs + "oldWorkspaceName", oldWorkspaceName));
            msg.Add(new XElement(MessageNs + "ownerName", ownerName));
            msg.Add(newWorkspace.ToXml("newWorkspace"));

            XElement result = Invoker.Invoke();
            return Workspace.FromXml(this, result);
        }

        public Workspace CreateWorkspace(Workspace workspace)
        {
            XElement msg = Invoker.CreateEnvelope("CreateWorkspace");
            msg.Add(workspace.ToXml("workspace"));
            XElement result = Invoker.Invoke();
            return Workspace.FromXml(this, result);
        }
        //    <DeleteWorkspace xmlns="http://schemas.microsoft.com/TeamFoundation/2005/06/VersionControl/ClientServices/03">
        //      <workspaceName>string</workspaceName>
        //      <ownerName>string</ownerName>
        //    </DeleteWorkspace>
        public void DeleteWorkspace(string workspaceName, string ownerName)
        {
            var msg = Invoker.CreateEnvelope("DeleteWorkspace");
            msg.Add(new XElement("workspaceName", workspaceName));
            msg.Add(new XElement("ownerName", ownerName));
            Invoker.Invoke();
        }

        #endregion

        public void UpdateLocalVersion(UpdateLocalVersionQueue updateLocalVersionQueue)
        {
            var msg = Invoker.CreateEnvelope("UpdateLocalVersion");
            foreach (var el in updateLocalVersionQueue.ToXml(MessageNs))
            {
                msg.Add(el);
            }
            Invoker.Invoke();
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
            var msg = Invoker.CreateEnvelope("QueryItems");
            if (!string.IsNullOrEmpty(workspaceName))
                msg.Add(new XElement(MessageNs + "workspaceName", workspaceName));
            if (!string.IsNullOrEmpty(workspaceOwner))
                msg.Add(new XElement(MessageNs + "workspaceOwner", workspaceOwner));
            msg.Add(new XElement(MessageNs + "items", itemSpecs.Select(itemSpec => itemSpec.ToXml(MessageNs + "ItemSpec"))));
            msg.Add(versionSpec.ToXml(MessageNs + "version"));
            msg.Add(new XElement(MessageNs + "deletedState", deletedState));
            msg.Add(new XElement(MessageNs + "itemType", itemType));
            msg.Add(new XElement(MessageNs + "generateDownloadUrls", includeDownloadInfo.ToLowString()));

            var result = Invoker.Invoke();
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
        public List<ExtendedItem> QueryItemsExtended(string workspaceName, string workspaceOwner, ItemSpec[] itemSpecs,
                                                     DeletedState deletedState, ItemType itemType)
        {
            var msg = Invoker.CreateEnvelope("QueryItemsExtended");
            msg.Add(new XElement(MessageNs + "workspaceName", workspaceName));
            msg.Add(new XElement(MessageNs + "workspaceOwner", workspaceOwner));
            msg.Add(new XElement(MessageNs + "items", itemSpecs.Select(itemSpec => itemSpec.ToXml(MessageNs + "ItemSpec"))));
            msg.Add(new XElement(MessageNs + "deletedState", deletedState));
            msg.Add(new XElement(MessageNs + "itemType", itemType));

            var result = Invoker.Invoke();
            File.AppendAllText("/home/vmladenov/res_" + itemSpecs[0].Item + ".txt", result.ToString());
            return result.Descendants(MessageNs + "ExtendedItem").Select(ExtendedItem.FromXml).ToList();
        }

        public List<ExtendedItem> QueryItemsExtended(Workspace workspace, ItemSpec itemSpec,
                                                     DeletedState deletedState, ItemType itemType)
        {
            if (workspace == null)
                return QueryItemsExtended(string.Empty, string.Empty, new [] { itemSpec }, deletedState, itemType);
            return QueryItemsExtended(workspace.Name, workspace.OwnerName, new [] { itemSpec }, deletedState, itemType);
        }

        #endregion

        internal List<GetOperation> Get(Workspace workspace,
                                        GetRequest[] requests, bool force, bool noGet)
        {
            if (workspace == null)
                throw new System.ArgumentNullException("workspace");
            var msg = Invoker.CreateEnvelope("Get");
            msg.Add(new XElement(MessageNs + "workspaceName", workspace.Name));
            msg.Add(new XElement(MessageNs + "ownerName", workspace.OwnerName));
            msg.Add(new XElement(MessageNs + "requests", requests.Select(r => r.ToXml(MessageNs))));
            if (force)
                msg.Add(new XElement(MessageNs + "force", force.ToLowString()));
            if (noGet)
                msg.Add(new XElement(MessageNs + "noGet", noGet.ToLowString()));

            List<GetOperation> operations = new List<GetOperation>();
            var result = Invoker.Invoke();

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
            var msg = Invoker.CreateEnvelope("QueryPendingSets");
            msg.Add(new XElement(MessageNs + "localWorkspaceName", localWorkspaceName));
            msg.Add(new XElement(MessageNs + "localWorkspaceOwner", localWorkspaceOwner));
            msg.Add(new XElement(MessageNs + "queryWorkspaceName", queryWorkspaceName));
            msg.Add(new XElement(MessageNs + "ownerName", ownerName));
            msg.Add(new XElement(MessageNs + "itemSpecs", itemSpecs.Select(i => i.ToXml(MessageNs + "ItemSpec"))));
            msg.Add(new XElement(MessageNs + "generateDownloadUrls", generateDownloadUrls.ToLowString()));

            var result = Invoker.Invoke();
            return new List<PendingSet>(result.Elements(MessageNs + "PendingSet").Select(PendingSet.FromXml));
//            var pendingChangesElements = result.Descendants(MessageNs + "PendingChange");
//            var failuresElements = result.Descendants(MessageNs + "PendingChange");
//
//            var changes = new List<PendingChange>(pendingChangesElements.Select(el => PendingChange.FromXml(el)));
//            var faillist = new List<Failure>(failuresElements.Select(el => Failure.FromXml(el)));
//            return new Tuple<List<PendingChange>, List<Failure>>(changes, faillist);
        }
    }
}

