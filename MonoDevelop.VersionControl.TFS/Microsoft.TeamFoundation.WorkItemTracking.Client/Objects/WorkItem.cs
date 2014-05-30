//
// Microsoft.TeamFoundation.WorkItemTracking.Client.WorkItem
//
// Authors:
//	Joel Reed (joelwreed@gmail.com)
//
// Copyright (C) 2007 Joel Reed
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.TeamFoundation.WorkItemTracking.Client.Metadata;

namespace Microsoft.TeamFoundation.WorkItemTracking.Client.Objects
{
    public sealed class WorkItem
    {
        public int Id { get { return Convert.ToInt32(WorkItemInfo["System.Id"]); } }

        public Dictionary<string, object> WorkItemInfo { get; set; }

        public int ProjectId
        {
            get
            {
                object val;
                if (this.WorkItemInfo.TryGetValue("System.AreaId", out val))
                {
                    return Convert.ToInt32(val);
                }
                return -1;
            }
        }

        public WorkItemType Type
        {
            get
            {
                object val;
                if (this.WorkItemInfo.TryGetValue("System.WorkItemType", out val))
                {
                    var strType = Convert.ToString(val);
                    var workItems = from wt in CachedMetaData.Instance.WorkItemTypes
                                                   join c in CachedMetaData.Instance.Constants on wt.NameConstantId equals c.Id
                                                   where wt.ProjectId == this.ProjectId && string.Equals(c.Value, strType, StringComparison.OrdinalIgnoreCase)
                                                   select wt;
                    return workItems.SingleOrDefault();
                }
                return null;
            }
        }

        public string GetNextStateForCheckin()
        {
            string fieldName = "System.State";
            string actionName = "Microsoft.VSTS.Actions.Checkin";
            object val;
            var type = this.Type;
            if (type != null && this.WorkItemInfo.TryGetValue(fieldName, out val))
            {
                var currentState = Convert.ToString(val);
                var query = from a in CachedMetaData.Instance.Actions
                                        join fc in CachedMetaData.Instance.Constants on a.FromStateId equals fc.Id
                                        join tc in CachedMetaData.Instance.Constants on a.FromStateId equals tc.Id
                                        where string.Equals(a.Name, actionName, StringComparison.OrdinalIgnoreCase) &&
                                            a.WorkItemTypeId == type.Id && string.Equals(fc.Value, currentState, StringComparison.OrdinalIgnoreCase)
                                        select tc;
                var toConstant = query.SingleOrDefault();
                if (toConstant != null)
                    return toConstant.Value;
            }
            return string.Empty;
        }
    }
}
