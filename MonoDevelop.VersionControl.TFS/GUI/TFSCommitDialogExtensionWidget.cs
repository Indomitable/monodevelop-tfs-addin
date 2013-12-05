//
// TFSCommitDialogExtensionWidgetGtk.cs
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
using Gtk;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using System.Collections.Generic;
using Microsoft.TeamFoundation.WorkItemTracking.Client.Enums;
using MonoDevelop.VersionControl.TFS.GUI.WorkItems;

namespace MonoDevelop.VersionControl.TFS.GUI
{
    public class TFSCommitDialogExtensionWidget : HBox
    {
        readonly TFSRepository repo;
        readonly TreeView workItemsView = new TreeView();
        readonly ListStore workItemStore = new ListStore(typeof(int), typeof(string), typeof(string));
        readonly ListStore checkinActions = new ListStore(typeof(string));
        readonly Button removeButton = new Button();

        public TFSCommitDialogExtensionWidget(TFSRepository repo)
        {
            this.repo = repo;
            BuildGui();
        }

        void BuildGui()
        {
            CellRendererText cellId = new CellRendererText();
            TreeViewColumn idColumn = new TreeViewColumn();
            idColumn.Title = GettextCatalog.GetString("ID");
            idColumn.PackStart(cellId, false);
            idColumn.AddAttribute(cellId, "text", 0);

            CellRendererText cellTitle = new CellRendererText();
            TreeViewColumn titleColumn = new TreeViewColumn();
            titleColumn.Title = "Title";
            titleColumn.Expand = true;
            titleColumn.Sizing = TreeViewColumnSizing.Fixed;
            titleColumn.PackStart(cellTitle, true);
            titleColumn.AddAttribute(cellTitle, "text", 1);

            CellRendererCombo cellAction = new CellRendererCombo();
            TreeViewColumn actionColumn = new TreeViewColumn();
            actionColumn.Title = "Action";
            actionColumn.PackStart(cellAction, false);
            actionColumn.AddAttribute(cellAction, "text", 2);
            cellAction.Editable = true;
            cellAction.Model = checkinActions;
            cellAction.TextColumn = 0;
            cellAction.HasEntry = false;
            cellAction.Edited += OnActionChanged;
            //checkinActions.AppendValues(WorkItemCheckinAction.None.ToString());
            checkinActions.AppendValues(WorkItemCheckinAction.Associate.ToString());
            //checkinActions.AppendValues(WorkItemCheckinAction.Resolve.ToString());

            workItemsView.AppendColumn(idColumn);
            workItemsView.AppendColumn(titleColumn);
            workItemsView.AppendColumn(actionColumn);

            workItemsView.Model = workItemStore;
            workItemsView.WidthRequest = 300;
            workItemsView.HeightRequest = 120;

            this.PackStart(workItemsView, true, true, 3);

            VButtonBox buttonBox = new VButtonBox();
            Button addButton = new Button();
            addButton.Label = GettextCatalog.GetString("Add Work Item");
            addButton.Clicked += OnAddWorkItem;
            removeButton.Label = GettextCatalog.GetString("Remove Work Item");
            removeButton.Sensitive = false;
            removeButton.Clicked += OnRemoveWorkItem;

            addButton.WidthRequest = removeButton.WidthRequest = 150;

            buttonBox.PackStart(addButton);
            buttonBox.PackStart(removeButton);
            buttonBox.Layout = ButtonBoxStyle.Start;

            this.PackStart(buttonBox, false, false, 3);

            this.ShowAll();
        }

        void OnRemoveWorkItem(object sender, EventArgs e)
        {
            TreeSelection selection = workItemsView.Selection;
            TreeIter iter;
            if (!selection.GetSelected(out iter))
            {
                return;
            }
            workItemStore.Remove(ref iter);
            removeButton.Sensitive = workItemStore.IterNChildren() > 0;
        }

        void OnAddWorkItem(object sender, EventArgs e)
        {
            using (var selectWorkItemDialog = new SelectWorkItemDialog(repo))
            {
                selectWorkItemDialog.WorkItemList.OnSelectWorkItem += (workItem) =>
                {
                    if (IsWorkItemAdded(workItem.Id))
                        return;
                    string title = string.Empty;
                    if (workItem.WorkItemInfo.ContainsKey("System.Title"))
                    {
                        title = Convert.ToString(workItem.WorkItemInfo["System.Title"]);
                    }
                    workItemStore.AppendValues(workItem.Id, title, "Associate");
                    removeButton.Sensitive = true;
                };
                selectWorkItemDialog.Run(Xwt.Toolkit.CurrentEngine.WrapWindow(MessageService.RootWindow));
            }
        }

        private bool IsWorkItemAdded(int workItemId)
        {
            TreeIter iter;
            if (workItemStore.GetIterFirst(out iter))
            {
                var id = (int)workItemStore.GetValue(iter, 0);
                if (id == workItemId)
                    return true;
                while (workItemStore.IterNext(ref iter))
                {
                    var idNext = (int)workItemStore.GetValue(iter, 0);
                    if (idNext == workItemId)
                        return true;
                }
            }
            return false;
        }

        void OnActionChanged(object o, EditedArgs args)
        {
            TreeSelection selection = workItemsView.Selection;
            TreeIter iter;
            if (!selection.GetSelected(out iter))
            {
                return;
            }
            workItemStore.SetValue(iter, 2, args.NewText);
        }

        private KeyValuePair<int, WorkItemCheckinAction> GetValue(TreeIter iter)
        {
            var id = (int)workItemStore.GetValue(iter, 0);
            var checkinAction = (WorkItemCheckinAction)Enum.Parse(typeof(WorkItemCheckinAction), (string)workItemStore.GetValue(iter, 2));
            return new KeyValuePair<int, WorkItemCheckinAction>(id, checkinAction);
        }

        public Dictionary<int, WorkItemCheckinAction> WorkItems
        {
            get
            {
                var workItems = new Dictionary<int, WorkItemCheckinAction>();
                TreeIter iter;
                if (workItemStore.GetIterFirst(out iter))
                {
                    var value = GetValue(iter);
                    workItems.Add(value.Key, value.Value);

                    while (workItemStore.IterNext(ref iter))
                    {
                        var valueNext = GetValue(iter);
                        workItems.Add(valueNext.Key, valueNext.Value);
                    }
                }
                return workItems;
            }
        }
    }
}

