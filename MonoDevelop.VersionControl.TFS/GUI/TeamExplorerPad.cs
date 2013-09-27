using MonoDevelop.Ide.Gui;
using Xwt;
using MonoDevelop.Components.Commands;
using MonoDevelop.VersionControl.TFS.Commands;
using MonoDevelop.Ide;
using Microsoft.TeamFoundation.Client;
using MonoDevelop.VersionControl.TFS.Helpers;
using Microsoft.TeamFoundation.Server;
using System.Linq;

namespace MonoDevelop.VersionControl.TFS.GUI
{
    public class TeamExplorerPad : IPadContent
    {
        private readonly VBox _content = new VBox();
        private readonly TreeView _treeView = new TreeView();
        private readonly DataField<string> _name = new DataField<string>();
        private readonly TreeStore _treeStore;
        private System.Action onServersChanged;

        public TeamExplorerPad()
        {
            _treeStore = new TreeStore(_name);
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
        }

        #endregion

        private void UpdateData()
        {
            _treeStore.Clear();
            foreach (var server in TFSVersionControlService.Instance.Servers)
            {
                var node = _treeStore.AddNode().SetValue(_name, server.Name);

                var credentials = CredentialsManager.LoadCredential(server.Url);
                using (var tfsServer = TeamFoundationServerFactory.GetServer(server.Url, credentials))
                {
                    if (!tfsServer.HasAuthenticated)
                        tfsServer.Authenticate();
                    var projectService = tfsServer.GetService<ICommonStructureService>();
                    foreach (var projectInfo in projectService.ListProjects().OrderBy(x => x.Name))
                    {
                        node.AddChild().SetValue(_name, projectInfo.Name);
                        node.AddChild().SetValue(_name, "Work Items");
                        node.MoveToParent();
                        node.AddChild().SetValue(_name, "Source Control");
                        node.MoveToParent();
                        node.MoveToParent();
                    }
                }
            }
        }
    }
}

