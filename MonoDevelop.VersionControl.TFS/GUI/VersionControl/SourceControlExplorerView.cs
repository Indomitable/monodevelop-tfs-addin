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
using MonoDevelop.VersionControl.TFS.Helpers;
using MonoDevelop.VersionControl.TFS.Infrastructure.Objects;
using MonoDevelop.Ide;
using MonoDevelop.Core;
using System.Linq;
using System.IO;
using Microsoft.TeamFoundation.VersionControl.Client.Objects;
using Microsoft.TeamFoundation.VersionControl.Client.Enums;
using MonoDevelop.VersionControl.TFS.GUI.Workspace;
using Microsoft.TeamFoundation.Client;
using Gtk;
using Gdk;
using MonoDevelop.VersionControl.TFS.GUI.VersionControl.Dialogs;

namespace MonoDevelop.VersionControl.TFS.GUI.VersionControl
{
    public class SourceControlExplorerView : AbstractViewContent
    {
        private readonly VBox _view = new VBox();
        private readonly Label _localFolder = new Label();
        private readonly TreeView _listView = new TreeView();
        private readonly ListStore _listStore = new ListStore(typeof(ExtendedItem), typeof(Pixbuf), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string));
        private readonly ComboBox _workspaceComboBox = new ComboBox();
        private readonly ListStore _workspaceStore = new ListStore(typeof(Microsoft.TeamFoundation.VersionControl.Client.Workspace), typeof(string));
        private readonly Button manageButton = new Button(GettextCatalog.GetString("Manage"));
        private readonly Button refreshButton = new Button(GettextCatalog.GetString("Refresh"));
        private readonly TreeView _treeView = new TreeView();
        private readonly TreeStore _treeStore = new TreeStore(typeof(BaseItem), typeof(Pixbuf), typeof(string));
        private ProjectCollection projectCollection;
        private readonly List<Microsoft.TeamFoundation.VersionControl.Client.Workspace> _workspaces = new List<Microsoft.TeamFoundation.VersionControl.Client.Workspace>();
        private Microsoft.TeamFoundation.VersionControl.Client.Workspace _currentWorkspace;

        public SourceControlExplorerView()
        {
            ContentName = GettextCatalog.GetString("Source Explorer");
            BuildGui();
        }

        public static void Open(ProjectInfo project)
        {
            foreach (var view in IdeApp.Workbench.Documents)
            {
                var sourceDoc = view.GetContent<SourceControlExplorerView>();
                if (sourceDoc != null)
                {
                    sourceDoc.Load(project.Collection);
                    sourceDoc.ExpandPath(VersionControlPath.RootFolder + project.Name);
                    view.Window.SelectWindow();
                    return;
                }
            }

            var sourceControlExplorerView = new SourceControlExplorerView();
            sourceControlExplorerView.Load(project.Collection);
            sourceControlExplorerView.ExpandPath(VersionControlPath.RootFolder + project.Name);
            IdeApp.Workbench.OpenDocument(sourceControlExplorerView, true);
        }

        public static void Open(ProjectCollection collection)
        {
            Open(collection, VersionControlPath.RootFolder, null);
        }

        public static void Open(ProjectCollection collection, string path, string fileName)
        {
            foreach (var view in IdeApp.Workbench.Documents)
            {
                var sourceDoc = view.GetContent<SourceControlExplorerView>();
                if (sourceDoc != null)
                {
                    sourceDoc.Load(collection);
                    sourceDoc.ExpandPath(path);
                    sourceDoc.FindListItem(fileName);
                    view.Window.SelectWindow();
                    return;
                }
            }

            var sourceControlExplorerView = new SourceControlExplorerView();
            sourceControlExplorerView.Load(collection);
            sourceControlExplorerView.ExpandPath(path);
            sourceControlExplorerView.FindListItem(fileName);
            IdeApp.Workbench.OpenDocument(sourceControlExplorerView, true);
        }

        public override void Load(string fileName)
        {
            throw new NotSupportedException();
        }

        private void Load(ProjectCollection collection)
        {
            if (this.projectCollection != null && string.Equals(collection.Id, this.projectCollection.Id, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            projectCollection = collection;
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

        public override Widget Control { get { return _view; } }

        private void BuildGui()
        {
            HBox headerBox = new HBox();
            headerBox.PackStart(new Label(GettextCatalog.GetString("Workspace") + ":"), false, false, 0);

            _workspaceComboBox.Model = _workspaceStore;
            var workspaceTextRenderer = new CellRendererText();
            _workspaceComboBox.PackStart(workspaceTextRenderer, true);
            _workspaceComboBox.SetAttributes(workspaceTextRenderer, "text", 1);

            headerBox.PackStart(_workspaceComboBox, false, false, 0);
            headerBox.PackStart(manageButton, false, false, 0);
            headerBox.PackStart(refreshButton, false, false, 0);
            _view.PackStart(headerBox, false, false, 0);

            HPaned mainBox = new HPaned();

            VBox treeViewBox = new VBox();

            TreeViewColumn treeColumn = new TreeViewColumn();
            treeColumn.Title = "Folders";
            var repoImageRenderer = new CellRendererPixbuf();
            treeColumn.PackStart(repoImageRenderer, false);
            treeColumn.SetAttributes(repoImageRenderer, "pixbuf", 1);
            var folderTextRenderer = new CellRendererText();
            treeColumn.PackStart(folderTextRenderer, true);
            treeColumn.SetAttributes(folderTextRenderer, "text", 2);
            _treeView.AppendColumn(treeColumn);

            treeViewBox.WidthRequest = 250;
            ScrolledWindow scrollContainer = new ScrolledWindow();
            scrollContainer.Add(_treeView);
            treeViewBox.PackStart(scrollContainer, true, true, 0);
            mainBox.Pack1(treeViewBox, false, false);


            VBox rightBox = new VBox();
            HBox headerRightBox = new HBox();

            headerRightBox.PackStart(new Label(GettextCatalog.GetString("Local Path") + ":"), false, false, 0);
            Alignment leftAlign = new Alignment(0, 0, 0, 0);
            _localFolder.Justify = Justification.Left;
            leftAlign.Add(_localFolder);
            headerRightBox.PackStart(leftAlign);
            rightBox.PackStart(headerRightBox, false, false, 0);

            var itemNameColumn = new TreeViewColumn();
            itemNameColumn.Title = "Name";
            var itemIconRenderer = new CellRendererPixbuf();
            itemNameColumn.PackStart(itemIconRenderer, false);
            itemNameColumn.SetAttributes(itemIconRenderer, "pixbuf", 1);
            var itemNameRenderer = new CellRendererText();
            itemNameColumn.PackStart(itemNameRenderer, true);
            itemNameColumn.SetAttributes(itemNameRenderer, "text", 2);
            _listView.AppendColumn(itemNameColumn);

            _listView.AppendColumn("Pending Change", new CellRendererText(), "text", 3);
            _listView.AppendColumn("User", new CellRendererText(), "text", 4);
            _listView.AppendColumn("Latest", new CellRendererText(), "text", 5);
            _listView.AppendColumn("Last Check-in", new CellRendererText(), "text", 6);

            _listView.Selection.Mode = SelectionMode.Multiple;
            _listView.Model = _listStore;
            var listViewScollWindow = new ScrolledWindow();
            listViewScollWindow.Add(_listView);
            rightBox.PackStart(listViewScollWindow, true, true, 0);
            mainBox.Pack2(rightBox, true, true);
            _view.PackStart(mainBox, true, true, 0);
            AttachEvents();
            _view.ShowAll();
        }

        private void AttachEvents()
        {
            _workspaceComboBox.Changed += OnChangeActiveWorkspaces;
            manageButton.Clicked += OnManageWorkspaces;
            refreshButton.Clicked += OnRefresh;
            _treeView.Selection.Changed += OnFolderChanged;
            _treeView.RowActivated += OnTreeViewItemClicked;
            _listView.RowActivated += OnListItemClicked;
            _listView.ButtonPressEvent += OnListViewMouseClick;
        }

        private void FillWorkspaces()
        {
            string activeWorkspace = TFSVersionControlService.Instance.GetActiveWorkspace(projectCollection);
            _workspaceComboBox.Changed -= OnChangeActiveWorkspaces;
            _workspaceStore.Clear();
            _workspaces.Clear();
            _workspaces.AddRange(WorkspaceHelper.GetLocalWorkspaces(projectCollection));
            TreeIter activeWorkspaceRow = TreeIter.Zero;
            foreach (var workspace in _workspaces)
            {
                var iter = _workspaceStore.AppendValues(workspace, workspace.Name);
                if (string.Equals(workspace.Name, activeWorkspace, StringComparison.Ordinal))
                {
                    activeWorkspaceRow = iter;
                }
            }
            _workspaceComboBox.Changed += OnChangeActiveWorkspaces;
            if (_workspaces.Count > 0)
            {
                if (!activeWorkspaceRow.Equals(TreeIter.Zero))
                    _workspaceComboBox.SetActiveIter(activeWorkspaceRow);
                else
                    _workspaceComboBox.Active = 0;
            }
        }

        private static Pixbuf GetRepositoryImage()
        {
            return new Pixbuf(System.Reflection.Assembly.GetCallingAssembly(), "MonoDevelop.VersionControl.TFS.Icons.repository.png", 16, 16);
        }

        private void FillTreeView()
        {
            _treeStore.Clear();
            var versionControl = projectCollection.GetService<RepositoryService>();
            var items = versionControl.QueryItems(this._currentWorkspace, new ItemSpec(VersionControlPath.RootFolder, RecursionType.Full), VersionSpec.Latest, DeletedState.NonDeleted, ItemType.Folder, false);

            var root = ItemSetToHierarchItemConverter.Convert(items);
            var node = _treeStore.AppendNode();
            _treeStore.SetValues(node, root.Item, GetRepositoryImage(), root.Name);
            AddChilds(node, root.Children);
            TreeIter firstNode;
            if (_treeStore.GetIterFirst(out firstNode))
            {
                _treeView.ExpandRow(_treeStore.GetPath(firstNode), false);
                _treeView.Selection.SelectIter(firstNode);
            }
            _treeView.Model = _treeStore;
        }

        int treeLevel;

        private void AddChilds(TreeIter node, List<HierarchyItem> children)
        {
            treeLevel++;
            foreach (var child in children)
            {
                var childNode = _treeStore.AppendNode(node);
                _treeStore.SetValue(childNode, 0, child.Item);
                _treeStore.SetValue(childNode, 2, child.Name);
                if (treeLevel == 1)
                    _treeStore.SetValue(childNode, 1, GetRepositoryImage());
                else
                    _treeStore.SetValue(childNode, 1, GetItemImage(ItemType.Folder));
                AddChilds(childNode, child.Children);
            }
            treeLevel--;
        }

        private static Pixbuf GetItemImage(ItemType itemType)
        {
            if (itemType == ItemType.File)
            {
                return new Pixbuf(System.Reflection.Assembly.GetCallingAssembly(), "MonoDevelop.VersionControl.TFS.Icons.text-file-16.png");
            }
            else
            {
                return new Pixbuf(System.Reflection.Assembly.GetCallingAssembly(), "MonoDevelop.VersionControl.TFS.Icons.open-folder-16.png");
            }
        }

        private void FillListView(string serverPath)
        {
            _listStore.Clear();

            var versionControl = projectCollection.GetService<RepositoryService>();
            var itemSet = versionControl.QueryItemsExtended(this._currentWorkspace, new ItemSpec(serverPath, RecursionType.OneLevel), DeletedState.NonDeleted, ItemType.Any);
            foreach (var item in itemSet.Skip(1).OrderBy(i => i.ItemType).ThenBy(i => i.TargetServerItem))
            {
                var row = _listStore.Append();
                _listStore.SetValue(row, 0, item);
                _listStore.SetValue(row, 1, GetItemImage(item.ItemType));
                _listStore.SetValue(row, 2, item.ServerPath.ItemName);
                if (this._currentWorkspace != null)
                {
                    if (item.ChangeType != ChangeType.None && !item.HasOtherPendingChange)
                    {
                        _listStore.SetValue(row, 3, item.ChangeType.ToString());
                        _listStore.SetValue(row, 4, this._currentWorkspace.OwnerName);
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
                        if (!item.ChangeType.HasFlag(ChangeType.None))
                        {
                            userNames.Insert(0, this._currentWorkspace.OwnerName);
                            changeNames.Insert(0, item.ChangeType.ToString());
                        }
                        _listStore.SetValue(row, 3, string.Join(", ", changeNames));
                        _listStore.SetValue(row, 4, string.Join(", ", userNames));
                    }
                }
                if (!IsMapped(item.ServerPath))
                {
                    _listStore.SetValue(row, 5, "Not mapped");
                }
                else
                {
                    if (!item.IsInWorkspace)
                    {
                        _listStore.SetValue(row, 5, "Not downloaded");
                    }
                    else
                    {
                        _listStore.SetValue(row, 5, item.IsLatest ? "Yes" : "No");
                    }
                }
                _listStore.SetValue(row, 6, item.CheckinDate.ToString("yyyy-MM-dd HH:mm"));
            }
        }

        private void ExpandPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;
            TreeIter iter = TreeIter.Zero;
            _treeStore.Foreach((m, p, i) =>
            {
                var item = ((BaseItem)m.GetValue(i, 0));
                if (string.Equals(item.ServerPath, path, StringComparison.OrdinalIgnoreCase))
                {
                    iter = i;
                    return true;
                }
                return false;
            });

            if (iter.Equals(TreeIter.Zero))
                return;
            _treeView.CollapseAll();
            _treeView.ExpandToPath(_treeStore.GetPath(iter));
            _treeView.Selection.SelectIter(iter);
        }

        private void FindListItem(string name)
        {
            if (string.IsNullOrEmpty(name))
                return;
            TreeIter iter = TreeIter.Zero;
            _listStore.Foreach((model, path, it) =>
            {
                var item = ((BaseItem)model.GetValue(it, 0));
                if (string.Equals(item.ServerPath.ItemName, name, StringComparison.OrdinalIgnoreCase))
                {
                    iter = it;
                    return true;
                }
                return false;
            });
            if (iter.Equals(TreeIter.Zero))
                return;
            _listView.Selection.SelectIter(iter);
            var treePath = _listStore.GetPath(iter);
            _listView.ScrollToCell(treePath, _listView.Columns[0], false, 0, 0);
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
                string rest = serverPath.ChildPart(mappedFolder.ServerItem);
                _localFolder.Text = Path.Combine(mappedFolder.LocalItem, rest);
            }
        }

        #region Events

        private void OnChangeActiveWorkspaces(object sender, EventArgs ev)
        {
            TreeIter workspaceIter;
            if (_workspaceComboBox.GetActiveIter(out workspaceIter))
            {
                var workspace = (Microsoft.TeamFoundation.VersionControl.Client.Workspace)_workspaceStore.GetValue(workspaceIter, 0);
                _currentWorkspace = workspace;
                TFSVersionControlService.Instance.SetActiveWorkspace(projectCollection, workspace.Name);
                TreeIter treeIter;
                if (_treeView.Selection.GetSelected(out treeIter))
                {
                    var currentItem = (BaseItem)_treeStore.GetValue(treeIter, 0);
                    ShowMappingPath(currentItem.ServerPath);
                    FillListView(currentItem.ServerPath);
                }
            }
            else
            {
                TFSVersionControlService.Instance.SetActiveWorkspace(projectCollection, string.Empty);
            }
        }

        private void OnFolderChanged(object sender, EventArgs e)
        {
            TreeIter iter;
            if (!_treeView.Selection.GetSelected(out iter))
                return;
        
            var item = (BaseItem)_treeStore.GetValue(iter, 0);
            FillListView(item.ServerPath);
            ShowMappingPath(item.ServerPath);
        }

        private void OnManageWorkspaces(object sender, EventArgs e)
        {
            using (var dialog = new WorkspacesDialog(projectCollection))
            {
                if (dialog.Run() == Xwt.Command.Close)
                {
                    FillWorkspaces();
                }
            }
        }

        void OnRefresh(object sender, EventArgs e)
        {
            TreeIter iter;
            string selectedPath = string.Empty;
            if (_treeView.Selection.GetSelected(out iter))
                selectedPath = ((BaseItem)_treeStore.GetValue(iter, 0)).ServerPath;
            FillTreeView();
            if (!string.IsNullOrEmpty(selectedPath))
                ExpandPath(selectedPath);
        }

        private string DownloadItemToTemp(ExtendedItem extendedItem)
        {
            var dowloadService = this.projectCollection.GetService<VersionControlDownloadService>();
            var item = _currentWorkspace.GetItem(extendedItem.ServerPath, ItemType.File, true);
            var filePath = dowloadService.DownloadToTempWithName(item.ArtifactUri, item.ServerPath.ItemName);
            return filePath;
        }

        private void OnListItemClicked(object sender, RowActivatedArgs e)
        {
            TreeIter iter;
            if (!_listStore.GetIter(out iter, e.Path))
                return;

            var item = (ExtendedItem)_listStore.GetValue(iter, 0);
            if (item.ItemType == ItemType.Folder)
            {
                ExpandPath(item.TargetServerItem);
                return;
            }
            if (item.ItemType == ItemType.File)
            {
                if (IsMapped(item.ServerPath))
                {
                    if (item.IsInWorkspace)
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
                    else
                    {
                        var filePath = this.DownloadItemToTemp(item);
                        if (MonoDevelop.Projects.Services.ProjectService.IsWorkspaceItemFile(filePath))
                        {
                            var parentFolder = _currentWorkspace.GetExtendedItem(item.ServerPath.ParentPath, ItemType.Folder);
                            if (parentFolder == null)
                                return;
                            GetLatestVersion(new List<ExtendedItem> { parentFolder });
                            var futurePath = _currentWorkspace.GetLocalPathForServerPath(item.ServerPath);
                            IdeApp.Workspace.OpenWorkspaceItem(futurePath, true);
                        }
                        FileHelper.FileDelete(filePath);
                    }
                }
                else
                {
                    var filePath = this.DownloadItemToTemp(item);
                    IdeApp.Workbench.OpenDocument(filePath, null, true);
                }
            }
        }

        void OnTreeViewItemClicked(object o, RowActivatedArgs args)
        {
            var isExpanded = _treeView.GetRowExpanded(args.Path);
            if (isExpanded)
                _treeView.CollapseRow(args.Path);
            else
                _treeView.ExpandRow(args.Path, false);
        }

        #endregion

        #region Popup Menu

        [GLib.ConnectBefore]
        private void OnListViewMouseClick(object o, ButtonPressEventArgs args)
        {
            if (args.Event.Button == 3 && _listView.Selection.GetSelectedRows().Any())
            {
                var menu = BuildPopupMenu();
                if (menu.Children.Length > 0)
                    menu.Popup();
                args.RetVal = true;
            }
        }

        private Menu BuildPopupMenu()
        {
            Menu menu = new Menu();
            var items = new List<ExtendedItem>();
            foreach (var path in _listView.Selection.GetSelectedRows())
            {
                TreeIter iter;
                _listStore.GetIter(out iter, path);
                items.Add((ExtendedItem)_listStore.GetValue(iter, 0));
            }
        
            if (items.All(i => IsMapped(i.ServerPath)))
            {
                foreach (var item in GetGroup(items))
                {
                    menu.Add(item);
                }
                var editGroup = EditGroup(items);
                if (editGroup.Any())
                {
                    menu.Add(new SeparatorMenuItem());
                    foreach (var item in editGroup)
                    {
                        menu.Add(item);
                    }
                }
                if (items.Count == 1 && items[0].ItemType == ItemType.Folder)
                {
                    menu.Add(new SeparatorMenuItem());
                    menu.Add(CreateOpenFolderMenuItem(items[0]));
                }
            }
            else
            {
                foreach (var item in NotMappedMenu(items))
                {
                    menu.Add(item);
                }
            }
            menu.ShowAll();
            return menu;
        }

        private List<MenuItem> GetGroup(List<ExtendedItem> items)
        {
            var groupItems = new List<MenuItem>();
            MenuItem getLatestVersionItem = new MenuItem(GettextCatalog.GetString("Get Latest Version"));
            getLatestVersionItem.Activated += (sender, e) => GetLatestVersion(items);
            groupItems.Add(getLatestVersionItem);
        
            MenuItem forceGetLatestVersionItem = new MenuItem(GettextCatalog.GetString("Get Specific Version"));
            forceGetLatestVersionItem.Activated += (sender, e) => ForceGetLatestVersion(items);
            groupItems.Add(forceGetLatestVersionItem);
            return groupItems;
        }

        private List<MenuItem> EditGroup(List<ExtendedItem> items)
        {
            var groupItems = new List<MenuItem>();
            //Check Out
            var checkOutItems = items.Where(i => i.ChangeType == ChangeType.None || i.ChangeType == ChangeType.Lock || i.ItemType == ItemType.Folder).ToList();
            if (checkOutItems.Any())
            {
                MenuItem checkOutItem = new MenuItem(GettextCatalog.GetString("Check out items"));
                checkOutItem.Activated += (sender, e) =>
                {
                    CheckOutDialog.Open(checkOutItems, _currentWorkspace);
                    FireFilesChanged(checkOutItems);
                    RefreshList(items);
                };
                groupItems.Add(checkOutItem);
            }
            //Lock
            var lockItems = items.Where(i => !i.IsLocked).ToList();
            if (lockItems.Any())
            {
                MenuItem lockItem = new MenuItem(GettextCatalog.GetString("Lock"));
                lockItem.Activated += (sender, e) =>
                {
                    LockDialog.Open(lockItems, _currentWorkspace);
                    FireFilesChanged(lockItems);
                    RefreshList(items);
                };
                groupItems.Add(lockItem);
            }
            //UnLock
            var unLockItems = items.Where(i => i.IsLocked && !i.HasOtherPendingChange).ToList();
            if (unLockItems.Any())
            {
                MenuItem unLockItem = new MenuItem(GettextCatalog.GetString("UnLock"));
                unLockItem.Activated += (sender, e) =>
                {
                    var folders = new List<string>(unLockItems.Where(i => i.ItemType == ItemType.Folder).Select(i => (string)i.ServerPath));
                    var files = new List<string>(unLockItems.Where(i => i.ItemType == ItemType.File).Select(i => (string)i.ServerPath));
                    _currentWorkspace.LockFolders(folders, LockLevel.None);
                    _currentWorkspace.LockFiles(files, LockLevel.None);
                    FireFilesChanged(unLockItems);
                    RefreshList(items);
                };
                groupItems.Add(unLockItem);
            }
            //Rename
            var ableToRename = items.FirstOrDefault(i => !i.ChangeType.HasFlag(ChangeType.Delete));
            if (ableToRename != null)
            {
                MenuItem renameItem = new MenuItem(GettextCatalog.GetString("Rename"));
                renameItem.Activated += (sender, e) =>
                {
                    RenameDialog.Open(ableToRename, _currentWorkspace);
                    FireFilesChanged(new List<ExtendedItem> { ableToRename });
                    RefreshList(items);
                };
                groupItems.Add(renameItem);
            }
            //Delete
            var ableToDelete = items.Where(i => !i.ChangeType.HasFlag(ChangeType.Delete)).ToList();
            if (ableToDelete.Any())
            {
                MenuItem deleteItem = new MenuItem(GettextCatalog.GetString("Delete"));
                deleteItem.Activated += (sender, e) => DeleteItems(ableToDelete);
                groupItems.Add(deleteItem);
            }
            //Undo
            var undoItems = items.Where(i => !i.ChangeType.HasFlag(ChangeType.None) || i.ItemType == ItemType.Folder).ToList();
            if (undoItems.Any())
            {
                MenuItem revertItem = new MenuItem(GettextCatalog.GetString("Undo Changes"));
                revertItem.Activated += (sender, e) =>
                {
                    UndoDialog.Open(undoItems, _currentWorkspace);
                    FireFilesChanged(undoItems);
                    RefreshList(items);
                };
                groupItems.Add(revertItem);
            }
            return groupItems;
        }

        private List<MenuItem> NotMappedMenu(List<ExtendedItem> items)
        {
            MenuItem mapItem = new MenuItem(GettextCatalog.GetString("Map"));
            mapItem.Activated += (sender, e) => MapItem(items);
            return new List<MenuItem> { mapItem };
        }

        private MenuItem CreateOpenFolderMenuItem(ExtendedItem item)
        {
            MenuItem openFolder = new MenuItem(GettextCatalog.GetString("Open mapped folder"));
            openFolder.Activated += (sender, e) =>
            {
                var path = item.LocalItem;
                if (string.IsNullOrEmpty(path))
                    path = _currentWorkspace.GetLocalPathForServerPath(item.ServerPath);
                DesktopService.OpenFolder(path);
            };
            return openFolder;
        }

        private void MapItem(List<ExtendedItem> items)
        {
            var item = items.FirstOrDefault(i => i.ItemType == ItemType.Folder);
            if (_currentWorkspace == null || item == null)
                return;
            using (Xwt.SelectFolderDialog folderSelect = new Xwt.SelectFolderDialog("Browse For Folder"))
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

        private void FireFilesChanged(List<ExtendedItem> items)
        {
            TFSVersionControlService.Instance.RefreshWorkingRepositories();
            FileService.NotifyFilesChanged(items.Select(i => (FilePath)_currentWorkspace.GetLocalPathForServerPath(i.ServerPath)), true);
        }

        private void FireFilesRemoved(List<ExtendedItem> items)
        {
            TFSVersionControlService.Instance.RefreshWorkingRepositories();
            FileService.NotifyFilesRemoved(items.Select(i => (FilePath)_currentWorkspace.GetLocalPathForServerPath(i.ServerPath)));
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
            using (var progress = VersionControlService.GetProgressMonitor("Get", VersionControlOperationType.Pull))
            {
                var option = GetOptions.None;
                progress.Log.WriteLine("Start downloading items. GetOption: " + option);
                foreach (var request in requests)
                {
                    progress.Log.WriteLine(request);
                }
                _currentWorkspace.Get(requests, option, progress);
                progress.ReportSuccess("Finish Downloading.");
            }
            RefreshList(items);
        }

        private void ForceGetLatestVersion(List<ExtendedItem> items)
        {
            using (var specVersionDialog = new GetSpecVersionDialog(_currentWorkspace))
            {
                specVersionDialog.AddItems(items);
                if (specVersionDialog.Run() == Xwt.Command.Ok)
                {
                    RefreshList(items);
                }
            }
        }

        private void DeleteItems(List<ExtendedItem> items)
        {
            List<Failure> failures;
            _currentWorkspace.PendDelete(items.Select(x => (FilePath)x.LocalItem).ToList(), RecursionType.Full, out failures);
            if (failures.Any(f => f.SeverityType == SeverityType.Error))
                FailuresDisplayDialog.ShowFailures(failures);
            FireFilesRemoved(items);
            RefreshList(items);
        }

        #endregion
    }
}