//
// CheckInDialog.cs
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
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Client.Enums;
using Microsoft.TeamFoundation.VersionControl.Client.Objects;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.VersionControl.TFS.GUI.WorkItems;
using MonoDevelop.VersionControl.TFS.WorkItemTracking.Structure;
using Xwt;
using MonoDevelop.VersionControl.TFS.VersionControl;
using MonoDevelop.VersionControl.TFS.VersionControl.Models;
using MonoDevelop.VersionControl.TFS.VersionControl.Structure;

namespace MonoDevelop.VersionControl.TFS.GUI.VersionControl.Dialogs
{
    public class CheckInDialog : Dialog
    {
        readonly Notebook notebook = new Notebook();

        readonly ListView filesView = new ListView();
        readonly DataField<bool> isCheckedField = new DataField<bool>();
        readonly DataField<string> nameField = new DataField<string>();
        readonly DataField<string> changesField = new DataField<string>();
        readonly DataField<string> folderField = new DataField<string>();
        readonly DataField<PendingChange> changeField = new DataField<PendingChange>();
        readonly ListStore fileStore;

        readonly TextEntry commentEntry = new TextEntry();

        readonly ListView workItemsView = new ListView();
        readonly DataField<WorkItem> workItemField = new DataField<WorkItem>();
        readonly DataField<int> idField = new DataField<int>();
        readonly DataField<string> titleField = new DataField<string>();
        readonly ListStore workItemsStore;

        public CheckInDialog()
        {
            fileStore = new ListStore(isCheckedField, nameField, changesField, folderField, changeField);
            workItemsStore = new ListStore(idField, titleField, workItemField);
            BuildGui();
        }

        void BuildGui()
        {
            this.Title = GettextCatalog.GetString("Check In files");
            this.Resizable = false;
            notebook.TabOrientation = NotebookTabOrientation.Left;

            var checkInTab = new VBox();
            checkInTab.PackStart(new Label(GettextCatalog.GetString("Pending Changes") + ":"));
            filesView.WidthRequest = 500;
            filesView.HeightRequest = 150;
            var checkView = new CheckBoxCellView(isCheckedField);
            checkView.Editable = true;
            filesView.Columns.Add("Name", checkView, new TextCellView(nameField));
            filesView.Columns.Add("Changes", changesField);
            filesView.Columns.Add("Folder", folderField);
            filesView.DataSource = fileStore;
            checkInTab.PackStart(filesView, true, true);

            checkInTab.PackStart(new Label(GettextCatalog.GetString("Comment") + ":"));
            commentEntry.MultiLine = true;
            checkInTab.PackStart(commentEntry);

            notebook.Add(checkInTab, GettextCatalog.GetString("Pending Changes"));


            var workItemsTab = new HBox();
            var workItemsListBox = new VBox();
            workItemsListBox.PackStart(new Label(GettextCatalog.GetString("Work Items") + ":"));
            workItemsView.Columns.Add("Id", idField);
            workItemsView.Columns.Add("Title", titleField);
            workItemsView.DataSource = workItemsStore;
            workItemsListBox.PackStart(workItemsView, true);
            workItemsTab.PackStart(workItemsListBox, true, true);

            var workItemButtonBox = new VBox();
            var addWorkItemButton = new Button(GettextCatalog.GetString("Add Work Item"));
            addWorkItemButton.Clicked += OnAddWorkItem;
            workItemButtonBox.PackStart(addWorkItemButton);
            var removeWorkItemButton = new Button(GettextCatalog.GetString("Remove Work Item"));
            removeWorkItemButton.Clicked += OnRemoveWorkItem;
            workItemButtonBox.PackStart(removeWorkItemButton);
            workItemsTab.PackStart(workItemButtonBox);

            notebook.Add(workItemsTab, GettextCatalog.GetString("Work Items"));


            this.Buttons.Add(Command.Ok, Command.Cancel);


            this.Content = notebook;
        }

        void OnRemoveWorkItem (object sender, EventArgs e)
        {
            if (workItemsView.SelectedRow > -1)
            {
                workItemsStore.RemoveRow(workItemsView.SelectedRow);
            }
        }

        void OnAddWorkItem (object sender, EventArgs e)
        {
            using (var selectWorkItemDialog = new SelectWorkItemDialog())
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
                    var row = workItemsStore.AddRow();
                    workItemsStore.SetValue(row, workItemField, workItem);
                    workItemsStore.SetValue(row, idField, workItem.Id);
                    workItemsStore.SetValue(row, titleField, title);
                };
                selectWorkItemDialog.Run(Xwt.Toolkit.CurrentEngine.WrapWindow(MessageService.RootWindow));
            }
        }

        private bool IsWorkItemAdded(int workItemId)
        {
            for (int i = 0; i < workItemsStore.RowCount; i++)
            {
                var workItem = workItemsStore.GetValue(i, workItemField);
                if (workItem.Id == workItemId)
                    return true;
            }
            return false;
        }

        private void FillStore(IEnumerable<ExtendedItem> items, IWorkspace workspace)
        {
            fileStore.Clear();
            var pendingChanges = workspace.GetPendingChanges(items);
            foreach (var pendingChange in pendingChanges)
            {
                var row = fileStore.AddRow();
                fileStore.SetValue(row, isCheckedField, true);
                var path = (RepositoryPath)pendingChange.ServerItem;
                fileStore.SetValue(row, nameField, path.ItemName);
                fileStore.SetValue(row, changesField, pendingChange.ChangeType.ToString());
                fileStore.SetValue(row, folderField, path.ParentPath);
                fileStore.SetValue(row, changeField, pendingChange);
            }
        }

        internal List<PendingChange> SelectedChanges
        {
            get
            {
                var items = new List<PendingChange>();
                for (int i = 0; i < fileStore.RowCount; i++)
                {
                    var isChecked = fileStore.GetValue(i, isCheckedField);
                    if (isChecked)
                        items.Add(fileStore.GetValue(i, changeField));
                }
                return items;
            }
        }

        internal Dictionary<int, WorkItemCheckinAction> SelectedWorkItems
        {
            get
            {
                var items = new Dictionary<int, WorkItemCheckinAction>();
                for (int i = 0; i < workItemsStore.RowCount; i++)
                {
                    var workItem = workItemsStore.GetValue(i, workItemField);
                    items.Add(workItem.Id, WorkItemCheckinAction.Associate);
                }
                return items;
            }
        }

        internal string Comment
        {
            get
            {
                return commentEntry.Text;
            }
        }

        internal static void Open(IEnumerable<ExtendedItem> items, IWorkspace workspace)
        {
            using (var dialog = new CheckInDialog())
            {
                dialog.FillStore(items, workspace);
                if (dialog.Run(Toolkit.CurrentEngine.WrapWindow(MessageService.RootWindow)) == Command.Ok)
                {
                    using (var progress = VersionControlService.GetProgressMonitor("CheckIn", VersionControlOperationType.Push))
                    {
                        progress.BeginTask("Check In", 1);
                        var result = workspace.CheckIn(dialog.SelectedChanges, dialog.Comment, dialog.SelectedWorkItems);
                        foreach (var failure in result.Failures.Where(f => f.SeverityType == SeverityType.Error))
                        {
                            progress.ReportError(failure.Code, new Exception(failure.Message));
                        }
                        progress.EndTask();
                        progress.ReportSuccess("Finish Check In");
                    }
                }
            }
        }
    }
}

