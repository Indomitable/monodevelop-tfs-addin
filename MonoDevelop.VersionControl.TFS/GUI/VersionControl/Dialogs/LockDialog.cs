//
// LockDialog.cs
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
using MonoDevelop.VersionControl.TFS.VersionControl;
using Xwt;

namespace MonoDevelop.VersionControl.TFS.GUI.VersionControl.Dialogs
{
    public class LockDialog : Dialog
    {
        readonly ListView fileView = new ListView();
        readonly DataField<bool> isCheckedField = new DataField<bool>();
        readonly DataField<string> nameField = new DataField<string>();
        readonly DataField<string> folderField = new DataField<string>();
        readonly DataField<ExtendedItem> itemField = new DataField<ExtendedItem>();
        readonly ListStore fileStore;
        readonly ComboBox lockLevelBox = GuiHelper.GetLockLevelComboBox(true);

        public LockDialog()
        {
            fileStore = new ListStore(isCheckedField, nameField, folderField, itemField);
            BuildGui();
        }

        void BuildGui()
        {
            this.Title = GettextCatalog.GetString("Lock Files");
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
                var checkOutLockLevel = (CheckOutLockLevel)lockLevelBox.SelectedItem;
                if (checkOutLockLevel == CheckOutLockLevel.CheckOut)
                    return LockLevel.CheckOut;
                else
                    return LockLevel.Checkin;
            }
        }

        internal static void Open(List<ExtendedItem> items, Workspace workspace)
        {
            using (var dialog = new LockDialog())
            {
                dialog.FillStore(items);
                if (dialog.Run(Toolkit.CurrentEngine.WrapWindow(MessageService.RootWindow)) == Command.Ok)
                {
                    var itemsToLock = dialog.SelectedItems;
                    var lockLevel = dialog.LockLevel;

                    using (var progress = VersionControlService.GetProgressMonitor("Lock Files", VersionControlOperationType.Pull))
                    {
                        progress.BeginTask("Lock Files", itemsToLock.Count);
                        var folders = new List<string>(itemsToLock.Where(i => i.ItemType == ItemType.Folder).Select(i => (string)i.ServerPath));
                        var files = new List<string>(itemsToLock.Where(i => i.ItemType == ItemType.File).Select(i => (string)i.ServerPath));
                        workspace.LockFolders(folders, lockLevel);
                        workspace.LockFiles(files, lockLevel);
                        progress.EndTask();
                        progress.ReportSuccess("Finish locking.");
                    }
                }
            }
        }
    }
}

