//
// TFSCommitDialogExtensionWidget.cs
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
using Xwt;
using MonoDevelop.Core;
using Microsoft.TeamFoundation.VersionControl.Client.Enums;

namespace MonoDevelop.VersionControl.TFS.GUI
{
    public class TFSCommitDialogExtensionWidgetXwt : VBox
    {
        readonly ListView workItemList = new ListView();
        readonly DataField<int> idField = new DataField<int>();
        readonly DataField<string> titleField = new DataField<string>();
        readonly DataField<WorkItemCheckinAction> actionField = new DataField<WorkItemCheckinAction>();
        readonly ListStore listStore;
        readonly Button buttonRemoveWorkItem;

        public TFSCommitDialogExtensionWidgetXwt()
        {
            buttonRemoveWorkItem = new Button(GettextCatalog.GetString("Remove WorkItem"));
            listStore = new ListStore(idField, titleField, actionField);
            BuildGui();
        }

        void BuildGui()
        {
            HBox workItemBox = new HBox();
            workItemList.Columns.Add("ID", idField);
            workItemList.Columns.Add("Title", titleField);
            var comboBoxCellView = new ComboBoxCellView();

            workItemList.Columns.Add("Action", comboBoxCellView);
            workItemList.DataSource = listStore;

            workItemBox.PackStart(workItemList, true);

            VBox buttonBox = new VBox();
            Button buttonAddWorkItem = new Button(GettextCatalog.GetString("Add WorkItem"));

            buttonAddWorkItem.WidthRequest = buttonRemoveWorkItem.WidthRequest = 150;
            buttonRemoveWorkItem.Sensitive = false;
            buttonRemoveWorkItem.Clicked += (sender, e) =>
            {
                if (workItemList.SelectedRow > -1)
                {
                    listStore.RemoveRow(workItemList.SelectedRow);
                    buttonRemoveWorkItem.Sensitive = listStore.RowCount > 0;
                }
            };

            buttonAddWorkItem.Clicked += SelectWorkItem;
            buttonBox.PackStart(buttonAddWorkItem);
            buttonBox.PackStart(buttonRemoveWorkItem);

            workItemBox.PackStart(buttonBox);

            this.PackStart(workItemBox);
        }

        void SelectWorkItem(object sender, EventArgs e)
        {
            using (var selectWorkItemDialog = new SelectWorkItemDialog())
            {
                selectWorkItemDialog.WorkItemList.OnSelectWorkItem += (workItem) =>
                {
                    var row = listStore.AddRow();
                    listStore.SetValue(row, idField, workItem.Id);
                    if (workItem.WorkItemInfo.ContainsKey("System.Title"))
                    {
                        listStore.SetValue(row, titleField, Convert.ToString(workItem.WorkItemInfo["System.Title"]));
                    }
                    buttonRemoveWorkItem.Sensitive = listStore.RowCount > 0;
                };
                selectWorkItemDialog.Run(this.ParentWindow);
            }
        }
    }
}

