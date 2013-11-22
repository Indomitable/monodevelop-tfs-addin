//
// WorkItemsView.cs
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
using MonoDevelop.Ide.Gui;
using Xwt;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using Microsoft.TeamFoundation.WorkItemTracking.Client.Objects;

namespace MonoDevelop.VersionControl.TFS.GUI
{
    public class WorkItemsView : AbstractXwtViewContent
    {
        private readonly WorkItemListWidget widget = new WorkItemListWidget();

        public WorkItemsView()
        {
            this.ContentName = GettextCatalog.GetString("Work Items");
        }

        public override void Load(string fileName)
        {
            throw new NotImplementedException();
        }

        private void Load(StoredQuery query)
        {
            this.ContentName = GettextCatalog.GetString("Work Items: " + query.QueryName);
            widget.LoadQueryByPage(query);
        }

        public override Widget Widget
        {
            get
            {
                return widget;
            }
        }

        public static void Open(StoredQuery query)
        {
            foreach (var view in IdeApp.Workbench.Documents)
            {
                var workView = view.GetContent<WorkItemsView>();
                if (workView != null)
                {
                    workView.Load(query);
                    view.Window.SelectWindow();
                    return;
                }
            }

            WorkItemsView workItemsView = new WorkItemsView();
            workItemsView.Load(query);
            IdeApp.Workbench.OpenDocument(workItemsView, true);
        }
    }
}

