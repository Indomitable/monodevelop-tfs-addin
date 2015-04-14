//
// ClientService.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using MonoDevelop.VersionControl.TFS.Core;
using MonoDevelop.VersionControl.TFS.Core.Services;
using MonoDevelop.VersionControl.TFS.Core.Services.Resolvers;
using MonoDevelop.VersionControl.TFS.WorkItemTracking.Metadata;
using MonoDevelop.VersionControl.TFS.WorkItemTracking.Services.Resolvers;
using MonoDevelop.VersionControl.TFS.WorkItemTracking.Structure;
using MonoDevelop.VersionControl.TFS.WorkItemTracking.Query;

namespace MonoDevelop.VersionControl.TFS.WorkItemTracking.Services
{
    [ServiceResolver(typeof(ClientServiceResolver))]
    sealed class ClientService : TFSService
    {
        private ClientService(Uri baseUri, string servicePath)
            : base(baseUri, servicePath)
        {
            
        }

        #region implemented abstract members of TFSService

        public override XNamespace MessageNs
        {
            get
            {
                return "http://schemas.microsoft.com/TeamFoundation/2005/06/WorkItemTracking/ClientServices/03";
            }
        }

        #endregion

        #region implemented abstract members of TFSCollectionService

        public IServiceResolver ServiceResolver
        {
            get
            {
                return new ClientServiceResolver();
            }
        }

        #endregion

        private static readonly string headerName = "RequestHeader";
        private static readonly string requestId = "uuid:" + System.Guid.NewGuid().ToString("D");

        private XElement GetHeaderElement()
        {
            return new XElement(this.MessageNs + "Id", requestId);
        }

        private List<T> GetMetadata<T>(MetadataRowSetNames table)
            where T: class
        {
            var invoker = GetSoapInvoker();
            var envelope = invoker.CreateEnvelope("GetMetadataEx2", headerName);
            envelope.Header.Add(GetHeaderElement());
            envelope.Body.Add(new XElement(MessageNs + "metadataHave",
                new XElement(MessageNs + "MetadataTableHaveEntry",
                    new XElement(MessageNs + "TableName", table),
                    new XElement(MessageNs + "RowVersion", 0))));
            envelope.Body.Add(new XElement(MessageNs + "useMaster", "false"));
            var response = invoker.InvokeResponse();
            var extractor = new TableExtractor<T>(response, table.ToString());
            return extractor.Extract();
        }

        public List<Hierarchy> GetHierarchy()
        {
            return GetMetadata<Hierarchy>(MetadataRowSetNames.Hierarchy).Where(h => !h.IsDeleted).ToList();
        }

        public List<Field> GetFields()
        {
            return this.GetMetadata<Field>(MetadataRowSetNames.Fields);
        }

        public List<Constant> GetConstants()
        {
            return this.GetMetadata<Constant>(MetadataRowSetNames.Constants).Where(c => !c.IsDeleted).ToList();
        }

        public List<WorkItemType> GetWorkItemTypes()
        {
            return this.GetMetadata<WorkItemType>(MetadataRowSetNames.WorkItemTypes).Where(t => !t.IsDeleted).ToList();
        }

        public List<Structure.Action> GetActions()
        {
            return this.GetMetadata<Structure.Action>(MetadataRowSetNames.Actions).Where(t => !t.IsDeleted).ToList();
        }

        public List<StoredQuery> GetStoredQueries(WorkItemProject project)
        {
            var invoker = GetSoapInvoker();
            var msg = invoker.CreateEnvelope("GetStoredQueries", headerName);
            msg.Header.Add(GetHeaderElement());
            msg.Body.Add(new XElement(MessageNs + "rowVersion", 0));
            msg.Body.Add(new XElement(MessageNs + "projectId", project.Id));
            var response = invoker.InvokeResponse();
            var extractor = new TableExtractor<StoredQuery>(response, "StoredQueries");
            return extractor.Extract();
        }

        public List<int> GetWorkItemIds(StoredQuery query, FieldList fields)
        {
            WorkItemContext context = new WorkItemContext { ProjectId = query.ProjectId, Me = WorkItemsContext.WhoAmI };

            var invoker = GetSoapInvoker();
            var envelope = invoker.CreateEnvelope("QueryWorkitems", headerName);
            envelope.Header.Add(GetHeaderElement());
            XNamespace queryNs = XNamespace.Get("");
            envelope.Body.Add(new XElement(MessageNs + "psQuery",
                new XElement(queryNs + "Query", new XAttribute("Product", this.Url.ToString()), query.GetQueryXml(context, fields))));

            XElement sorting = query.GetSortingXml();
            foreach (var items in sorting.DescendantsAndSelf())
            {
                items.Name = MessageNs + items.Name.LocalName;
            }
            envelope.Body.Add(sorting);
            envelope.Body.Add(new XElement(MessageNs + "useMaster", "false"));
            var response = invoker.InvokeResponse();

            var queryIds = response.Element(MessageNs + "resultIds").Element("QueryIds");
            if (queryIds == null)
                return new List<int>();
            var list = new List<int>();
            foreach (var item in queryIds.Elements("id"))
            {
                var startId = item.Attribute("s");
                if (startId == null)
                    continue;
                var s = Convert.ToInt32(startId.Value);
                var endId = item.Attribute("e");
                if (endId != null)
                {
                    var e = Convert.ToInt32(endId.Value);
                    var range = Enumerable.Range(s, e - s + 1);
                    list.AddRange(range);
                }
                else
                {
                    list.Add(s);
                }
            }
            return list;
        }

        public WorkItem GetWorkItem(int id)
        {
            var invoker = GetSoapInvoker();
            var msg = invoker.CreateEnvelope("GetWorkItem", headerName);
            msg.Header.Add(GetHeaderElement());
            msg.Body.Add(new XElement(MessageNs + "workItemId", id));
            var response = invoker.InvokeResponse();
            var extractor = new TableDictionaryExtractor(response, "WorkItemInfo");
            var workItem = new WorkItem();
            //workItem.Id = id;
            var data = extractor.Extract().Single();
            workItem.WorkItemInfo = new Dictionary<string, object>();
            foreach (var item in data)
            {
                workItem.WorkItemInfo.Add(item.Key, item.Value);
            }
            return workItem;
        }

        public List<WorkItem> PageWorkitemsByIds(StoredQuery query, List<int> ids)
        {
            if (ids.Count > 50)
                throw new Exception("Page only by 50");
            var invoker = GetSoapInvoker();
            var msg = invoker.CreateEnvelope("PageWorkitemsByIds", headerName);
            msg.Header.Add(GetHeaderElement());
            msg.Body.Add(new XElement(MessageNs + "ids", ids.Select(i => new XElement(MessageNs + "int", i))));
            var columns = query.GetSelectColumns();
            var fields = CachedMetaData.Instance.Fields.GetFieldsByNames(columns);
            if (fields["System.Id"] == null) //If No id Exists add it
            {
                fields.Insert(0, CachedMetaData.Instance.Fields["System.Id"]);
            }
            msg.Body.Add(new XElement(MessageNs + "columns", fields.Where(f => !f.IsLongField).Select(c => new XElement(MessageNs + "string", c.ReferenceName))));
            msg.Body.Add(new XElement(MessageNs + "longTextColumns", fields.Where(f => f.IsLongField).Select(c => new XElement(MessageNs + "int", c.Id))));
            var response = invoker.InvokeResponse();
            var extractor = new TableDictionaryExtractor(response, "Items");
            var data = extractor.Extract();
            List<WorkItem> list = new List<WorkItem>();
            foreach (var item in data)
            {
                list.Add(new WorkItem { WorkItemInfo = item });
            }
            return list;
        }

        public void Associate(int workItemId, int changeSet, string comment)
        {
            var workItem = GetWorkItem(workItemId);
            var revision = Convert.ToInt32(workItem.WorkItemInfo["System.Rev"]);
            string historyMsg = string.Format("Associated with changeset {0}.", changeSet);
            string changeSetLink = "vstfs:///VersionControl/Changeset/" + changeSet;
            string oneLineComment = comment.Replace(Environment.NewLine, " ");
            oneLineComment = oneLineComment.Substring(0, Math.Min(oneLineComment.Length, 120));

            var invoker = GetSoapInvoker();
            var msg = invoker.CreateEnvelope("Update", headerName);
            msg.Header.Add(GetHeaderElement());

            XNamespace packageNs = XNamespace.Get("");
            msg.Body.Add(new XElement(MessageNs + "package",
                new XElement(packageNs + "Package", new XAttribute("Product", this.Url.ToString()),
                    new XElement("UpdateWorkItem", new XAttribute("ObjectType", "WorkItem"), new XAttribute("Revision", revision), new XAttribute("WorkItemID", workItemId),
                        new XElement("ComputedColumns", 
                            new XElement("ComputedColumn", new XAttribute("Column", "System.RevisedDate")),
                            new XElement("ComputedColumn", new XAttribute("Column", "System.ChangedDate")),
                            new XElement("ComputedColumn", new XAttribute("Column", "System.PersonId")),
                            new XElement("ComputedColumn", new XAttribute("Column", "System.AuthorizedDate"))),
                        new XElement("InsertText", new XAttribute("FieldDisplayName", "History"), new XAttribute("FieldName", "System.History"), historyMsg),
                        new XElement("InsertResourceLink", new XAttribute("Comment", oneLineComment), new XAttribute("FieldName", "System.BISLinks"), new XAttribute("LinkType", "Fixed in Changeset"), new XAttribute("Location", changeSetLink))
                    ))));
            invoker.InvokeResponse();
        }

        public void Resolve(int workItemId, int changeSet, string comment)
        {
            throw new NotImplementedException();
        }
    }
}
