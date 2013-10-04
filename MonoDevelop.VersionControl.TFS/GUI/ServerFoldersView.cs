//
// ServerFoldersView.cs
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
using MonoDevelop.VersionControl.TFS.Helpers;
using Microsoft.TeamFoundation.VersionControl.Client;
using MonoDevelop.VersionControl.TFS.Infrastructure.Objects;
using System.Collections.Generic;
using Microsoft.TeamFoundation.VersionControl.Common;

namespace MonoDevelop.VersionControl.TFS.GUI
{
    public class ServerFoldersView : IDisposable
    {
        private readonly DataField<string> _name = new DataField<string>();
        private readonly DataField<string> _path = new DataField<string>();
        private readonly TreeStore _treeStore;

        public ServerFoldersView()
        {
            TreeView = new TreeView();
            _treeStore = new TreeStore(_name, _path);
            TreeView.Columns.Add(new ListViewColumn("Name", new TextCellView(_name) { Editable = false }));
            TreeView.DataSource = _treeStore;
            TreeView.SelectionChanged += OnRowChanged;
        }

        private void OnRowChanged(object sender, EventArgs e)
        {
            if (OnChangePath != null)
                OnChangePath();
        }

        public void FillTreeView(ServerEntry server)
        {
            _treeStore.Clear();
            using (var tfsServer = TeamFoundationServerHelper.GetServer(server))
            {
                tfsServer.Authenticate();

                var versionControl = tfsServer.GetService<VersionControlServer>();
                var itemSet = versionControl.GetItems(new ItemSpec(VersionControlPath.RootFolder, RecursionType.Full), VersionSpec.Latest, DeletedState.NonDeleted, ItemType.Folder, false);

                var root = ItemSetToHierarchItemConverter.Convert(itemSet.Items);
                var node = _treeStore.AddNode().SetValue(_name, root.Name).SetValue(_path, root.ServerPath);
                AddChilds(node, root.Children);
                var topNode = _treeStore.GetFirstNode();
                TreeView.ExpandRow(topNode.CurrentPosition, false);
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
            TreeView.ExpandRow(node.CurrentPosition, false);
            TreeView.ScrollToRow(node.CurrentPosition);
            TreeView.SelectRow(node.CurrentPosition);
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

        public TreeView TreeView { get; set; }

        #region IDisposable implementation

        public void Dispose()
        {
            _treeStore.Dispose();
            TreeView.SelectionChanged -= OnRowChanged;
            TreeView.Dispose();
        }

        #endregion

        public event Action OnChangePath;

        public string SelectedPath
        {
            get
            {
                if (TreeView.SelectedRow == null)
                    return string.Empty;
                var node = _treeStore.GetNavigatorAt(TreeView.SelectedRow);
                return node.GetValue(_path);
            }
        }
    }
}

