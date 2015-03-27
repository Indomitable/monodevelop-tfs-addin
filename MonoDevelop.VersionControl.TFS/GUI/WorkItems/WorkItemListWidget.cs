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
using System.Linq;
using System.Collections.Generic;
using System.Text;
using MonoDevelop.VersionControl.TFS.WorkItemTracking.Structure;
using MonoDevelop.VersionControl.TFS.WorkItemTracking.Metadata;
using MonoDevelop.VersionControl.TFS.WorkItemTracking;
using MonoDevelop.VersionControl.TFS.Core.Structure;

namespace MonoDevelop.VersionControl.TFS.GUI.WorkItems
{
    public class WorkItemListWidget : VBox
    {
        private readonly TreeView listView = new TreeView();
        private DataField<bool> isCheckedField;
        private DataField<WorkItem> workItemField;

        internal WorkItemListWidget()
        {
            this.PackStart(listView, true, true);
            listView.SelectionMode = SelectionMode.Multiple;
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
            var store = (TreeStore)listView.DataSource;
            StringBuilder builder = new StringBuilder();
            foreach (var row in listView.SelectedRows)
            {
                List<string> rowValues = new List<string>();
                foreach (var column in listView.Columns)
                {
                    var field = ((TextCellView)column.Views[0]).TextField as IDataField<object>;
                    var val = Convert.ToString(store.GetNavigatorAt(row).GetValue(field));
                    rowValues.Add(val);
                }
                builder.AppendLine(string.Join("\t", rowValues));
            }
            Clipboard.SetText(builder.ToString());
        }

        internal void LoadQuery(StoredQuery query, ProjectCollection collection)
        {
            listView.Columns.Clear();
            using (var progress = new MonoDevelop.Ide.ProgressMonitoring.MessageDialogProgressMonitor(true, false, false))
            {
                var fields = CachedMetaData.Instance.Fields;
                WorkItemStore store = new WorkItemStore(query, collection);
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
                        if (ShowCheckboxes)
                        {
                            isCheckedField = new DataField<bool>();
                            dataFields.Insert(0, isCheckedField);
                            var checkColumn = new CheckBoxCellView(isCheckedField) { Editable = true };
                            checkColumn.Toggled += (sender, e) => 
                            {
                                var astore = (TreeStore)listView.DataSource;
                                var node = astore.GetNavigatorAt(listView.CurrentEventRow);
                                var workItem = node.GetValue(workItemField);
                                if (!node.GetValue(isCheckedField))
                                {
                                    if (OnSelectWorkItem != null)
                                        OnSelectWorkItem(workItem);
                                }
                                else
                                {
                                    if (OnRemoveWorkItem != null)
                                        OnRemoveWorkItem(workItem);
                                }
                            };
                            listView.Columns.Add("", checkColumn);
                        }
                        workItemField = new DataField<WorkItem>();
                        dataFields.Insert(0, workItemField);
                        var listStore = new TreeStore(dataFields.ToArray());
                        foreach (var map in mapping)
                        {
                            listView.Columns.Add(map.Key.Name, map.Value);
                        }
                        listView.DataSource = listStore;
                        foreach (var workItem in data)
                        {
                            var row = listStore.AddNode();
                            row.SetValue(workItemField, workItem);
                            foreach (var map in mapping)
                            {
                                object value;
                                if (workItem.WorkItemInfo.TryGetValue(map.Key.ReferenceName, out value))
                                {
                                    row.SetValue(map.Value, value);
                                }
                                else
                                {
                                    row.SetValue(map.Value, null);
                                }
                            }
                        }
                    }
                }
            }
        }

        internal delegate void WorkItemSelectedEventHandler(WorkItem workItem);

        internal event WorkItemSelectedEventHandler OnSelectWorkItem;

        internal event WorkItemSelectedEventHandler OnRemoveWorkItem;

        public bool ShowCheckboxes { get; set; }
    }
}
