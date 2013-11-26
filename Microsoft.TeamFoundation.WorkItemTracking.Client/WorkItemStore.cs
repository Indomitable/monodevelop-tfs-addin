//
// WorkItemStore.cs
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

using System.Collections.Generic;
using Microsoft.TeamFoundation.WorkItemTracking.Client.Objects;
using Microsoft.TeamFoundation.WorkItemTracking.Client.Metadata;
using MonoDevelop.Core;
using System;
using System.Linq;

namespace Microsoft.TeamFoundation.WorkItemTracking.Client
{
    public sealed class WorkItemStore
    {
        readonly StoredQuery query;
        readonly ClientService clientService;

        public WorkItemStore(StoredQuery query)
        {
            this.clientService = query.Collection.GetService<ClientService>();
            this.query = query;
        }

        public List<WorkItem> LoadByWorkItem(IProgressMonitor progress)
        {
            var ids = this.clientService.GetWorkItemIds(this.query, CachedMetaData.Instance.Fields);
            var list = new List<WorkItem>();
            progress.BeginTask("Loading WorkItems", ids.Count);
            foreach (var id in ids)
            {
                list.Add(clientService.GetWorkItem(id));
                progress.Step(1);
            }
            progress.EndTask();
            return list;
        }

        public List<WorkItem> LoadByPage(IProgressMonitor progress)
        {
            var ids = this.clientService.GetWorkItemIds(this.query, CachedMetaData.Instance.Fields);
            int pages = (int)Math.Ceiling((double)ids.Count / (double)50);
            var result = new List<WorkItem>();
            progress.BeginTask("Loading WorkItems", pages);
            for (int i = 0; i < pages; i++)
            {
                var idList = new List<int>(ids.Skip(i * 50).Take(50));
                var items = this.clientService.PageWorkitemsByIds(this.query, idList);
                result.AddRange(items);
                progress.Step(1);
            }
            progress.EndTask();
            return result;
        }
    }
}
