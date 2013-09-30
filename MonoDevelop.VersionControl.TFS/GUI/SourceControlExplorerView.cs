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
        readonly TreeView _treeView = new TreeView();
        readonly ListView _listView = new ListView();
        readonly DataField<string> _name = new DataField<string>();
        readonly DataField<string> _path = new DataField<string>();
        readonly TreeStore _treeStore;
        readonly ComboBox _workspaces = new ComboBox();
        readonly DataField<string> _workspaceName = new DataField<string>();
        readonly ListStore _workspaceStore;

        public SourceControlExplorerView()
        {
            ContentName = GettextCatalog.GetString("Source Explorer");
            _treeStore = new TreeStore(_name, _path);
            _workspaceStore = new ListStore(_workspaceName);
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
                    sourceDoc.ExpandProject(projectName);
                    view.Window.SelectWindow();
                    return;
                }
            }

            SourceControlExplorerView sourceControlExplorerView = new SourceControlExplorerView();
            sourceControlExplorerView.Load(serverName);
            sourceControlExplorerView.ExpandProject(projectName);
            IdeApp.Workbench.OpenDocument(sourceControlExplorerView, true);
        }

        public string ServerName { get; set; }

        #region implemented abstract members of AbstractViewContent

        public override void Load(string serverName)
        {
            if (string.Equals(serverName, this.ServerName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            this.ServerName = serverName;
            var server = TFSVersionControlService.Instance.GetServer(this.ServerName);
            ContentName = GettextCatalog.GetString("Source Explorer") + " - " + this.ServerName;
            FillWorkspaces();
            LoadTreeView(server.Url);
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
            topBox.PackStart(_workspaces);
            Button button = new Button(GettextCatalog.GetString("Manage"));
            button.Clicked += OnManageWorkspaces;

            _view.PackStart(topBox);

            HBox box = new HBox();
            _treeView.MinWidth = 300;
            box.PackStart(_treeView);

            _treeView.Columns.Add(new ListViewColumn("Name", new TextCellView(_name) { Editable = false }));

        
            _treeView.DataSource = _treeStore;

            box.PackStart(_listView, true, true);

            _view.PackStart(box, true, true);
        }

        private void LoadTreeView(Uri serverUrl)
        {
            _treeStore.Clear();
            var credentials = CredentialsManager.LoadCredential(serverUrl);
            using (var tfsServer = TeamFoundationServerFactory.GetServer(serverUrl, credentials))
            {
                tfsServer.Authenticate();

                var versionControl = tfsServer.GetService<VersionControlServer>();
                var itemSet = versionControl.GetItems(new ItemSpec(VersionControlPath.RootFolder, RecursionType.Full), VersionSpec.Latest, DeletedState.NonDeleted, ItemType.Folder, false);

                var root = ItemSetToHierarchItemConverter.Convert(itemSet.Items);
                var node = _treeStore.AddNode().SetValue(_name, root.Name).SetValue(_path, root.ServerPath);
                AddChilds(node, root.Children);
                var topNode = _treeStore.GetFirstNode();
                _treeView.ExpandRow(topNode.CurrentPosition, false);
            }
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

        private void ExpandProject(string projectName)
        {
            if (string.IsNullOrEmpty(projectName))
                return;
            var node = _treeStore.GetFirstNode();
            node.MoveToChild();
            while (!string.Equals(node.GetValue(_name), projectName, StringComparison.OrdinalIgnoreCase))
            {
                if (!node.MoveNext())
                {
                    return;
                }
            }
            _treeView.ExpandRow(node.CurrentPosition, false);
            _treeView.ScrollToRow(node.CurrentPosition);
            _treeView.SelectRow(node.CurrentPosition);
        }

        public override void Dispose()
        {
            base.Dispose();
            _listView.Dispose();
            _treeView.Dispose();
            _treeStore.Dispose();

            _workspaces.Dispose();
            _workspaceStore.Dispose();

            _view.Dispose();
        }

        private void FillWorkspaces()
        {
            var server = TFSVersionControlService.Instance.GetServer(ServerName);
            _workspaceStore.Clear();
            var credentials = CredentialsManager.LoadCredential(server.Url);
            using (var tfsServer = TeamFoundationServerFactory.GetServer(server.Url, credentials))
            {
                tfsServer.Authenticate();
                var versionControl = tfsServer.GetService<VersionControlServer>();

                var workspaces = versionControl.QueryWorkspaces(string.Empty, credentials.UserName, string.Empty); //"VMLADENOV-7"
                foreach (var workspace in workspaces)
                {
                    var row = _workspaceStore.AddRow();
                    _workspaceStore.SetValue(row, _workspaceName, workspace.Name);
                }
//                if (_workspaces.Items.Count > 0)
//                {
//                    _workspaces.SelectedIndex = 0;
//                }
            }
        }

        private void OnManageWorkspaces(object sender, EventArgs e)
        {

        }
    }
}

