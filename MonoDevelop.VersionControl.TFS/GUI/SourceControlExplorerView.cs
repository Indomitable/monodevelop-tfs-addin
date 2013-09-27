using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using MonoDevelop.Ide.Gui;
using Xwt;
using MonoDevelop.VersionControl.TFS.Helpers;

namespace MonoDevelop.VersionControl.TFS.GUI
{
    public class SourceControlExplorerView : AbstractXwtViewContent
    {
        readonly VBox _view;
        TreeView _treeView;
        ListView _listView;
        readonly MonoDevelop.Core.IProgressMonitor _monitor;

        public SourceControlExplorerView()
        {
            _view = new VBox();    
            BuildContent();
        }

        public SourceControlExplorerView(MonoDevelop.Core.IProgressMonitor monitor)
            : this()
        {
            _monitor = monitor;
        }

        #region implemented abstract members of AbstractViewContent

        public override void Load(string fileName)
        {
            LoadTreeView(new Uri(fileName));
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
            _treeView = new TreeView();
            _treeView.MinWidth = 300;
            box.PackStart(_treeView);
            _listView = new ListView();
            box.PackStart(_listView, true, true);

            _view.PackStart(box, true, true);
        }

        private void LoadTreeView(Uri serverUrl)
        {
            NetworkCredential credentials = UserInfoHelper.ExtractCredentials(serverUrl);
            using (var tfsServer = new TeamFoundationServer(serverUrl, credentials))
            {
                if (_monitor != null)
                    _monitor.Log.Write("Authenticating ...");

                tfsServer.Authenticate();

                if (_monitor != null)
                    _monitor.Log.Write("Loading ...");

                var versionControl = (VersionControlServer)tfsServer.GetService(typeof(VersionControlServer));
                var itemSet = versionControl.GetItems(new ItemSpec(VersionControlPath.RootFolder, RecursionType.Full), VersionSpec.Latest, DeletedState.NonDeleted, ItemType.Folder, false);

                if (_monitor != null)
                    _monitor.BeginTask(null, itemSet.Items.Length);

                DataField<string> name = new DataField<string>();
                DataField<string> path = new DataField<string>();

                TreeStore store = new TreeStore(name, path);
                var root = ItemSetToHierarchItemConverter.Convert(itemSet.Items);

                var node = store.AddNode().SetValue(name, root.Name).SetValue(path, root.ServerPath);
                UpdateProgress();
                AddChilds(name, path, node, root.Children);

                _treeView.Columns.Add(new ListViewColumn("Name", new TextCellView(name) { Editable = false }));
                //_treeView.Columns.Add(new ListViewColumn("Path", new TextCellView(path) { Editable = false, Visible = false }));
                _treeView.DataSource = store;


                if (_monitor != null)
                    _monitor.EndTask();
            }
        }

        private void AddChilds(DataField<string> name, DataField<string> path, TreeNavigator node, List<HierarchyItem> children)
        {
            foreach (var child in children)
            {
                UpdateProgress();
                node.AddChild().SetValue(name, child.Name).SetValue(path, child.ServerPath);
                AddChilds(name, path, node, child.Children);
                node.MoveToParent();
            }
        }

        private void UpdateProgress()
        {
            if (_monitor != null)
            {
                _monitor.Step(1);
            }
        }
    }
}
