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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoDevelop.VersionControl.TFS.Core.Structure;
using MonoDevelop.VersionControl.TFS.WorkItemTracking.Metadata;
using MonoDevelop.VersionControl.TFS.WorkItemTracking.Structure;

namespace MonoDevelop.VersionControl.TFS.WorkItemTracking
{
    sealed class WorkItemManager
    {
        private readonly ProjectCollection collection;

        public WorkItemManager(ProjectCollection collection)
        {
            this.collection = collection;
            Init();
        }

        private void Init()
        {
            CachedMetaData.Instance.Init(this.collection);
            var constants = CachedMetaData.Instance.Constants;
            var userNameBuilder = new StringBuilder();
            //var server = this.collection.Server;
//            if (server != null && !string.IsNullOrEmpty(server.Credentials.Domain))
//            {
//                userNameBuilder.Append(server.Credentials.Domain + "\\");
//            }
            userNameBuilder.Append(this.collection.Server.UserName);
            var userName = userNameBuilder.ToString();
            var me = constants.FirstOrDefault(c => string.Equals(c.Value, userName, StringComparison.OrdinalIgnoreCase));
            if (me != null)
            {
                WorkItemsContext.WhoAmI = me.DisplayName;
                WorkItemsContext.MySID = me.SID;
            }
        }

        public WorkItemProject GetByGuid(Guid guid)
        {
            return CachedMetaData.Instance.Projects.SingleOrDefault(p => p.Guid == guid);
        }

        public List<StoredQuery> GetPublicQueries(WorkItemProject project)
        {
            return this.collection.GetStoredQueries(project).Where(q => q.IsPublic && !q.IsDeleted).OrderBy(q => q.QueryName).ToList();
        }

        public List<StoredQuery> GetMyQueries(WorkItemProject project)
        {
            return this.collection.GetStoredQueries(project).Where(q => string.Equals(WorkItemsContext.MySID, q.Owner) && !q.IsDeleted).ToList();
        }

        public void UpdateWorkItems(int changeSet, Dictionary<int, WorkItemCheckinAction> workItems, string comment)
        {
            foreach (var workItem in workItems)
            {
                switch (workItem.Value)
                {
                    case WorkItemCheckinAction.Associate:
                        this.collection.AssociateWorkItemWithChangeset(workItem.Key, changeSet, comment);
                        break;
                    case WorkItemCheckinAction.Resolve:
                        this.collection.ResolveWorkItemWithChangeset(workItem.Key, changeSet, comment);
                        break;
                }
            }
        }
    }
}
