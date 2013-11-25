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
using Microsoft.TeamFoundation.WorkItemTracking.Client.Objects;
using System.Linq;

namespace Microsoft.TeamFoundation.WorkItemTracking.Client.Metadata
{
    public class CachedMetaData
    {
        private static CachedMetaData instance;

        public static CachedMetaData Instance
        {
            get
            {
                return instance ?? (instance = new CachedMetaData());
            }
        }

        public void Init(ClientService clientService)
        {
            var hierarchy = clientService.GetHierarchy();
            ExtractProjects(hierarchy);
            this.Fields = new FieldList(clientService.GetFields());
            this.Constants = clientService.GetConstants();
        }

        private void ExtractProjects(List<Hierarchy> hierarchy)
        {
            const int projectType = -42;
            const int iterationType = -44;
            Projects = new List<Project>();

            foreach (var item in hierarchy.Where(h => h.TypeId == projectType && !h.IsDeleted))
            {
                Projects.Add(new Project { Id = item.AreaId, Guid = item.Guid, Name = item.Name });
            }

            Iterations = new List<Iteration>();
            foreach (var item in hierarchy.Where(h => h.TypeId == iterationType && !h.IsDeleted))
            {
                var iteration = new Iteration { Id = item.AreaId, Name = item.Name };
                var item1 = item;
                var parent = hierarchy.Single(h => h.AreaId == item1.ParentId);
                while (parent.TypeId != projectType)
                {
                    parent = hierarchy.Single(h => h.AreaId == parent.ParentId);
                }
                iteration.Project = Projects.Single(p => p.Id == parent.AreaId);
                Iterations.Add(iteration);
            }
        }

        public FieldList Fields { get; set; }

        public List<Constant> Constants { get; set; }

        public List<Project> Projects { get; set; }

        public List<Iteration> Iterations { get; set; }
    }
}

