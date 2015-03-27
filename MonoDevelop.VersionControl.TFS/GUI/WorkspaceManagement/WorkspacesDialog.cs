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
using MonoDevelop.Ide;
using MonoDevelop.VersionControl.TFS.Core.Structure;
using MonoDevelop.VersionControl.TFS.VersionControl.Structure;

namespace MonoDevelop.VersionControl.TFS.GUI.WorkspaceManagement
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

        internal WorkspacesDialog(ProjectCollection projectCollection)
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
            var workspaces = _showRemoteCheck.State == CheckBoxState.On ? this.projectCollection.GetRemoteWorkspaces() : this.projectCollection.GetLocalWorkspaces();
            foreach (var workspace in workspaces)
            {
                var row = _listStore.AddRow();
                _listStore.SetValue(row, _name, workspace.Name);
                _listStore.SetValue(row, _computer, workspace.Computer);
                _listStore.SetValue(row, _owner, workspace.Owner);
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
                var name = _listStore.GetValue(_listView.SelectedRow, _name);
                var owner = _listStore.GetValue(_listView.SelectedRow, _owner);
                this.projectCollection.DeleteWorkspace(name, owner);
                FillWorkspaces();
            }
        }

        private void ShowDialog(DialogAction action)
        {
            WorkspaceData workspaceData = null;
            if (action == DialogAction.Edit)
            {
                string workspaceName = _listStore.GetValue(_listView.SelectedRow, _name);
                workspaceData = this.projectCollection.GetWorkspace(workspaceName);
            }
            using (var dialog = new WorkspaceAddEditDialog(workspaceData, this.projectCollection))
            {
                if (dialog.Run(this) == Command.Ok)
                {
                    FillWorkspaces();
                }
            }
        }
    }
}

