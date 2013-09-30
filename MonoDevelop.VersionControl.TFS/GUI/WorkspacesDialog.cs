using System;
using Xwt;
using MonoDevelop.Core;
using MonoDevelop.VersionControl.TFS.Helpers;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;

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

        public string ServerName { get; set; }

        public WorkspacesDialog(string serverName)
        {
            this.ServerName = serverName;
            _listStore = new ListStore(_name, _computer, _owner, _comment);
            BuildGui();
        }

        private void BuildGui()
        {
            this.Title = GettextCatalog.GetString("Manage Workspaces");
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
            CheckBox check = new CheckBox();
            check.Clicked += (sender, e) => FillWorkspaces(check.State == CheckBoxState.On);
            remoteBox.PackStart(check);
            remoteBox.PackStart(new Label(GettextCatalog.GetString("Show remote workspaces")));
            content.PackStart(remoteBox);

            HBox buttonBox = new HBox();
            const int buttonWidth = 100;
            Button addWorkspaceButton = new Button(GettextCatalog.GetString("Add")) { MinWidth = buttonWidth };
            Button editWorkspaceButton = new Button(GettextCatalog.GetString("Edit")) { MinWidth = buttonWidth };
            Button removeWorkspaceButton = new Button(GettextCatalog.GetString("Remove")) { MinWidth = buttonWidth };
            Button closeButton = new Button(GettextCatalog.GetString("Close")) { MinWidth = buttonWidth };
            closeButton.Clicked += (sender, e) => this.Respond(Command.Close);

            buttonBox.PackStart(addWorkspaceButton);
            buttonBox.PackStart(editWorkspaceButton);
            buttonBox.PackStart(removeWorkspaceButton);
            buttonBox.PackEnd(closeButton);

            content.PackStart(buttonBox);

            this.Content = content;

            FillWorkspaces(false);
        }

        private void FillWorkspaces(bool showRemote)
        {
            _listStore.Clear();
            var workspaces = showRemote ? WorkspaceHelper.GetRemoteWorkspaces(ServerName) : WorkspaceHelper.GetLocalWorkspaces(ServerName);
            foreach (var workspace in workspaces)
            {
                var row = _listStore.AddRow();
                _listStore.SetValue(row, _name, workspace.Name);
                _listStore.SetValue(row, _computer, workspace.Computer);
                _listStore.SetValue(row, _owner, workspace.OwnerName);
                _listStore.SetValue(row, _comment, workspace.Comment.Replace(Environment.NewLine, " "));
            }
        }
    }

    public class WorkspaceAddEditDialog : Dialog
    {
        enum DialogAction
        {
            Create,
            Edit
        }

        private readonly DialogAction _action;
        private readonly Workspace _workspace;
        private readonly TextEntry _nameEntry = new TextEntry();
        private readonly TextEntry _ownerEntry = new TextEntry();
        private readonly TextEntry _computerEntry = new TextEntry();
        private readonly ComboBox _permissions = new ComboBox();

        public WorkspaceAddEditDialog()
        {
            _action = DialogAction.Create;
        }

        public WorkspaceAddEditDialog(Workspace workspace)
        {
            this._workspace = workspace;
            _action = DialogAction.Edit;
        }

        private void BuildGui()
        {
            if (_action == DialogAction.Create)
            {
                this.Title = "Add Workspace";
                FillFieldsDefault();
            }
            else
            {
                this.Title = "Edit Workspace " + _workspace.Name;
                FillFields();
            }
        }

        private void FillFieldsDefault()
        {

        }

        private void FillFields()
        {

        }
    }
}

