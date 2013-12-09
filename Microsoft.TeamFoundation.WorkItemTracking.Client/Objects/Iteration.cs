//
// Iteration.cs
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
using System.Linq;
using System.Runtime.Remoting.Proxies;

namespace Microsoft.TeamFoundation.WorkItemTracking.Client.Objects
{
    internal interface IIteration
    {
        int Id { get; set; }

        string Name { get; set; }

        List<SubIteration> Children { get; set; }
    }
   

    internal class MainIteration : IIteration
    {
        public MainIteration()
        {
            Children = new List<SubIteration>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public Project Project { get; set; }

        public List<SubIteration> Children { get; set; }
    }

    internal class SubIteration : IIteration
    {
        public SubIteration()
        {
            Children = new List<SubIteration>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public List<SubIteration> Children { get; set; }
    }

    internal class IterationList : List<IIteration>
    {
        const int IterationParentType = -43;
        readonly List<Project> projects;

        public IterationList(List<Project> projects)
        {
            this.projects = projects;
        }


        public void Build(List<Hierarchy> hierarchy)
        {
            var projectIterationsNodes = hierarchy.Where(h => !h.IsDeleted && h.TypeId == IterationParentType).ToArray();
            foreach (var projectIterationNode in projectIterationsNodes)
            {
                var projectIterationNode1 = projectIterationNode;
                var mainIterationNodes = hierarchy.Where(h => !h.IsDeleted && h.ParentId == projectIterationNode1.AreaId).ToArray();
                foreach (var mainIterationNode in mainIterationNodes)
                {
                    var mainIteration = new MainIteration();
                    mainIteration.Id = mainIterationNode.AreaId;
                    mainIteration.Name = mainIterationNode.Name;
                    mainIteration.Project = projects.Single(p => p.Id == projectIterationNode1.ParentId);
                    this.Add(mainIteration);
                    this.BuildSubIterations(hierarchy, mainIteration);
                }
            }
        }

        private void BuildSubIterations(List<Hierarchy> hierarchy, IIteration iteration)
        {
            foreach (var subIterationNode in hierarchy.Where(h => !h.IsDeleted && h.ParentId == iteration.Id))
            {
                var subIteration = new SubIteration();
                subIteration.Id = subIterationNode.AreaId;
                subIteration.Name = subIterationNode.Name;
                iteration.Children.Add(subIteration);
                this.Add(subIteration);
                this.BuildSubIterations(hierarchy, subIteration);
            }
        }

        public IIteration LocateIteration(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;
            var pathParts = path.Split(new [] { '\\' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (pathParts.Length < 2)
                return null;
            var projectName = pathParts[0];
            var iterationName = pathParts[1];
            var mainIteration = this.OfType<MainIteration>().Single(i => string.Equals(i.Project.Name, projectName, System.StringComparison.OrdinalIgnoreCase) && 
                                                                         string.Equals(i.Name, iterationName, System.StringComparison.OrdinalIgnoreCase));
            if (pathParts.Length == 2) //Only 1 level
                return mainIteration;
            else
            {
                List<SubIteration> iterations = mainIteration.Children;
                for (int i = 2; i < pathParts.Length; i++)
                {
                    var currentPart = pathParts[i];

                    foreach (var iteration in iterations)
                    {
                        if (string.Equals(iteration.Name, currentPart, System.StringComparison.OrdinalIgnoreCase))
                        {
                            if (i < pathParts.Length -1)
                            {
                                iterations = iteration.Children;
                                break;
                            }
                            else
                                return iteration;
                        }
                    }
                }
                return null;
            }
        }
    }
}

