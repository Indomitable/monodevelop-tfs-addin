using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using MonoDevelop.Ide.Gui;
using Xwt;
using MonoDevelop.VersionControl.TFS.Helpers;
using MonoDevelop.VersionControl.TFS.Infrastructure.Objects;
using MonoDevelop.Ide;
using MonoDevelop.Core;

namespace MonoDevelop.VersionControl.TFS.GUI
{
    public class SourceControlExplorerView : AbstractXwtViewContent
    {
        readonly VBox _view = new VBox();
        readonly ListView _listView = new ListView();
        readonly ComboBox _workspaces = new ComboBox();
        readonly DataField<string> _workspaceName = new DataField<string>();
        readonly ListStore _workspaceStore;
        readonly ServerFoldersView _foldersView;
        private ServerEntry _server;

        public SourceControlExplorerView()
        {
            ContentName = GettextCatalog.GetString("Source Explorer");
            _workspaceStore = new ListStore(_workspaceName);
            _foldersView = new ServerFoldersView();
            BuildContent();
        }

        public static void Open(string serverName, string projectName)
        {
            foreach (var view in IdeApp.Workbench.Documents)
            {
                var sourceDoc = view.GetContent<SourceControlExplorerView>();
                if (sourceDoc != null)
                {
                    sourceDoc.Load(serverName);
                    sourceDoc._foldersView.ExpandProject(projectName);
                    view.Window.SelectWindow();
                    return;
                }
            }

            SourceControlExplorerView sourceControlExplorerView = new SourceControlExplorerView();
            sourceControlExplorerView.Load(serverName);
            sourceControlExplorerView._foldersView.ExpandProject(projectName);
            IdeApp.Workbench.OpenDocument(sourceControlExplorerView, true);
        }

        #region implemented abstract members of AbstractViewContent

        public override void Load(string serverName)
        {
            if (this._server != null && string.Equals(serverName, this._server.Name, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            _server = TFSVersionControlService.Instance.GetServer(serverName);
            ContentName = GettextCatalog.GetString("Source Explorer") + " - " + serverName;
            FillWorkspaces();
            _foldersView.FillTreeView(_server);
        }

        #endregion

        #region implemented abstract members of AbstractXwtViewContent

        public override Widget Widget
        {
            get
            {
                return _view;
            }
        }

        #endregion

        private void BuildContent()
        {
            HBox topBox = new HBox();
            topBox.HeightRequest = 25;

            topBox.PackStart(new Label(GettextCatalog.GetString("Workspace") + ":"));
            _workspaces.ItemsSource = _workspaceStore;
            _workspaces.Views.Add(new TextCellView(_workspaceName));
            topBox.PackStart(_workspaces);
            Button button = new Button(GettextCatalog.GetString("Manage"));
            button.Clicked += OnManageWorkspaces;
            topBox.PackStart(button);
            _view.PackStart(topBox);

            HBox box = new HBox();
            _foldersView.TreeView.MinWidth = 300;
            box.PackStart(_foldersView.TreeView);

            box.PackStart(_listView, true, true);

            _view.PackStart(box, true, true);
        }

        public override void Dispose()
        {
            base.Dispose();
            _listView.Dispose();
            _foldersView.Dispose();

            _workspaces.Dispose();
            _workspaceStore.Dispose();

            _view.Dispose();
        }

        private void FillWorkspaces()
        {
            _workspaceStore.Clear();
            var workspaces = WorkspaceHelper.GetLocalWorkspaces(_server);
            foreach (var workspace in workspaces)
            {
                var row = _workspaceStore.AddRow();
                _workspaceStore.SetValue(row, _workspaceName, workspace.Name);
            }
            if (workspaces.Count > 0)
            {
                _workspaces.SelectedIndex = 0;
            }
        }

        private void OnManageWorkspaces(object sender, EventArgs e)
        {
            using (var dialog = new WorkspacesDialog(_server))
            {
                dialog.Run(this.Widget.ParentWindow);
            }
        }
    }
}

