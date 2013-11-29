//
// WorkItemManager.cs
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
using Microsoft.TeamFoundation.Client;
using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation.WorkItemTracking.Client.Objects;
using Microsoft.TeamFoundation.WorkItemTracking.Client.Metadata;
using Microsoft.TeamFoundation.WorkItemTracking.Client.Enums;

namespace Microsoft.TeamFoundation.WorkItemTracking.Client
{
    public class WorkItemManager
    {
        private readonly ProjectCollection collection;
        private readonly ClientService clientService;

        public WorkItemManager(ProjectCollection collection)
        {
            this.collection = collection;
            this.clientService = collection.GetService<ClientService>();
            Init();
        }

        private void Init()
        {
            CachedMetaData.Instance.Init(this.clientService);
            var constants = CachedMetaData.Instance.Constants;
            var userName = this.collection.Server.Credentials.Domain + "\\" + this.collection.Server.UserName;
            var me = constants.FirstOrDefault(c => string.Equals(c.Value, userName, StringComparison.OrdinalIgnoreCase));
            if (me != null)
            {
                WorkItemsContext.WhoAmI = me.DisplayName;
                WorkItemsContext.MySID = me.SID;
            }
        }

        public Project GetByGuid(string guid)
        {
            return CachedMetaData.Instance.Projects.SingleOrDefault(p => string.Equals(p.Guid, guid, StringComparison.OrdinalIgnoreCase));
        }

        public List<StoredQuery> GetPublicQueries(Project project)
        {
            var list = clientService.GetStoredQueries(project).Where(q => q.IsPublic && !q.IsDeleted).OrderBy(q => q.QueryName).ToList();
            list.ForEach(sq => sq.Collection = this.collection);
            return list;
        }

        public List<StoredQuery> GetMyQueries(Project project)
        {
            var list = clientService.GetStoredQueries(project).Where(q => string.Equals(WorkItemsContext.MySID, q.Owner) && !q.IsDeleted).ToList();
            list.ForEach(sq => sq.Collection = this.collection);
            return list;
        }

        public void UpdateWorkItems(int changeSet, Dictionary<int, WorkItemCheckinAction> workItems, string comment)
        {
            foreach (var workItem in workItems)
            {
                switch (workItem.Value)
                {
                    case WorkItemCheckinAction.Associate:
                        this.clientService.Associate(workItem.Key, changeSet, comment);
                        break;
                    case WorkItemCheckinAction.Resolve:
                        this.clientService.Resolve(workItem.Key, changeSet, comment);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
