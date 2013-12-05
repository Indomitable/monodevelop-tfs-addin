//
// SelectWorkItemDialog.cs
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
using Xwt;
using Microsoft.TeamFoundation.WorkItemTracking.Client.Objects;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Linq;
using MonoDevelop.Core;
using System.Collections.Generic;

namespace MonoDevelop.VersionControl.TFS.GUI.WorkItems
{
    public class SelectWorkItemDialog : Dialog
    {
        private readonly TFSRepository repo;
        private readonly TreeView queryView = new TreeView();
        private readonly DataField<string> titleField = new DataField<string>();
        private readonly DataField<StoredQuery> queryField = new DataField<StoredQuery>();
        private readonly TreeStore queryStore;
        private readonly WorkItemListWidget workItemList = new WorkItemListWidget();

        public WorkItemListWidget WorkItemList
        {
            get
            {
                return workItemList;
            }
        }

        public SelectWorkItemDialog(TFSRepository repo)
        {
            this.repo = repo;
            queryStore = new TreeStore(titleField, queryField);
            BuildGui();
        }

        void BuildGui()
        {
            this.Title = GettextCatalog.GetString("Select Work Item");
            VBox content = new VBox();
            HBox mainBox = new HBox();
            queryView.Columns.Add(new ListViewColumn(string.Empty, new TextCellView(titleField)));
            queryView.DataSource = queryStore;
            queryView.WidthRequest = 200;
            BuildQueryView();
            mainBox.PackStart(queryView);

            workItemList.WidthRequest = 400;
            workItemList.HeightRequest = 400;

            mainBox.PackStart(workItemList, true, true);

            content.PackStart(mainBox, true, true);

            HBox buttonBox = new HBox();

            Button okButton = new Button(GettextCatalog.GetString("Ok"));
            okButton.WidthRequest = Constants.ButtonWidth;
            okButton.Clicked += (sender, e) => Respond(Command.Ok);
            buttonBox.PackEnd(okButton);

            content.PackStart(buttonBox);
            //this.Resizable = false;
            this.Content = content;

            AttachEvents();
        }

        void BuildQueryView()
        {
            queryStore.Clear();
            foreach (var server in TFSVersionControlService.Instance.Servers)
            {
                var node = queryStore.AddNode().SetValue(titleField, server.Name);
                foreach (var pc in server.ProjectCollections)
                {
                    node.AddChild().SetValue(titleField, pc.Name);
                    var workItemManager = new WorkItemManager(pc);
                    foreach (var projectInfo in pc.Projects.OrderBy(x => x.Name))
                    {
                        var workItemProject = workItemManager.GetByGuid(projectInfo.Guid);
                        if (workItemProject == null)
                            continue;

                        node.AddChild().SetValue(titleField, projectInfo.Name);

                        var privateQueries = workItemManager.GetMyQueries(workItemProject);
                        if (privateQueries.Any())
                        {
                            node.AddChild().SetValue(titleField, "My Queries");
                            foreach (var query in privateQueries)
                            {
                                node.AddChild().SetValue(titleField, query.QueryName).SetValue(queryField, query);
                                node.MoveToParent();
                            }
                            node.MoveToParent();
                        }
                        var publicQueries = workItemManager.GetPublicQueries(workItemProject);
                        if (publicQueries.Any())
                        {
                            node.AddChild().SetValue(titleField, "Public");
                            foreach (var query in publicQueries)
                            {
                                node.AddChild().SetValue(titleField, query.QueryName).SetValue(queryField, query);
                                node.MoveToParent();
                            }
                            node.MoveToParent();
                        }
                        node.MoveToParent();
                    }
                    queryView.ExpandRow(node.CurrentPosition, true);
                }
            }
            var cursor = queryStore.GetFirstNode();
            if (cursor.MoveToChild()) //Move to Project Collections
                queryView.ExpandToRow(cursor.CurrentPosition);
        }

        void AttachEvents()
        {
            queryView.RowActivated += (sender, e) =>
            {
                var navigator = queryStore.GetNavigatorAt(e.Position);
                var query = navigator.GetValue(queryField);
                if (query != null)
                {
                    workItemList.LoadQuery(query);
                }
            };
        }
    }
}

