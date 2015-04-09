//
// UndoDialog.cs
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
using Microsoft.TeamFoundation.VersionControl.Client.Objects;
using MonoDevelop.Core;
using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Client.Enums;
using MonoDevelop.Ide;
using MonoDevelop.VersionControl.TFS.VersionControl.Structure;
using MonoDevelop.VersionControl.TFS.VersionControl;
using MonoDevelop.VersionControl.TFS.VersionControl.Models;

namespace MonoDevelop.VersionControl.TFS.GUI.VersionControl.Dialogs
{
    public class UndoDialog : Dialog
    {
        readonly ListView fileView = new ListView();
        readonly DataField<bool> isCheckedField = new DataField<bool>();
        readonly DataField<string> nameField = new DataField<string>();
        readonly DataField<string> changesField = new DataField<string>();
        readonly DataField<string> folderField = new DataField<string>();
        readonly DataField<PendingChange> changeField = new DataField<PendingChange>();
        readonly ListStore fileStore;

        public UndoDialog()
        {
            fileStore = new ListStore(isCheckedField, nameField, changesField, folderField, changeField);
            BuildGui();
        }

        void BuildGui()
        {
            this.Title = GettextCatalog.GetString("Undo changes");
            this.Resizable = false;
            var content = new VBox();
            content.PackStart(new Label(GettextCatalog.GetString("Files") + ":"));
            fileView.WidthRequest = 500;
            fileView.HeightRequest = 150;
            var checkView = new CheckBoxCellView(isCheckedField);
            checkView.Editable = true;
            fileView.Columns.Add("Name", checkView, new TextCellView(nameField));
            fileView.Columns.Add("Changes", changesField);
            fileView.Columns.Add("Folder", folderField);
            fileView.DataSource = fileStore;
            content.PackStart(fileView, true, true);

            this.Buttons.Add(Command.Ok, Command.Cancel);

            this.Content = content;
        }

        private void FillStore(List<ExtendedItem> items, IWorkspace workspace)
        {
            fileStore.Clear();
            var pendingChanges = workspace.GetPendingChanges(items, false);
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

        internal List<PendingChange> SelectedItems
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

        internal static void Open(List<ExtendedItem> items, IWorkspace workspace)
        {
            using (var dialog = new UndoDialog())
            {
                dialog.FillStore(items, workspace);
                if (dialog.Run(Toolkit.CurrentEngine.WrapWindow(MessageService.RootWindow)) == Command.Ok)
                {
                    var changesToUndo = dialog.SelectedItems;
                    using (var progress = VersionControlService.GetProgressMonitor("Undo", VersionControlOperationType.Pull))
                    {
                        progress.BeginTask("Undo", changesToUndo.Count);
                        var itemSpecs = changesToUndo.Select(change => new ItemSpec(change.LocalItem, change.ItemType == ItemType.File ? RecursionType.None : RecursionType.Full));
                        workspace.Undo(itemSpecs);
                        progress.EndTask();
                        progress.ReportSuccess("Finish Undo");
                    }
                }
            }
        }
    }
}
