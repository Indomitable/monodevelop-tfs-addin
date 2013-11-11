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
using MonoDevelop.Ide.Gui;
using Xwt;
using MonoDevelop.VersionControl.TFS.Helpers;
using MonoDevelop.VersionControl.TFS.Infrastructure.Objects;
using MonoDevelop.Ide;
using MonoDevelop.Core;
using System.Linq;
using System.IO;
using Microsoft.TeamFoundation.VersionControl.Client.Objects;
using Microsoft.TeamFoundation.VersionControl.Client.Enums;

namespace MonoDevelop.VersionControl.TFS.GUI
{
    public class SourceControlExplorerView : AbstractXwtViewContent
    {
        private readonly VBox _view = new VBox();
        //private readonly Menu _menu = new Menu();

        #region File/Folders ListView

        private readonly Label _localFolder = new Label();
        private readonly ListView _listView = new ListView();
        private readonly DataField<ExtendedItem> _itemList = new DataField<ExtendedItem>();
        private readonly DataField<string> _typeList = new DataField<string>();
        private readonly DataField<string> _nameList = new DataField<string>();
        private readonly DataField<string> _changeList = new DataField<string>();
        private readonly DataField<string> _userList = new DataField<string>();
        private readonly DataField<string> _latestList = new DataField<string>();
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

        private Microsoft.TeamFoundation.Client.ProjectCollection projectCollection;
        private readonly List<Workspace> _workspaces = new List<Workspace>();
        private Workspace _currentWorkspace = null;

        public SourceControlExplorerView()
        {
            ContentName = GettextCatalog.GetString("Source Explorer");
            _workspaceStore = new ListStore(_workspaceName);
            _listStore = new ListStore(_itemList, _typeList, _nameList, _changeList, _userList, _latestList, _lastCheckinList);
            _treeStore = new TreeStore(_itemTree, _nameTree);
            BuildContent();
        }

        public static void Open(Microsoft.TeamFoundation.Client.ProjectInfo project)
        {
            foreach (var view in IdeApp.Workbench.Documents)
            {
                var sourceDoc = view.GetContent<SourceControlExplorerView>();
                if (sourceDoc != null)
                {
                    sourceDoc.Load(project);
                    sourceDoc.ExpandPath(VersionControlPath.RootFolder + project.Name);
                    view.Window.SelectWindow();
                    return;
                }
            }

            SourceControlExplorerView sourceControlExplorerView = new SourceControlExplorerView();
            sourceControlExplorerView.Load(project);
            sourceControlExplorerView.ExpandPath(VersionControlPath.RootFolder + project.Name);
            IdeApp.Workbench.OpenDocument(sourceControlExplorerView, true);
        }

        #region implemented abstract members of AbstractViewContent

        public override void Load(string fileName)
        {
            throw new NotImplementedException();
        }

        public void Load(Microsoft.TeamFoundation.Client.ProjectInfo project)
        {
            if (this.projectCollection != null && string.Equals(project.Collection.Id, this.projectCollection.Id, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            projectCollection = project.Collection;
            ContentName = GettextCatalog.GetString("Source Explorer") + " - " + projectCollection.Server.Name + " - " + projectCollection.Name;
            using (var progress = new MonoDevelop.Ide.ProgressMonitoring.MessageDialogProgressMonitor(true))
            {
                progress.BeginTask("Loading...", 2);
                FillWorkspaces();
                progress.Step(1);
                FillTreeView();
                progress.EndTask();
            }
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
            _treeView.ButtonPressed += (sender, e) =>
            {
                if (_treeView.SelectedRow != null && e.Button == PointerButton.Right)
                {
                    BuildPopupMenu(MenuInvoker.TreeView).Popup();
                }
            };
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
            _listView.Columns.Add(new ListViewColumn("Pending Change", new TextCellView(_changeList)));
            _listView.Columns.Add(new ListViewColumn("User", new TextCellView(_userList)));
            _listView.Columns.Add(new ListViewColumn("Latest", new TextCellView(_latestList)));
            _listView.Columns.Add(new ListViewColumn("Last Check-in", new TextCellView(_lastCheckinList)));
            _listView.RowActivated += OnListItemClicked;
            _listView.DataSource = _listStore;

            _listView.ButtonPressed += (sender, e) =>
            {
                if (e.Button == PointerButton.Right && _listView.SelectedRow >= 0)
                {
                    BuildPopupMenu(MenuInvoker.ListView).Popup();
                }
            };
            rightBox.PackStart(_listView, true, true);
            mainBox.Panel2.Content = rightBox;

            _view.PackStart(mainBox, true, true);
        }

        private void FillWorkspaces()
        {
            _workspaceStore.Clear();
            _workspaces.Clear();
            _workspaces.AddRange(WorkspaceHelper.GetLocalWorkspaces(projectCollection));
            int activeWorkspaceRow = -1;
            string activeWorkspace = TFSVersionControlService.Instance.GetActiveWorkspace(projectCollection);
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
            var versionControl = projectCollection.GetService<RepositoryService>();
            var items = versionControl.QueryItems(this._currentWorkspace, new ItemSpec(VersionControlPath.RootFolder, RecursionType.Full), VersionSpec.Latest, DeletedState.NonDeleted, ItemType.Folder, false);

            var root = ItemSetToHierarchItemConverter.Convert(items);
            var node = _treeStore.AddNode().SetValue(_nameTree, root.Name).SetValue(_itemTree, root.Item);
            AddChilds(node, root.Children);
            var topNode = _treeStore.GetFirstNode();
            _treeView.ExpandRow(topNode.CurrentPosition, false);
        }

        private void FillListView(string serverPath)
        {
            _listStore.Clear();

            var versionControl = projectCollection.GetService<RepositoryService>();
            var itemSet = versionControl.QueryItemsExtended(this._currentWorkspace, new ItemSpec(serverPath, RecursionType.OneLevel), DeletedState.NonDeleted, ItemType.Any);
            foreach (var item in itemSet.Skip(1).OrderBy(i => i.ItemType).ThenBy(i => i.TargetServerItem))
            {
                var row = _listStore.AddRow();
                _listStore.SetValue(row, _itemList, item);
                _listStore.SetValue(row, _typeList, item.ItemType.ToString());
                _listStore.SetValue(row, _nameList, item.TargetServerItem);
                if (this._currentWorkspace != null)
                {
                    if (item.ChangeType != ChangeType.None && !item.HasOtherPendingChange)
                    {
                        _listStore.SetValue(row, _changeList, item.ChangeType.ToString());
                        _listStore.SetValue(row, _userList, this._currentWorkspace.OwnerName);
                    }
                    if (item.HasOtherPendingChange)
                    {
                        var remoteChanges = this._currentWorkspace.GetPendingSets(item.SourceServerItem, RecursionType.None);
                        List<string> changeNames = new List<string>();
                        List<string> userNames = new List<string>();
                        foreach (var remoteChange in remoteChanges)
                        {
                            var pChange = remoteChange.PendingChanges.FirstOrDefault();
                            if (pChange == null)
                                continue;
                            changeNames.Add(pChange.ChangeType.ToString());
                            userNames.Add(remoteChange.Owner);
                        }
                        _listStore.SetValue(row, _changeList, string.Join(", ", changeNames));
                        _listStore.SetValue(row, _userList, string.Join(", ", userNames));
                    }
                }
                if (!IsMapped(serverPath))
                {
                    _listStore.SetValue(row, _latestList, "Not mapped");
                }
                else
                {
                    if (!item.IsInWorkspace)
                    {
                        _listStore.SetValue(row, _latestList, "Not downloaded");
                    }
                    else
                    {
                        _listStore.SetValue(row, _latestList, item.IsLatest ? "Yes" : "No");
                    }
                }
                _listStore.SetValue(row, _lastCheckinList, item.CheckinDate);
            }
        }

        private void OnChangeActiveWorkspaces(object sender, EventArgs ev)
        {
            if (_workspaceComboBox.SelectedIndex > -1)
            {
                var name = _workspaceStore.GetValue(_workspaceComboBox.SelectedIndex, _workspaceName);
                _currentWorkspace = _workspaces.Single(ws => string.Equals(ws.Name, name, StringComparison.Ordinal));
                TFSVersionControlService.Instance.SetActiveWorkspace(projectCollection, name);
                if (_treeView.SelectedRow != null)
                    ShowMappingPath(_treeStore.GetNavigatorAt(_treeView.SelectedRow).GetValue(_itemTree).ServerItem);
            }
            else
            {
                TFSVersionControlService.Instance.SetActiveWorkspace(projectCollection, string.Empty);
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

        private bool IsMapped(string serverPath)
        {
            if (_currentWorkspace == null)
                return false;
            return _currentWorkspace.IsServerPathMapped(serverPath);
        }

        private void ShowMappingPath(string serverPath)
        {
            if (!IsMapped(serverPath))
            {
                _localFolder.Text = GettextCatalog.GetString("Not Mapped");
                return;
            }
            var mappedFolder = _currentWorkspace.Folders.First(f => serverPath.StartsWith(f.ServerItem, StringComparison.Ordinal));
            if (string.Equals(serverPath, mappedFolder.ServerItem, StringComparison.Ordinal))
                _localFolder.Text = mappedFolder.LocalItem;
            else
            {
                string rest = serverPath.Substring(mappedFolder.ServerItem.Length + 1);
                _localFolder.Text = Path.Combine(mappedFolder.LocalItem, rest);
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
            using (var dialog = new WorkspacesDialog(projectCollection))
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
            if (item.ItemType == ItemType.File && IsMapped(item.ServerPath))
            {
                if (MonoDevelop.Projects.Services.ProjectService.IsWorkspaceItemFile(item.LocalItem))
                {
                    IdeApp.Workspace.OpenWorkspaceItem(item.LocalItem, true);
                }
                else
                {
                    IdeApp.Workbench.OpenDocument(item.LocalItem, null, true);
                }
            }
        }

        #endregion

        #region Popup Menu

        enum MenuInvoker
        {
            TreeView,
            ListView
        }

        private Menu BuildPopupMenu(MenuInvoker invoker)
        {
            Menu menu = new Menu();
            IItem item = invoker == MenuInvoker.ListView ?
                         (IItem)_listStore.GetValue(_listView.SelectedRow, _itemList) :
                         (IItem)_treeStore.GetNavigatorAt(_treeView.SelectedRow).GetValue(_itemTree);

            if (IsMapped(item.ServerPath))
            {
                MenuItem getLatestVersionItem = new MenuItem(GettextCatalog.GetString("Get Latest Version"));
                getLatestVersionItem.Clicked += (sender, e) => GetLatestVersion(item, invoker);
                menu.Items.Add(getLatestVersionItem);

                MenuItem forceGetLatestVersionItem = new MenuItem(GettextCatalog.GetString("Force Get Latest Version"));
                forceGetLatestVersionItem.Clicked += (sender, e) => ForceGetLatestVersion(item, invoker);
                menu.Items.Add(forceGetLatestVersionItem);

                if (invoker == MenuInvoker.ListView) //List Popup Menu
                {
                    var listItem = (ExtendedItem)item;
                    if (listItem.ChangeType != ChangeType.None)
                    {
                        MenuItem revertItem = new MenuItem(GettextCatalog.GetString("Undo Changes"));
                        revertItem.Clicked += (sender, e) => UndoChanges(listItem);
                        menu.Items.Add(revertItem);
                    }
                }
            }
            return menu;
        }

        private void GetLatestVersion(IItem item, MenuInvoker invoker)
        {
            RecursionType recursion = item.ItemType == ItemType.File ? RecursionType.None : RecursionType.Full;
            GetRequest request = new GetRequest(item.ServerPath, recursion, VersionSpec.Latest);
            _currentWorkspace.Get(request, GetOptions.None);
            //Refresh List
            if (invoker == MenuInvoker.TreeView)
            {
                FillListView(item.ServerPath);
            }
            else
            {
                FillListView(item.ServerPath.ParentPath);
            }
        }

        private void ForceGetLatestVersion(IItem item, MenuInvoker invoker)
        {
            RecursionType recursion = item.ItemType == ItemType.File ? RecursionType.None : RecursionType.Full;
            GetRequest request = new GetRequest(item.ServerPath, recursion, VersionSpec.Latest);
            _currentWorkspace.Get(request, GetOptions.Overwrite);
            //Refresh List
            if (invoker == MenuInvoker.TreeView)
            {
                FillListView(item.ServerPath);
            }
            else
            {
                FillListView(item.ServerPath.ParentPath);
            }
        }

        private void UndoChanges(ExtendedItem item)
        {
            RecursionType recursion = item.ItemType == ItemType.File ? RecursionType.None : RecursionType.Full;
            _currentWorkspace.Undo(new List<FilePath> { item.LocalItem }, recursion);
            FillListView(item.ServerPath.ParentPath);
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