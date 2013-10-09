//
// WorkspacesDialog.cs
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
using MonoDevelop.VersionControl.TFS.Helpers;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using MonoDevelop.VersionControl.TFS.Infrastructure.Objects;
using System.Collections.Generic;
using MonoDevelop.Ide;
using Microsoft.TeamFoundation.VersionControl.Common;

namespace MonoDevelop.VersionControl.TFS.GUI
{
    public class WorkspacesDialog : Dialog
    {
        private readonly ListView _listView = new ListView();
        private readonly DataField<string> _name = new DataField<string>();
        private readonly DataField<string> _computer = new DataField<string>();
        private readonly DataField<string> _owner = new DataField<string>();
        private readonly DataField<string> _comment = new DataField<string>();
        private readonly ListStore _listStore;
        private readonly ProjectCollection projectCollection;
        private readonly CheckBox _showRemoteCheck = new CheckBox();

        public WorkspacesDialog(ProjectCollection projectCollection)
        {
            this.projectCollection = projectCollection;
            _listStore = new ListStore(_name, _computer, _owner, _comment);
            BuildGui();
        }

        private void BuildGui()
        {
            this.Title = GettextCatalog.GetString("Manage Workspaces" + " - " + projectCollection.Server.Name + " - " + projectCollection.Name);
            this.Resizable = false;
            VBox content = new VBox();
            content.PackStart(new Label(GettextCatalog.GetString("Showing all local workspaces to which you have access, and all remote workspaces which you own.")));
            content.PackStart(new Label(GettextCatalog.GetString("Workspaces:")));

            _listView.Columns.Add(new ListViewColumn(GettextCatalog.GetString("Name"), new TextCellView(_name)));
            _listView.Columns.Add(new ListViewColumn(GettextCatalog.GetString("Computer"), new TextCellView(_computer)));
            _listView.Columns.Add(new ListViewColumn(GettextCatalog.GetString("Owner"), new TextCellView(_owner)));
            _listView.Columns.Add(new ListViewColumn(GettextCatalog.GetString("Comment"), new TextCellView(_comment)));
            _listView.MinHeight = 200;
            _listView.DataSource = _listStore;

            content.PackStart(_listView);

            HBox remoteBox = new HBox();

            _showRemoteCheck.Clicked += (sender, e) => FillWorkspaces();
            remoteBox.PackStart(_showRemoteCheck);
            remoteBox.PackStart(new Label(GettextCatalog.GetString("Show remote workspaces")));
            content.PackStart(remoteBox);

            HBox buttonBox = new HBox();
            Button addWorkspaceButton = new Button(GettextCatalog.GetString("Add")) { MinWidth = Constants.ButtonWidth };
            addWorkspaceButton.Clicked += AddWorkspaceClick;
            Button editWorkspaceButton = new Button(GettextCatalog.GetString("Edit")) { MinWidth = Constants.ButtonWidth };
            editWorkspaceButton.Clicked += EditWorkspaceClick;
            Button removeWorkspaceButton = new Button(GettextCatalog.GetString("Remove")) { MinWidth = Constants.ButtonWidth };
            removeWorkspaceButton.Clicked += RemoveWorkspaceClick;
            Button closeButton = new Button(GettextCatalog.GetString("Close")) { MinWidth = Constants.ButtonWidth };
            closeButton.Clicked += (sender, e) => this.Respond(Command.Close);

            buttonBox.PackStart(addWorkspaceButton);
            buttonBox.PackStart(editWorkspaceButton);
            buttonBox.PackStart(removeWorkspaceButton);
            buttonBox.PackEnd(closeButton);

            content.PackStart(buttonBox);

            this.Content = content;

            FillWorkspaces();
        }

        private void FillWorkspaces()
        {
            _listStore.Clear();
            var workspaces = _showRemoteCheck.State == CheckBoxState.On ? WorkspaceHelper.GetRemoteWorkspaces(this.projectCollection) : 
                                                                          WorkspaceHelper.GetLocalWorkspaces(this.projectCollection);
            foreach (var workspace in workspaces)
            {
                var row = _listStore.AddRow();
                _listStore.SetValue(row, _name, workspace.Name);
                _listStore.SetValue(row, _computer, workspace.Computer);
                _listStore.SetValue(row, _owner, workspace.OwnerName);
                _listStore.SetValue(row, _comment, workspace.Comment.Replace(Environment.NewLine, " "));
            }
        }

        private void AddWorkspaceClick(object sender, EventArgs e)
        {
            ShowDialog(DialogAction.Create);
        }

        private void EditWorkspaceClick(object sender, EventArgs e)
        {
            if (_listView.SelectedRow > -1)
                ShowDialog(DialogAction.Edit);
        }

        private void RemoveWorkspaceClick(object sender, EventArgs e)
        {
            if (_listView.SelectedRow > -1 &&
                MessageService.Confirm(GettextCatalog.GetString("Are you sure you want to delete selected workspace?"), AlertButton.Yes))
            {
                var versionControl = this.projectCollection.GetService<TfsVersionControlService>();
                var name = _listStore.GetValue(_listView.SelectedRow, _name);
                var owner = _listStore.GetValue(_listView.SelectedRow, _owner);
                versionControl.DeleteWorkspace(name, owner);
                FillWorkspaces();
            }
        }

        private void ShowDialog(DialogAction action)
        {
            Workspace workspace = null;
            if (action == DialogAction.Edit)
            {
                string workspaceName = _listStore.GetValue(_listView.SelectedRow, _name);
                workspace = WorkspaceHelper.GetWorkspace(this.projectCollection, workspaceName);
            }
            using (var dialog = new WorkspaceAddEditDialog(workspace, this.projectCollection))
            {
                if (dialog.Run(this) == Command.Ok)
                {
                    FillWorkspaces();
                }
            }
        }
    }

    public class WorkspaceAddEditDialog : Dialog
    {
        private readonly DialogAction _action;
        private readonly Workspace _workspace;
        private readonly TextEntry _nameEntry = new TextEntry();
        private readonly TextEntry _ownerEntry = new TextEntry();
        private readonly TextEntry _computerEntry = new TextEntry();
        //private readonly ComboBox _permissions = new ComboBox();
        private readonly TextEntry _commentEntry = new TextEntry();
        private readonly ListView _workingFoldersView = new ListView();
        private readonly DataField<string> _tfsFolder = new DataField<string>();
        private readonly DataField<string> _localFolder = new DataField<string>();
        private readonly ListStore _workingFoldersStore;
        private readonly ProjectCollection projectCollection;

        public WorkspaceAddEditDialog(Workspace workspace, ProjectCollection projectCollection)
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
            var credentials = CredentialsManager.LoadCredential(this.projectCollection.Server.Uri);
            _ownerEntry.Text = credentials.UserName;
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
                var versionControl = this.projectCollection.GetService<TfsVersionControlService>();
                switch (_action)
                {
                    case DialogAction.Create:
                        versionControl.CreateWorkspace(new Workspace(versionControl, BuildWorkspace()));
                        break;
                    case DialogAction.Edit:
                        versionControl.UpdateWorkspace(_workspace.Name, _workspace.OwnerName, new Workspace(versionControl, BuildWorkspace()));
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

    public class ProjectSelectDialog : Dialog
    {
        private readonly DataField<string> _name = new DataField<string>();
        private readonly DataField<string> _path = new DataField<string>();
        private readonly TreeStore _treeStore;
        private readonly TreeView treeView = new TreeView();
        private readonly ProjectCollection projectCollection;

        public ProjectSelectDialog(ProjectCollection projectCollection)
        {
            this.projectCollection = projectCollection;
            _treeStore = new TreeStore(_name, _path);


            BuildGui();
            FillTreeView();
        }

        void BuildGui()
        {
            this.Title = GettextCatalog.GetString("Browse for Folder");
            //this.Resizable = false;
            VBox content = new VBox();
            content.PackStart(new Label(GettextCatalog.GetString("Team Foundation Server") + ":"));
            content.PackStart(new TextEntry { Text = projectCollection.Server.Name + " - " + projectCollection.Name, Sensitive = false, MinWidth = 300 });

            content.PackStart(new Label(GettextCatalog.GetString("Folders") + ":"));

            treeView.Columns.Add(new ListViewColumn("Name", new TextCellView(_name) { Editable = false }));
            treeView.DataSource = _treeStore;
            treeView.MinWidth = 300;
            treeView.MinHeight = 300;
            content.PackStart(treeView, true, true);
                
            content.PackStart(new Label(GettextCatalog.GetString("Folder path") + ":"));

            TextEntry folderPathEntry = new TextEntry();
            folderPathEntry.Sensitive = false;

            treeView.SelectionChanged += (sender, e) => folderPathEntry.Text = this.SelectedPath;
            content.PackStart(folderPathEntry);

            HBox buttonBox = new HBox();

            Button nextButton = new Button(GettextCatalog.GetString("Next"));
            nextButton.MinWidth = Constants.ButtonWidth;
            nextButton.Clicked += (sender, e) => Respond(Command.Ok);
            buttonBox.PackStart(nextButton);

            Button cancelButton = new Button(GettextCatalog.GetString("Cancel"));
            cancelButton.MinWidth = Constants.ButtonWidth;
            cancelButton.Clicked += (sender, e) => Respond(Command.Cancel);
            buttonBox.PackEnd(cancelButton);

            content.PackStart(buttonBox);

            this.Content = content;
        }

        public void FillTreeView()
        {
            _treeStore.Clear();
            var versionControl = this.projectCollection.GetService<TfsVersionControlService>();
            var items = versionControl.QueryItems(new ItemSpec(VersionControlPath.RootFolder, RecursionType.Full), VersionSpec.Latest, DeletedState.NonDeleted, ItemType.Folder, false);
            var root = ItemSetToHierarchItemConverter.Convert(items);
            var node = _treeStore.AddNode().SetValue(_name, root.Name).SetValue(_path, root.ServerPath);
            AddChilds(node, root.Children);
            var topNode = _treeStore.GetFirstNode();
            treeView.ExpandRow(topNode.CurrentPosition, false);
        }

        private void AddChilds(TreeNavigator node, List<HierarchyItem> children)
        {
            foreach (var child in children)
            {
                node.AddChild().SetValue(_name, child.Name).SetValue(_path, child.ServerPath);
                AddChilds(node, child.Children);
                node.MoveToParent();
            }
        }

        public string SelectedPath
        {
            get
            {
                if (treeView.SelectedRow == null)
                    return string.Empty;
                var node = _treeStore.GetNavigatorAt(treeView.SelectedRow);
                return node.GetValue(_path);
            }
        }
    }
}

