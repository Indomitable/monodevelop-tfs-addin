// CheckOutDialog.cs
// 
// Author:
//       Ventsislav Mladenov
// 
// The MIT License (MIT)
// 
// Copyright (c) 2013-2015 Ventsislav Mladenov
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
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.VersionControl.TFS.Helpers;
using MonoDevelop.VersionControl.TFS.VersionControl.Enums;
using MonoDevelop.VersionControl.TFS.VersionControl.Models;
using Xwt;

namespace MonoDevelop.VersionControl.TFS.GUI.VersionControl.Dialogs
{
    public class CheckOutDialog : Dialog
    {
        readonly ListView fileView = new ListView();
        readonly DataField<bool> isCheckedField = new DataField<bool>();
        readonly DataField<string> nameField = new DataField<string>();
        readonly DataField<string> folderField = new DataField<string>();
        readonly DataField<ExtendedItem> itemField = new DataField<ExtendedItem>();
        readonly ListStore fileStore;
        readonly ComboBox lockLevelBox = GuiHelper.GetLockLevelComboBox();

        public CheckOutDialog()
        {
            fileStore = new ListStore(isCheckedField, nameField, folderField, itemField);
            BuildGui();
        }

        void BuildGui()
        {
            this.Title = GettextCatalog.GetString("Check Out");
            this.Resizable = false;
            var content = new VBox();
            content.PackStart(new Label(GettextCatalog.GetString("Files") + ":"));
            fileView.WidthRequest = 500;
            fileView.HeightRequest = 150;
            var checkView = new CheckBoxCellView(isCheckedField);
            checkView.Editable = true;
            fileView.Columns.Add("Name", checkView, new TextCellView(nameField));
            fileView.Columns.Add("Folder", folderField);
            fileView.DataSource = fileStore;
            content.PackStart(fileView, true, true);

            var lockBox = new HBox();
            lockBox.PackStart(new Label(GettextCatalog.GetString("Select lock type") + ":"));
            lockBox.PackStart(lockLevelBox, true, true);
            content.PackStart(lockBox);

            this.Buttons.Add(Command.Ok, Command.Cancel);

            this.Content = content;
        }

        private void FillStore(List<ExtendedItem> items)
        {
            fileStore.Clear();
            foreach (var item in items)
            {
                var row = fileStore.AddRow();
                fileStore.SetValue(row, isCheckedField, true);
                fileStore.SetValue(row, nameField, item.ServerPath.ItemName);
                fileStore.SetValue(row, folderField, item.ServerPath.ParentPath);
                fileStore.SetValue(row, itemField, item);
            }
        }

        internal List<ExtendedItem> SelectedItems
        {
            get
            {
                var items = new List<ExtendedItem>();
                for (int i = 0; i < fileStore.RowCount; i++)
                {
                    var isChecked = fileStore.GetValue(i, isCheckedField);
                    if (isChecked)
                        items.Add(fileStore.GetValue(i, itemField));
                }
                return items;
            }
        }

        internal LockLevel LockLevel
        {
            get
            {
                return (LockLevel)lockLevelBox.SelectedItem;
            }
        }

        internal static void Open(List<ExtendedItem> items, TFS.VersionControl.IWorkspace workspace)
        {
            using (var dialog = new CheckOutDialog())
            {
                dialog.FillStore(items);
                if (dialog.Run(Xwt.Toolkit.CurrentEngine.WrapWindow(MessageService.RootWindow)) == Command.Ok)
                {
                    var itemsToCheckOut = dialog.SelectedItems;
                    using (var progress = VersionControlService.GetProgressMonitor("Check Out", VersionControlOperationType.Pull))
                    {
                        progress.BeginTask("Check Out", itemsToCheckOut.Count);
                        foreach (var item in itemsToCheckOut)
                        {
                            var path = item.IsInWorkspace ? item.LocalPath : workspace.Data.GetLocalPathForServerPath(item.ServerPath);
                            workspace.Get(new GetRequest(item.ServerPath, RecursionType.Full, VersionSpec.Latest), GetOptions.None);
                            progress.Log.WriteLine("Check out item: " + item.ServerPath);
                            ICollection<Failure> failures;
                            workspace.PendEdit(path.ToEnumerable(), RecursionType.Full, dialog.LockLevel, out failures);
                            if (failures != null && failures.Any())
                            {
                                if (failures.Any(f => f.SeverityType == SeverityType.Error))
                                {
                                    foreach (var failure in failures.Where(f => f.SeverityType == SeverityType.Error))
                                    {
                                        progress.ReportError(failure.Code, new Exception(failure.Message));
                                    }
                                    break;
                                }
                                foreach (var failure in failures.Where(f => f.SeverityType == SeverityType.Warning))
                                {
                                    progress.ReportWarning(failure.Message);
                                }
                            }
                        }
                        progress.EndTask();
                        progress.ReportSuccess("Finish Check Out.");
                    }
                }
            }
        }
    }
}

