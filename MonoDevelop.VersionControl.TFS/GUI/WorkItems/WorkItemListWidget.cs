//
// WorkItemListWidget.cs
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
using Microsoft.TeamFoundation.WorkItemTracking.Client.Objects;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client.Metadata;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace MonoDevelop.VersionControl.TFS.GUI.WorkItems
{
    public class WorkItemListWidget : VBox
    {
        private readonly ListView listView = new ListView();
        private DataField<WorkItem> workItemField;

        public WorkItemListWidget()
        {
            this.PackStart(listView, true, true);
            listView.SelectionMode = SelectionMode.Multiple;
            listView.RowActivated += OnWorkItemClicked;
            listView.KeyPressed += OnWorkItemKeyPressed;
        }

        void OnWorkItemKeyPressed (object sender, KeyEventArgs e)
        {
            if (e.Modifiers == ModifierKeys.Control && (e.Key == Key.c || e.Key == Key.C))
            {
                CopySelectedToClipBoard();
            }
        }

        private void CopySelectedToClipBoard()
        {
            var store = (ListStore)listView.DataSource;
            StringBuilder builder = new StringBuilder();
            foreach (var row in listView.SelectedRows)
            {
                List<string> rowValues = new List<string>();
                foreach (var column in listView.Columns)
                {
                    var field = ((TextCellView)column.Views[0]).TextField as IDataField<object>;
                    var val = Convert.ToString(store.GetValue(row, field));
                    rowValues.Add(val);
                }
                builder.AppendLine(string.Join("\t", rowValues));
            }
            Clipboard.SetText(builder.ToString());
        }

        void OnWorkItemClicked(object sender, ListViewRowEventArgs e)
        {
            var store = (ListStore)listView.DataSource;
            var workItem = store.GetValue(e.RowIndex, workItemField);
            if (OnSelectWorkItem != null)
                OnSelectWorkItem(workItem);
        }

        public void LoadQuery(StoredQuery query)
        {
            listView.Columns.Clear();
            using (var progress = new MonoDevelop.Ide.ProgressMonitoring.MessageDialogProgressMonitor(true, false, false))
            {
                var fields = CachedMetaData.Instance.Fields;
                WorkItemStore store = new WorkItemStore(query);
                var data = store.LoadByPage(progress);
                if (data.Count > 0)
                {
                    var firstItem = data[0];
                    List<IDataField> dataFields = new List<IDataField>();
                    var mapping = new Dictionary<Field, IDataField<object>>();
                    foreach (var item in firstItem.WorkItemInfo.Keys)
                    {
                        var field = fields[item];
                        var dataField = new DataField<object>();
                        dataFields.Add(dataField);
                        mapping.Add(field, dataField);
                    }

                    if (dataFields.Any())
                    {
                        workItemField = new DataField<WorkItem>();
                        dataFields.Insert(0, workItemField);
                        var listStore = new ListStore(dataFields.ToArray());
                        foreach (var map in mapping)
                        {
                            listView.Columns.Add(map.Key.Name, map.Value);
                        }
                        listView.DataSource = listStore;
                        foreach (var workItem in data)
                        {
                            var row = listStore.AddRow();
                            listStore.SetValue(row, workItemField, workItem);
                            foreach (var map in mapping)
                            {
                                object value;
                                if (workItem.WorkItemInfo.TryGetValue(map.Key.ReferenceName, out value))
                                {
                                    listStore.SetValue(row, map.Value, value);
                                }
                                else
                                {
                                    listStore.SetValue(row, map.Value, null);
                                }
                            }
                        }
                    }
                }
            }
        }

        public delegate void WorkItemSelectedEventHandler(WorkItem workItem);

        public event WorkItemSelectedEventHandler OnSelectWorkItem;
    }
}
