//
// Microsoft.TeamFoundation.WorkItemTracking.Client.ProjectCollection
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;
using System.Web.Services;
using Microsoft.TeamFoundation.WorkItemTracking.Common;

namespace Microsoft.TeamFoundation.WorkItemTracking.Client
{
	public sealed class ProjectCollection : ReadOnlyList
	{
		private SortedList<string, Project> projects = new SortedList<string, Project>();

		public bool Contains(Project project)
		{
			return projects.ContainsKey(project.Name);
		}

		public bool Contains(string projectName)
		{
			return projects.ContainsKey(projectName);
		}

		public Project GetById(int projectId)
		{
			foreach (Project project in projects.Values)
				{
					if (project.Id == projectId) return project;
				}

			return null;
		}

		protected override object GetItem(int index)
		{
			return projects.Values[index] as object;
		}

		public int IndexOf(Project project)
		{
			return projects.IndexOfValue(project);
		}

		public override int Count
		{
			get { return projects.Count; }
		}

		public Project this[int index]
		{
			get { return projects.Values[index]; }
		}

		public Project this[string projectName]
		{
			get { return projects[projectName]; }
		}

	}
}
