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
using Xwt.Drawing;
using MonoDevelop.VersionControl.TFS.GUI.Workspace;

namespace MonoDevelop.VersionControl.TFS.GUI.VersionControl
{
    public class SourceControlExplorerView : AbstractXwtViewContent
    {
        private readonly VBox _view = new VBox();
        //private readonly Menu _menu = new Menu();

        #region File/Folders ListView

        private readonly Label _localFolder = new Label();
        private readonly ListView _listView = new ListView();
        private readonly DataField<Image> _iconList = new DataField<Image>();
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

        private readonly DataField<Image> _iconTree = new DataField<Image>();
        private readonly DataField<Item> _itemTree = new DataField<Item>();
        private readonly DataField<string> _nameTree = new DataField<string>();
        private readonly TreeView _treeView = new TreeView();
        private readonly TreeStore _treeStore;

        #endregion

        private Microsoft.TeamFoundation.Client.ProjectCollection projectCollection;
        private readonly List<Microsoft.TeamFoundation.VersionControl.Client.Workspace> _workspaces = new List<Microsoft.TeamFoundation.VersionControl.Client.Workspace>();
        private Microsoft.TeamFoundation.VersionControl.Client.Workspace _currentWorkspace = null;

        public SourceControlExplorerView()
        {
            ContentName = GettextCatalog.GetString("Source Explorer");
            _workspaceStore = new ListStore(_workspaceName);
            _listStore = new ListStore(_typeList, _iconList, _itemList, _nameList, _changeList, _userList, _latestList, _lastCheckinList);
            _treeStore = new TreeStore(_iconTree, _itemTree, _nameTree);
            BuildGui();
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

        public override void Load(string fileName)
        {
            throw new NotSupportedException();
        }

        public void Load(Microsoft.TeamFoundation.Client.ProjectInfo project)
        {
            if (this.projectCollection != null && string.Equals(project.Collection.Id, this.projectCollection.Id, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            projectCollection = project.Collection;
            ContentName = GettextCatalog.GetString("Source Explorer") + " - " + projectCollection.Server.Name + " - " + projectCollection.Name;
            using (var progress = new MonoDevelop.Ide.ProgressMonitoring.MessageDialogProgressMonitor(true, false, false))
            {
                progress.BeginTask("Loading...", 2);
                FillWorkspaces();
                progress.Step(1);
                FillTreeView();
                progress.EndTask();
            }
        }

        public override Widget Widget { get { return _view; } }

        private void BuildGui()
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

            _treeView.Columns.Add("Folders", new ImageCellView(_iconTree), new TextCellView(_nameTree));
            _treeView.DataSource = _treeStore;
            _treeView.SelectionChanged += OnFolderChanged;
            treeViewBox.WidthRequest = 80;
            treeViewBox.PackStart(_treeView, true, true);
            mainBox.Panel1.Shrink = false;
            mainBox.Panel1.Content = treeViewBox;


            VBox rightBox = new VBox();
            HBox headerRightBox = new HBox();
            headerRightBox.PackStart(new Label(GettextCatalog.GetString("Local Path") + ":"));
            headerRightBox.PackStart(_localFolder);
            rightBox.PackStart(headerRightBox);
            //_listView.Columns.Add(new ListViewColumn("Type", new TextCellView(_typeList)));
            _listView.Columns.Add("Name", new ImageCellView(_iconList), new TextCellView(_nameList));
            _listView.Columns.Add(new ListViewColumn("Pending Change", new TextCellView(_changeList)));
            _listView.Columns.Add(new ListViewColumn("User", new TextCellView(_userList)));
            _listView.Columns.Add(new ListViewColumn("Latest", new TextCellView(_latestList)));
            _listView.Columns.Add(new ListViewColumn("Last Check-in", new TextCellView(_lastCheckinList)));
            _listView.SelectionMode = SelectionMode.Multiple;
            _listView.RowActivated += OnListItemClicked;
            _listView.DataSource = _listStore;

            _listView.ButtonPressed += (sender, e) =>
            {
                if (e.Button == PointerButton.Right && _listView.SelectedRows.Any())
                {
                    var menu = BuildPopupMenu();
                    if (menu.Items.Any())
                        menu.Popup();
                }
            };
            rightBox.PackStart(_listView, true, true);
            mainBox.Panel2.Content = rightBox;

            _view.PackStart(mainBox, true, true);
        }

        private void FillWorkspaces()
        {
            string activeWorkspace = TFSVersionControlService.Instance.GetActiveWorkspace(projectCollection);
            _workspaceComboBox.SelectionChanged -= OnChangeActiveWorkspaces;
            _workspaceStore.Clear();
            _workspaces.Clear();
            _workspaces.AddRange(WorkspaceHelper.GetLocalWorkspaces(projectCollection));
            int activeWorkspaceRow = -1;
            foreach (var workspace in _workspaces)
            {
                var row = _workspaceStore.AddRow();
                _workspaceStore.SetValue(row, _workspaceName, workspace.Name);
                if (string.Equals(workspace.Name, activeWorkspace, StringComparison.Ordinal))
                {
                    activeWorkspaceRow = row;
                }
            }
            _workspaceComboBox.SelectionChanged += OnChangeActiveWorkspaces;
            if (_workspaces.Count > 0)
            {
                if (activeWorkspaceRow > -1)
                    _workspaceComboBox.SelectedIndex = activeWorkspaceRow;
                else
                    _workspaceComboBox.SelectedIndex = 0;
            }

        }

        private Image GetRepositoryImage()
        {
            var image = Image.FromResource("MonoDevelop.VersionControl.TFS.Icons.repository.png");
            return image.Scale(0.6);
        }

        private void FillTreeView()
        {
            _treeStore.Clear();
            var versionControl = projectCollection.GetService<RepositoryService>();
            var items = versionControl.QueryItems(this._currentWorkspace, new ItemSpec(VersionControlPath.RootFolder, RecursionType.Full), VersionSpec.Latest, DeletedState.NonDeleted, ItemType.Folder, false);

            var root = ItemSetToHierarchItemConverter.Convert(items);
            var node = _treeStore.AddNode().SetValue(_iconTree, GetRepositoryImage()).SetValue(_nameTree, root.Name).SetValue(_itemTree, root.Item);
            AddChilds(node, root.Children);
            var topNode = _treeStore.GetFirstNode();
            _treeView.ExpandRow(topNode.CurrentPosition, false);
        }

        private Image GetItemImage(ItemType itemType)
        {
            if (itemType == ItemType.File)
            {
                var image = Image.FromResource("MonoDevelop.VersionControl.TFS.Icons.text-file-16.png");
                return image;
            }
            else
            {
                var image = Image.FromResource("MonoDevelop.VersionControl.TFS.Icons.open-folder-16.png");
                return image;
            }
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
                _listStore.SetValue(row, _iconList, GetItemImage(item.ItemType));
                _listStore.SetValue(row, _nameList, ((VersionControlPath)item.TargetServerItem).ItemName);
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
                if (!IsMapped(item.ServerPath))
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

        int treeLevel = 0;

        private void AddChilds(TreeNavigator node, List<HierarchyItem> children)
        {
            treeLevel++;
            foreach (var child in children)
            {
                var childNode = node.AddChild().SetValue(_nameTree, child.Name).SetValue(_itemTree, child.Item);
                if (treeLevel == 1)
                    childNode.SetValue(_iconTree, GetRepositoryImage());
                else
                    childNode.SetValue(_iconTree, GetItemImage(ItemType.Folder));
                AddChilds(node, child.Children);
                node.MoveToParent();
            }
            treeLevel--;
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

        private void ShowMappingPath(VersionControlPath serverPath)
        {
            if (!IsMapped(serverPath))
            {
                _localFolder.Text = GettextCatalog.GetString("Not Mapped");
                return;
            }
            var mappedFolder = _currentWorkspace.Folders.First(f => serverPath.IsChildOrEqualTo(f.ServerItem));
            if (string.Equals(serverPath, mappedFolder.ServerItem, StringComparison.Ordinal))
                _localFolder.Text = mappedFolder.LocalItem;
            else
            {
                string rest = serverPath.ChildPart(mappedFolder.ServerItem); //serverPath.Substring(mappedFolder.ServerItem.Length + 1);
                _localFolder.Text = Path.Combine(mappedFolder.LocalItem, rest);
            }
        }

        #region Events

        private void OnChangeActiveWorkspaces(object sender, EventArgs ev)
        {
            if (_workspaceComboBox.SelectedIndex > -1)
            {
                var name = _workspaceStore.GetValue(_workspaceComboBox.SelectedIndex, _workspaceName);
                _currentWorkspace = _workspaces.Single(ws => string.Equals(ws.Name, name, StringComparison.Ordinal));
                TFSVersionControlService.Instance.SetActiveWorkspace(projectCollection, name);
                if (_treeView.SelectedRow != null)
                {
                    var currentItem = _treeStore.GetNavigatorAt(_treeView.SelectedRow).GetValue(_itemTree).ServerItem;
                    ShowMappingPath(currentItem);
                    FillListView(currentItem);
                }
            }
            else
            {
                TFSVersionControlService.Instance.SetActiveWorkspace(projectCollection, string.Empty);
            }
        }

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

        private Menu BuildPopupMenu()
        {
            Menu menu = new Menu();
            var items = new List<ExtendedItem>();
            foreach (var row in _listView.SelectedRows)
            {
                items.Add(_listStore.GetValue(row, _itemList));
            }

            if (items.All(i => IsMapped(i.ServerPath)))
            {
                foreach (var item in GetGroup(items))
                {
                    menu.Items.Add(item);
                }
                var editGroup = EditGroup(items);
                if (editGroup.Any())
                {
                    menu.Items.Add(new SeparatorMenuItem());
                    foreach (var item in editGroup)
                    {
                        menu.Items.Add(item);
                    }
                }
                if (items.Count == 1 && items[0].ItemType == ItemType.Folder)
                {
                    menu.Items.Add(new SeparatorMenuItem());
                    menu.Items.Add(CreateOpenFolderMenuItem(items[0]));
                }
            }
            else
            {
                foreach (var item in NotMappedMenu(items))
                {
                    menu.Items.Add(item);
                }
            }
            return menu;
        }

        private List<MenuItem> GetGroup(List<ExtendedItem> items)
        {
            var groupItems = new List<MenuItem>();
            MenuItem getLatestVersionItem = new MenuItem(GettextCatalog.GetString("Get Latest Version"));
            getLatestVersionItem.Clicked += (sender, e) => GetLatestVersion(items);
            groupItems.Add(getLatestVersionItem);

            MenuItem forceGetLatestVersionItem = new MenuItem(GettextCatalog.GetString("Get Specific Version"));
            forceGetLatestVersionItem.Clicked += (sender, e) => ForceGetLatestVersion(items);
            groupItems.Add(forceGetLatestVersionItem);
            return groupItems;
        }

        private List<MenuItem> EditGroup(List<ExtendedItem> items)
        {
            var groupItems = new List<MenuItem>();
            var itemsWithChages = items.Where(i => i.ChangeType != ChangeType.None).ToList();
            if (itemsWithChages.Any())
            {
                MenuItem revertItem = new MenuItem(GettextCatalog.GetString("Undo Changes"));
                revertItem.Clicked += (sender, e) => UndoChanges(itemsWithChages);
                groupItems.Add(revertItem);
            }
            return groupItems;
        }

        private List<MenuItem> NotMappedMenu(List<ExtendedItem> items)
        {
            MenuItem mapItem = new MenuItem(GettextCatalog.GetString("Map"));
            mapItem.Clicked += (sender, e) => MapItem(items);
            return new List<MenuItem> { mapItem };
        }

        private MenuItem CreateOpenFolderMenuItem(ExtendedItem item)
        {
            MenuItem openFolder = new MenuItem(GettextCatalog.GetString("Open mapped folder"));
            openFolder.Clicked += (sender, e) =>
            {
                var path = item.LocalItem;
                if (string.IsNullOrEmpty(path))
                    path = _currentWorkspace.TryGetLocalItemForServerItem(item.ServerPath);
                DesktopService.OpenFolder(path);
            };
            return openFolder;
        }

        private void MapItem(List<ExtendedItem> items)
        {
            var item = items.FirstOrDefault(i => i.ItemType == ItemType.Folder);
            if (_currentWorkspace == null || item == null)
                return;
            using (SelectFolderDialog folderSelect = new SelectFolderDialog("Browse For Folder"))
            {
                folderSelect.Multiselect = false;
                folderSelect.CanCreateFolders = true;
                if (folderSelect.Run())
                {
                    _currentWorkspace.Map(item.ServerPath, folderSelect.Folder);
                }
                RefreshList(items);
            }
        }

        private void RefreshList(List<ExtendedItem> items)
        {
            if (items.Any())
                FillListView(items[0].ServerPath.ParentPath); 
        }

        private void GetLatestVersion(List<ExtendedItem> items)
        {
            List<GetRequest> requests = new List<GetRequest>();
            foreach (var item in items)
            {
                RecursionType recursion = item.ItemType == ItemType.File ? RecursionType.None : RecursionType.Full;   
                requests.Add(new GetRequest(item.ServerPath, recursion, VersionSpec.Latest)); 
            }
            _currentWorkspace.Get(requests, GetOptions.None);
            RefreshList(items);
        }

        private void ForceGetLatestVersion(List<ExtendedItem> items)
        {
            using (var specVersionDialog = new GetSpecVersionDialog(_currentWorkspace))
            {
                specVersionDialog.AddItems(items);
                if (specVersionDialog.Run(this.Widget.ParentWindow) == Command.Ok)
                {
                    RefreshList(items);
                }
            }               
        }

        private void UndoChanges(List<ExtendedItem> items)
        {
            var specs = items.Select(i => new ItemSpec(i.LocalItem, i.ItemType == ItemType.File ? RecursionType.None : RecursionType.Full)).ToList();
            _currentWorkspace.Undo(specs);
            RefreshList(items);
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