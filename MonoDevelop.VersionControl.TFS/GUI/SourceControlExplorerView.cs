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

        public SourceControlExplorerView()
        {
            ContentName = GettextCatalog.GetString("Source Explorer");
            _treeStore = new TreeStore(_name, _path);
            BuildContent();
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
            topBox.HeightRequest = 20;
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

        public void ExpandProject(string projectName)
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
    }
}

