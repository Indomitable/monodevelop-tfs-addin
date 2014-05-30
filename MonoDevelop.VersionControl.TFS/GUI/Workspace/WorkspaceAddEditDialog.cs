//
// WorkspaceAddEditDialog.cs
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
using Microsoft.TeamFoundation.VersionControl.Client;
using MonoDevelop.Ide;

namespace MonoDevelop.VersionControl.TFS.GUI.Workspace
{
    public class WorkspaceAddEditDialog : Dialog
    {
        private readonly DialogAction _action;
        private readonly Microsoft.TeamFoundation.VersionControl.Client.Workspace _workspace;
        private readonly TextEntry _nameEntry = new TextEntry();
        private readonly TextEntry _ownerEntry = new TextEntry();
        private readonly TextEntry _computerEntry = new TextEntry();
        //private readonly ComboBox _permissions = new ComboBox();
        private readonly TextEntry _commentEntry = new TextEntry();
        private readonly ListView _workingFoldersView = new ListView();
        private readonly DataField<string> _tfsFolder = new DataField<string>();
        private readonly DataField<string> _localFolder = new DataField<string>();
        private readonly ListStore _workingFoldersStore;
        private readonly Microsoft.TeamFoundation.Client.ProjectCollection projectCollection;

        public WorkspaceAddEditDialog(Microsoft.TeamFoundation.VersionControl.Client.Workspace workspace, Microsoft.TeamFoundation.Client.ProjectCollection projectCollection)
        {
            this.projectCollection = projectCollection;
            if (workspace == null)
            {
                _action = DialogAction.Create;
            }
            else
            {
                this._workspace = workspace;
                _action = DialogAction.Edit;
            }
            _workingFoldersStore = new ListStore(_tfsFolder, _localFolder);
            BuildGui();
        }

        private void BuildGui()
        {
            this.Resizable = false;
            _nameEntry.WidthRequest = _ownerEntry.WidthRequest = _computerEntry.WidthRequest = 400;

            VBox content = new VBox();
            Table entryTable = new Table();
            entryTable.Add(new Label(GettextCatalog.GetString("Name") + ":"), 0, 0); 
            entryTable.Add(_nameEntry, 1, 0);

            entryTable.Add(new Label(GettextCatalog.GetString("Owner") + ":"), 0, 1); 
            entryTable.Add(_ownerEntry, 1, 1);

            entryTable.Add(new Label(GettextCatalog.GetString("Computer") + ":"), 0, 2); 
            entryTable.Add(_computerEntry, 1, 2);

//            entryTable.Add(new Label(GettextCatalog.GetString("Permissions") + ":"), 0, 3); 
//            _permissions.Items.Add(0, "Private workspace");
//            _permissions.Items.Add(1, "Public workspace (limited)");
//            _permissions.Items.Add(2, "Public workspace");
//            entryTable.Add(_permissions, 1, 3);

            content.PackStart(entryTable);

            content.PackStart(new Label(GettextCatalog.GetString("Comment") + ":"));
            _commentEntry.MultiLine = true; //Not working yet
            content.PackStart(_commentEntry);

            content.PackStart(new Label(GettextCatalog.GetString("Working folders") + ":"));
            _workingFoldersView.DataSource = _workingFoldersStore;
            _workingFoldersView.MinHeight = 150;
            _workingFoldersView.MinWidth = 300;

            var tfsFolderView = new TextCellView(_tfsFolder);
            tfsFolderView.Editable = true;

            var localFolderView = new TextCellView(_localFolder);

            _workingFoldersView.Columns.Add(new ListViewColumn("Source Control Floder", tfsFolderView));
            _workingFoldersView.Columns.Add(new ListViewColumn("Local Floder", localFolderView));

            content.PackStart(_workingFoldersView);

            HBox buttonBox = new HBox();

            Button addButton = new Button(GettextCatalog.GetString("Add"));
            addButton.MinWidth = Constants.ButtonWidth;
            addButton.Clicked += OnAddWorkingFolder;

            Button removeButton = new Button(GettextCatalog.GetString("Remove"));
            removeButton.MinWidth = Constants.ButtonWidth;
            removeButton.Clicked += OnRemoveWorkingFolder;

            Button okButton = new Button(GettextCatalog.GetString("OK"));
            okButton.MinWidth = Constants.ButtonWidth;
            okButton.Clicked += OnAddEditWorkspace;

            Button cancelButton = new Button(GettextCatalog.GetString("Cancel"));
            cancelButton.MinWidth = Constants.ButtonWidth;
            cancelButton.Clicked += (sender, e) => Respond(Command.Cancel);

            buttonBox.PackStart(addButton);
            buttonBox.PackStart(removeButton);
            buttonBox.PackEnd(okButton);
            buttonBox.PackEnd(cancelButton);

            content.PackStart(buttonBox);

            this.Content = content;

            if (_action == DialogAction.Create)
            {
                this.Title = "Add Workspace" + " - " + projectCollection.Server.Name + " - " + projectCollection.Name;
                FillFieldsDefault();
            }
            else
            {
                this.Title = "Edit Workspace " + _workspace.Name + " - " + projectCollection.Server.Name + " - " + projectCollection.Name;
                FillFields();
                FillWorkingFolders();
            }
        }

        private void OnAddWorkingFolder(object sender, EventArgs e)
        {
            using (var projectSelect = new ProjectSelectDialog(this.projectCollection))
            {
                if (projectSelect.Run(this) == Command.Ok && !string.IsNullOrEmpty(projectSelect.SelectedPath))
                {
                    using (SelectFolderDialog folderSelect = new SelectFolderDialog("Browse For Folder"))
                    {
                        folderSelect.Multiselect = false;
                        folderSelect.CanCreateFolders = true;
                        if (folderSelect.Run(this))
                        {
                            var row = _workingFoldersStore.AddRow();
                            _workingFoldersStore.SetValue(row, _tfsFolder, projectSelect.SelectedPath);
                            _workingFoldersStore.SetValue(row, _localFolder, folderSelect.Folder);
                        }
                    }
                }
            }
        }

        private void OnRemoveWorkingFolder(object sender, EventArgs e)
        {
            if (_workingFoldersView.SelectedRow < 0)
                return;
            _workingFoldersStore.RemoveRow(_workingFoldersView.SelectedRow);
        }

        private void FillFieldsDefault()
        {
            _nameEntry.Text = _computerEntry.Text = Environment.MachineName;
            _ownerEntry.Text = this.projectCollection.Server.UserName;
        }

        private void FillFields()
        {
            _nameEntry.Text = _workspace.Name;
            _ownerEntry.Text = _workspace.OwnerName;
            _computerEntry.Text = _workspace.Computer;
            _commentEntry.Text = _workspace.Comment;
        }

        private void FillWorkingFolders()
        {
            foreach (var workingFolder in _workspace.Folders)
            {
                var row = _workingFoldersStore.AddRow();
                _workingFoldersStore.SetValue(row, _tfsFolder, workingFolder.ServerItem);
                _workingFoldersStore.SetValue(row, _localFolder, workingFolder.LocalItem);
            }
        }

        private WorkspaceData BuildWorkspace()
        {
            WorkspaceData workspaceData = new WorkspaceData();
            workspaceData.Name = _nameEntry.Text;
            workspaceData.Owner = _ownerEntry.Text;
            workspaceData.Computer = _computerEntry.Text;
            workspaceData.Comment = _commentEntry.Text;
            for (int i = 0; i < _workingFoldersStore.RowCount; i++)
            {
                var tfsFolder = _workingFoldersStore.GetValue(i, _tfsFolder);
                var localFolder = _workingFoldersStore.GetValue(i, _localFolder);
                workspaceData.WorkingFolders.Add(new WorkingFolder(tfsFolder, localFolder));
            }
            return workspaceData;
        }

        private void OnAddEditWorkspace(object sender, EventArgs e)
        {
            try
            {
                var versionControl = this.projectCollection.GetService<RepositoryService>();
                switch (_action)
                {
                    case DialogAction.Create:
                        versionControl.CreateWorkspace(new Microsoft.TeamFoundation.VersionControl.Client.Workspace(versionControl, BuildWorkspace()));
                        break;
                    case DialogAction.Edit:
                        versionControl.UpdateWorkspace(_workspace.Name, _workspace.OwnerName, new Microsoft.TeamFoundation.VersionControl.Client.Workspace(versionControl, BuildWorkspace()));
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageService.ShowError(ex.Message);
            }
            this.Respond(Command.Ok);
        }
    }
}
