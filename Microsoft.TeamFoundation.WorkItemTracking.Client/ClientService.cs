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
using Microsoft.TeamFoundation.Client.Services;
using System.Xml.Linq;
using Microsoft.TeamFoundation.Client;
using System.Collections.Generic;
using Microsoft.TeamFoundation.WorkItemTracking.Client.Enums;
using Microsoft.TeamFoundation.WorkItemTracking.Client.Objects;
using Microsoft.TeamFoundation.WorkItemTracking.Client.Metadata;
using System.Net;

namespace Microsoft.TeamFoundation.WorkItemTracking.Client
{
    public class ClientService : TFSCollectionService
    {
        private class ClientServiceResolver : IServiceResolver
        {

            #region IServiceResolver implementation

            public string Id
            {
                get
                {
                    return "ca87fa49-58c9-4089-8535-1299fa60eebc";
                }
            }

            public string ServiceType
            {
                get
                {
                    return "WorkitemService3";
                }
            }

            #endregion

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

        public override IServiceResolver ServiceResolver
        {
            get
            {
                return new ClientServiceResolver();
            }
        }

        #endregion

        private static readonly string headerName = "RequestHeader";
        private static readonly string requestId = "uuid:" + Guid.NewGuid().ToString("D");

        private XElement GetHeaderElement()
        {
            return new XElement(this.MessageNs + "Id", requestId);
        }

        private List<T> GetMetadata<T>(MetadataRowSetNames table)
            where T: class
        {
            var invoker = new SoapInvoker(this);
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
            var linearHierarchies = GetMetadata<Hierarchy>(MetadataRowSetNames.Hierarchy);
            List<Hierarchy> hierarchies = new List<Hierarchy>();
            for (int i = 0; i < linearHierarchies.Count; i++)
            {
                var hierarchy = linearHierarchies[i];
                if (string.Equals(hierarchy.Name, "\\"))
                {
                    //Root
                    hierarchy.Parent = null;
                    hierarchies.Add(hierarchy);
                }
                else
                {
                    for (int j = i - 1; j >= 0; j--)
                    {
                        var prevHierchy = linearHierarchies[j];
                        if (prevHierchy.AreaId == hierarchy.ParentId)
                        {
                            //Found Parent
                            prevHierchy.Children.Add(hierarchy);
                            hierarchy.Parent = prevHierchy;
                            break;
                        }
                    }
                }
            }
            return hierarchies;
        }

        public List<StoredQuery> GetStoredQueries(Project project)
        {
            var invoker = new SoapInvoker(this);
            var msg = invoker.CreateEnvelope("GetStoredQueries", headerName);
            msg.Header.Add(GetHeaderElement());
            msg.Body.Add(new XElement(MessageNs + "rowVersion", 0));
            msg.Body.Add(new XElement(MessageNs + "projectId", project.Id));
            var response = invoker.InvokeResponse();
            var extractor = new TableExtractor<StoredQuery>(response, "StoredQueries");
            return extractor.Extract();
        }
    }
}
