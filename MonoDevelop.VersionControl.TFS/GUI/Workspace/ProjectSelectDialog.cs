//
// ProjectSelectDialog.cs
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

using Xwt;
using MonoDevelop.Core;
using Microsoft.TeamFoundation.VersionControl.Client;
using MonoDevelop.VersionControl.TFS.Infrastructure.Objects;
using System.Collections.Generic;
using Microsoft.TeamFoundation.VersionControl.Client.Objects;
using Microsoft.TeamFoundation.VersionControl.Client.Enums;

namespace MonoDevelop.VersionControl.TFS.GUI
{
    public class ProjectSelectDialog : Dialog
    {
        private readonly DataField<string> _name = new DataField<string>();
        private readonly DataField<string> _path = new DataField<string>();
        private readonly TreeStore _treeStore;
        private readonly TreeView treeView = new TreeView();
        private readonly Microsoft.TeamFoundation.Client.ProjectCollection projectCollection;

        public ProjectSelectDialog(Microsoft.TeamFoundation.Client.ProjectCollection projectCollection)
        {
            this.projectCollection = projectCollection;
            _treeStore = new TreeStore(_name, _path);

            BuildGui();
            FillTreeView();
        }

        void BuildGui()
        {
            this.Title = GettextCatalog.GetString("Browse for Folder");
            //this.Resizable = false;
            VBox content = new VBox();
            content.PackStart(new Label(GettextCatalog.GetString("Team Foundation Server") + ":"));
            content.PackStart(new TextEntry { Text = projectCollection.Server.Name + " - " + projectCollection.Name, Sensitive = false, MinWidth = 300 });

            content.PackStart(new Label(GettextCatalog.GetString("Folders") + ":"));

            treeView.Columns.Add(new ListViewColumn("Name", new TextCellView(_name) { Editable = false }));
            treeView.DataSource = _treeStore;
            treeView.MinWidth = 300;
            treeView.MinHeight = 300;
            content.PackStart(treeView, true, true);
                
            content.PackStart(new Label(GettextCatalog.GetString("Folder path") + ":"));

            TextEntry folderPathEntry = new TextEntry();
            folderPathEntry.Sensitive = false;

            treeView.SelectionChanged += (sender, e) => folderPathEntry.Text = this.SelectedPath;
            content.PackStart(folderPathEntry);

            HBox buttonBox = new HBox();

            Button nextButton = new Button(GettextCatalog.GetString("Next"));
            nextButton.MinWidth = Constants.ButtonWidth;
            nextButton.Clicked += (sender, e) => Respond(Command.Ok);
            buttonBox.PackStart(nextButton);

            Button cancelButton = new Button(GettextCatalog.GetString("Cancel"));
            cancelButton.MinWidth = Constants.ButtonWidth;
            cancelButton.Clicked += (sender, e) => Respond(Command.Cancel);
            buttonBox.PackEnd(cancelButton);

            content.PackStart(buttonBox);

            this.Content = content;
        }

        public void FillTreeView()
        {
            _treeStore.Clear();
            var versionControl = this.projectCollection.GetService<RepositoryService>();
            var items = versionControl.QueryItems(new ItemSpec(VersionControlPath.RootFolder, RecursionType.Full), VersionSpec.Latest, DeletedState.NonDeleted, ItemType.Folder, false);
            var root = ItemSetToHierarchItemConverter.Convert(items);
            var node = _treeStore.AddNode().SetValue(_name, root.Name).SetValue(_path, root.ServerPath);
            AddChilds(node, root.Children);
            var topNode = _treeStore.GetFirstNode();
            treeView.ExpandRow(topNode.CurrentPosition, false);
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

        public string SelectedPath
        {
            get
            {
                if (treeView.SelectedRow == null)
                    return string.Empty;
                var node = _treeStore.GetNavigatorAt(treeView.SelectedRow);
                return node.GetValue(_path);
            }
        }
    }
}
