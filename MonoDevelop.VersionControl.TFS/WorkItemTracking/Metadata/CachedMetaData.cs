//
// CachedMetaData.cs
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
using MonoDevelop.VersionControl.TFS.Core.Structure;
using MonoDevelop.VersionControl.TFS.WorkItemTracking.Structure;

namespace MonoDevelop.VersionControl.TFS.WorkItemTracking.Metadata
{
    internal sealed class CachedMetaData
    {
        private static CachedMetaData instance;

        public static CachedMetaData Instance
        {
            get
            {
                return instance ?? (instance = new CachedMetaData());
            }
        }

        public void Init(ProjectCollection collection)
        {
            var hierarchy = collection.GetHierarchy();
            ExtractProjects(hierarchy);
            this.Fields = new FieldList(collection.GetFields());
            this.Constants = collection.GetConstants();
            this.WorkItemTypes = collection.GetWorkItemTypes();
            this.Actions = collection.GetActions();
        }

        private void ExtractProjects(List<Hierarchy> hierarchy)
        {
            const int projectType = -42;

            Projects = new List<WorkItemProject>();

            foreach (var item in hierarchy.Where(h => h.TypeId == projectType && !h.IsDeleted))
            {
                Projects.Add(new WorkItemProject { Id = item.AreaId, Guid = Guid.Parse(item.Guid), Name = item.Name });
            }
            Iterations = new IterationList(Projects);
            Iterations.Build(hierarchy);
        }

        public FieldList Fields { get; set; }

        public List<Constant> Constants { get; set; }

        public List<WorkItemProject> Projects { get; set; }

        internal IterationList Iterations { get; set; }

        public List<WorkItemType> WorkItemTypes { get; set; }

        public List<MonoDevelop.VersionControl.TFS.WorkItemTracking.Structure.Action> Actions { get; set; }
    }
}

