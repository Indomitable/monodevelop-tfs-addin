using MonoDevelop.Ide.Gui;
using Xwt;
using MonoDevelop.Components.Commands;
using MonoDevelop.VersionControl.TFS.Commands;
using MonoDevelop.Ide;
using Microsoft.TeamFoundation.Client;
using MonoDevelop.VersionControl.TFS.Helpers;
using Microsoft.TeamFoundation.Server;
using System.Linq;
using MonoDevelop.Projects;

namespace MonoDevelop.VersionControl.TFS.GUI
{
    public class TeamExplorerPad : IPadContent
    {
        private enum NodeType
        {
            Server,
            Project,
            SourceControl,
            WorkItems
        }

        private readonly VBox _content = new VBox();
        private readonly TreeView _treeView = new TreeView();
        private readonly DataField<string> _name = new DataField<string>();
        private readonly DataField<NodeType> _type = new DataField<NodeType>();
        private readonly TreeStore _treeStore;
        private System.Action onServersChanged;

        public TeamExplorerPad()
        {
            _treeStore = new TreeStore(_name, _type);
        }

        #region IPadContent implementation

        public void Initialize(IPadWindow window)
        {
            var toolBar = window.GetToolbar(Gtk.PositionType.Top);
            CommandToolButton button = new CommandToolButton(TFSCommands.ConnectToServer, IdeApp.CommandService) { IconName = Gtk.Stock.Add };
            toolBar.Add(button);
            _treeView.Columns.Add(new ListViewColumn(string.Empty, new TextCellView(_name)));
            _treeView.DataSource = _treeStore;
            _content.PackStart(_treeView, true, true);
            UpdateData();

            onServersChanged = DispatchService.GuiDispatch<System.Action>(UpdateData);
            TFSVersionControlService.Instance.OnServersChange += onServersChanged;

            _treeView.RowActivated += OnRowClicked;
        }

        public void RedrawContent()
        {
            UpdateData();
        }

        public Gtk.Widget Control
        {
            get
            {
                return (Gtk.Widget)Toolkit.CurrentEngine.GetNativeWidget(_content);
            }
        }

        #endregion

        #region IDisposable implementation

        public void Dispose()
        {
            TFSVersionControlService.Instance.OnServersChange -= onServersChanged;
            _treeView.Dispose();
            _treeStore.Dispose();
            _content.Dispose();
        }

        #endregion

        private void UpdateData()
        {
            _treeStore.Clear();
            foreach (var server in TFSVersionControlService.Instance.Servers)
            {
                var node = _treeStore.AddNode().SetValue(_name, server.Name).SetValue(_type, NodeType.Server);

                var credentials = CredentialsManager.LoadCredential(server.Url);
                using (var tfsServer = TeamFoundationServerFactory.GetServer(server.Url, credentials))
                {
                    if (!tfsServer.HasAuthenticated)
                        tfsServer.Authenticate();
                    var projectService = tfsServer.GetService<ICommonStructureService>();
                    foreach (var projectInfo in projectService.ListProjects().OrderBy(x => x.Name))
                    {
                        node.AddChild().SetValue(_name, projectInfo.Name).SetValue(_type, NodeType.Project);
                        node.AddChild().SetValue(_name, "Work Items").SetValue(_type, NodeType.WorkItems);
                        node.MoveToParent();
                        node.AddChild().SetValue(_name, "Source Control").SetValue(_type, NodeType.SourceControl);
                        node.MoveToParent();
                        node.MoveToParent();
                    }
                }
            }
            ExpandServers();
        }

        private void ExpandServers()
        {
            var node = _treeStore.GetFirstNode();
            if (node == null)
                return;
            _treeView.ExpandRow(node.CurrentPosition, false);
            while (node.MoveNext())
            {
                _treeView.ExpandRow(node.CurrentPosition, false);
            }
        }

        #region Tree Events

        private void OnRowClicked(object sender, TreeViewRowEventArgs e)
        {
            var node = _treeStore.GetNavigatorAt(e.Position);
            var nodeType = node.GetValue(_type);

            switch (nodeType)
            {
                case NodeType.SourceControl:
                    node.MoveToParent();
                    var projectName = node.GetValue(_name);
                    node.MoveToParent();
                    var serverName = node.GetValue(_name);
                    SourceControlExplorerView.Open(serverName, projectName);
                    break;
                default:
                    break;
            }
        }

        #endregion

    }
}

