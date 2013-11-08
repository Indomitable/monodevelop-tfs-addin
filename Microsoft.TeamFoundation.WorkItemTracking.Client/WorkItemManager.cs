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
using System.Security.Principal;
using System.Net;

namespace Microsoft.TeamFoundation.WorkItemTracking.Client
{
    public class WorkItemManager
    {
        private readonly ProjectCollection collection;
        private readonly ClientService clientService;
        //        private string currentUserSid;
        public WorkItemManager(ProjectCollection collection)
        {
            this.collection = collection;
            this.clientService = collection.GetService<ClientService>();
            Init();
        }

        private void Init()
        {
            Projects = new List<Project>();
            var hierarchy = clientService.GetHierarchy();
            if (hierarchy.Count > 0)
            {
                var top = hierarchy[0];
                foreach (var item in top.Children)
                {
                    var project = new Project
                    {
                        Id = item.AreaId,
                        Name = item.Name,
                        Guid = item.Guid
                    };
                    Projects.Add(project);
                }
            }
//            var credentials = (NetworkCredential)collection.Server.Credentials;
//            NTAccount account = new NTAccount(credentials.Domain, credentials.UserName);
//            SecurityIdentifier s = (SecurityIdentifier)account.Translate(typeof(SecurityIdentifier));
//            currentUserSid = s.ToString();
        }

        public List<Project> Projects { get; set; }

        public Project GetByGuid(string guid)
        {
            return Projects.SingleOrDefault(p => string.Equals(p.Guid, guid, StringComparison.OrdinalIgnoreCase));
        }

        public List<StoredQuery> GetPublicQueries(Project project)
        {
            return clientService.GetStoredQueries(project).Where(q => q.IsPublic && !q.IsDeleted).OrderBy(q => q.QueryName).ToList();
        }

        public List<StoredQuery> GetMyQueries(Project project)
        {
            return new List<StoredQuery>();
            //return clientService.GetStoredQueries(project).Where(q => string.Equals(currentUserSid, q.Owner) && !q.IsDeleted).ToList();
        }
    }
}
