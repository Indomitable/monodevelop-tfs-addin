//
// SourceControlExplorerView.cs
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
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using MonoDevelop.Ide.Gui;
using Xwt;
using MonoDevelop.VersionControl.TFS.Helpers;
using MonoDevelop.VersionControl.TFS.Infrastructure.Objects;
using MonoDevelop.Ide;
using MonoDevelop.Core;
using System.Linq;
using System.IO;

namespace MonoDevelop.VersionControl.TFS.GUI
{
    public class SourceControlExplorerView : AbstractXwtViewContent
    {
        private readonly VBox _view = new VBox();

        #region File/Folders ListView

        private readonly Label _localFolder = new Label();
        private readonly ListView _listView = new ListView();
        private readonly DataField<ExtendedItem> _itemList = new DataField<ExtendedItem>();
        private readonly DataField<string> _nameList = new DataField<string>();
        private readonly DataField<string> _typeList = new DataField<string>();
        private readonly DataField<DateTime> _lastCheckinList = new DataField<DateTime>();
        private readonly ListStore _listStore;

        #endregion

        #region Workspaces ComboBox

        private readonly ComboBox _workspaceComboBox = new ComboBox();
        private readonly DataField<string> _workspaceName = new DataField<string>();
        private readonly ListStore _workspaceStore;

        #endregion

        #region Folders TreeView

        private readonly DataField<Item> _itemTree = new DataField<Item>();
        private readonly DataField<string> _nameTree = new DataField<string>();
        private readonly TreeView _treeView = new TreeView();
        private readonly TreeStore _treeStore;

        #endregion

        private ServerEntry _server;
        private readonly List<Workspace> _workspaces = new List<Workspace>();
        private Workspace _currentWorkspace = null;

        public SourceControlExplorerView()
        {
            ContentName = GettextCatalog.GetString("Source Explorer");
            _workspaceStore = new ListStore(_workspaceName);
            _listStore = new ListStore(_itemList, _typeList, _nameList, _lastCheckinList);
            _treeStore = new TreeStore(_itemTree, _nameTree);
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
                    sourceDoc.ExpandPath(VersionControlPath.RootFolder + projectName);
                    view.Window.SelectWindow();
                    return;
                }
            }

            SourceControlExplorerView sourceControlExplorerView = new SourceControlExplorerView();
            sourceControlExplorerView.Load(serverName);
            sourceControlExplorerView.ExpandPath(VersionControlPath.RootFolder + projectName);
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
            FillTreeView();
        }

        #endregion

        #region implemented abstract members of AbstractXwtViewContent

        public override Widget Widget { get { return _view; } }

        #endregion

        private void BuildContent()
        {
            HBox headerBox = new HBox();
            headerBox.HeightRequest = 25;

            headerBox.PackStart(new Label(GettextCatalog.GetString("Workspace") + ":"));
            _workspaceComboBox.ItemsSource = _workspaceStore;
            _workspaceComboBox.Views.Add(new TextCellView(_workspaceName));
            _workspaceComboBox.SelectionChanged += OnChangeActiveWorkspaces;
            headerBox.PackStart(_workspaceComboBox);
            Button button = new Button(GettextCatalog.GetString("Manage"));
            button.Clicked += OnManageWorkspaces;
            headerBox.PackStart(button);
            _view.PackStart(headerBox);

            HPaned mainBox = new HPaned();

            VBox treeViewBox = new VBox();
            _treeView.Columns.Add(new ListViewColumn("Folders", new TextCellView(_nameTree)));
            _treeView.DataSource = _treeStore;
            _treeView.SelectionChanged += OnFolderChanged;
            treeViewBox.WidthRequest = 50;
            treeViewBox.PackStart(_treeView, true, true);
            mainBox.Panel1.Content = treeViewBox;


            VBox rightBox = new VBox();
            HBox headerRightBox = new HBox();
            headerRightBox.PackStart(new Label(GettextCatalog.GetString("Local Path") + ":"));
            headerRightBox.PackStart(_localFolder);
            rightBox.PackStart(headerRightBox);
            _listView.Columns.Add(new ListViewColumn("Type", new TextCellView(_typeList)));
            _listView.Columns.Add(new ListViewColumn("Name", new TextCellView(_nameList)));
            _listView.Columns.Add(new ListViewColumn("Last Check-in", new TextCellView(_lastCheckinList)));
            _listView.RowActivated += OnListItemClicked;
            _listView.DataSource = _listStore;
            rightBox.PackStart(_listView, true, true);
            mainBox.Panel2.Content = rightBox;

            _view.PackStart(mainBox, true, true);
        }

        private void FillWorkspaces()
        {
            _workspaceStore.Clear();
            _workspaces.Clear();
            _workspaces.AddRange(WorkspaceHelper.GetLocalWorkspaces(_server));
            int activeWorkspaceRow = -1;
            string activeWorkspace = TFSVersionControlService.Instance.GetActiveWorkspace(_server);
            foreach (var workspace in _workspaces)
            {
                var row = _workspaceStore.AddRow();
                _workspaceStore.SetValue(row, _workspaceName, workspace.Name);
                if (string.Equals(workspace.Name, activeWorkspace, StringComparison.Ordinal))
                {
                    activeWorkspaceRow = row;
                }
            }
            if (_workspaces.Count > 0)
            {
                if (activeWorkspaceRow > -1)
                    _workspaceComboBox.SelectedIndex = activeWorkspaceRow;
                else
                    _workspaceComboBox.SelectedIndex = 0;
            }
        }

        private void FillTreeView()
        {
            _treeStore.Clear();
            using (var tfsServer = TeamFoundationServerHelper.GetServer(_server))
            {
                tfsServer.Authenticate();

                var versionControl = tfsServer.GetService<VersionControlServer>();
                var itemSet = versionControl.GetItems(new ItemSpec(VersionControlPath.RootFolder, RecursionType.Full), VersionSpec.Latest, DeletedState.NonDeleted, ItemType.Folder, false);

                var root = ItemSetToHierarchItemConverter.Convert(itemSet.Items);
                var node = _treeStore.AddNode().SetValue(_nameTree, root.Name).SetValue(_itemTree, root.Item);
                AddChilds(node, root.Children);
                var topNode = _treeStore.GetFirstNode();
                _treeView.ExpandRow(topNode.CurrentPosition, false);
            }
        }

        private void FillListView(string serverPath)
        {
            _listStore.Clear();
            using (var tfsServer = TeamFoundationServerHelper.GetServer(_server))
            {
                tfsServer.Authenticate();
                var versionControl = tfsServer.GetService<VersionControlServer>();
                var itemSet = versionControl.GetExtendedItems(serverPath, DeletedState.NonDeleted, ItemType.Any);
                foreach (var item in itemSet.OrderBy(i => i.ItemType).ThenBy(i => i.TargetServerItem))
                {
                    var row = _listStore.AddRow();
                    _listStore.SetValue(row, _itemList, item);
                    _listStore.SetValue(row, _typeList, item.ItemType.ToString());
                    _listStore.SetValue(row, _nameList, item.TargetServerItem);
                    _listStore.SetValue(row, _lastCheckinList, item.CheckinDate);
                }
            }
        }

        private void OnChangeActiveWorkspaces(object sender, EventArgs ev)
        {
            if (_workspaceComboBox.SelectedIndex > -1)
            {
                var name = _workspaceStore.GetValue(_workspaceComboBox.SelectedIndex, _workspaceName);
                _currentWorkspace = _workspaces.Single(ws => string.Equals(ws.Name, name, StringComparison.Ordinal));
                TFSVersionControlService.Instance.SetActiveWorkspace(_server, name);
            }
            else
            {
                TFSVersionControlService.Instance.SetActiveWorkspace(_server, string.Empty);
            }
        }

        private void AddChilds(TreeNavigator node, List<HierarchyItem> children)
        {
            foreach (var child in children)
            {
                node.AddChild().SetValue(_nameTree, child.Name).SetValue(_itemTree, child.Item);
                AddChilds(node, child.Children);
                node.MoveToParent();
            }
        }

        private void ExpandPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;
            var node = _treeStore.GetFirstNode();
            node = FindTreeItem(node, path);
            if (node == null)
                return;
            var copyPosition = node.CurrentPosition;
            while (node.MoveToParent())
                _treeView.ExpandRow(node.CurrentPosition, false);
            _treeView.ExpandRow(copyPosition, false);
            _treeView.ScrollToRow(copyPosition);
            _treeView.SelectRow(copyPosition);
        }

        private TreeNavigator FindTreeItem(TreeNavigator navigator, string path)
        {
            if (string.Equals(navigator.GetValue(_itemTree).ServerItem, path, StringComparison.OrdinalIgnoreCase))
                return navigator;
            if (navigator.MoveToChild())
            {
                return FindTreeItem(navigator, path);
            }
            else
            {
                if (!navigator.MoveNext())
                {
                    if (!navigator.MoveToParent())
                        return null;
                    while (!navigator.MoveNext())
                    {
                        navigator.MoveToParent();
                    }
                }
                return FindTreeItem(navigator, path);
            }
        }

        private void ShowMappingPath(string serverPath)
        {
            if (_currentWorkspace == null)
                _localFolder.Text = GettextCatalog.GetString("Not Mapped");
            else
            {
                bool foundMap = false;
                foreach (var mappedFolder in _currentWorkspace.Folders)
                {
                    if (serverPath.StartsWith(mappedFolder.ServerItem, StringComparison.Ordinal))
                    {
                        if (string.Equals(serverPath, mappedFolder.ServerItem))
                            _localFolder.Text = mappedFolder.LocalItem;
                        else
                        {
                            string rest = serverPath.Substring(mappedFolder.ServerItem.Length + 1);
                            _localFolder.Text = Path.Combine(mappedFolder.LocalItem, rest);
                        }
                        foundMap = true;
                        break;
                    }
                }
                if (!foundMap)
                {
                    _localFolder.Text = GettextCatalog.GetString("Not Mapped");
                }
            }
        }

        #region Events

        private void OnFolderChanged(object sender, EventArgs e)
        {
            if (_treeView.SelectedRow == null)
                return;

            var navigator = _treeStore.GetNavigatorAt(_treeView.SelectedRow);
            var item = navigator.GetValue(_itemTree);
            FillListView(item.ServerItem);
            ShowMappingPath(item.ServerItem);
        }

        private void OnManageWorkspaces(object sender, EventArgs e)
        {
            using (var dialog = new WorkspacesDialog(_server))
            {
                if (dialog.Run(this.Widget.ParentWindow) == Command.Close)
                {
                    FillWorkspaces();
                }
            }
        }

        private void OnListItemClicked(object sender, ListViewRowEventArgs e)
        {
            if (e.RowIndex < 0)
                return;
            var item = _listStore.GetValue(e.RowIndex, _itemList);
            if (item.ItemType == ItemType.Folder)
                ExpandPath(item.TargetServerItem);
        }

        #endregion

        public override void Dispose()
        {
            base.Dispose();
            _listView.Dispose();
            _treeStore.Dispose();
            _treeView.SelectionChanged -= OnFolderChanged;
            _treeView.Dispose();

            _workspaceComboBox.Dispose();
            _workspaceStore.Dispose();

            _view.Dispose();
        }
    }
}