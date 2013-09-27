using MonoDevelop.Components.Commands;
using MonoDevelop.VersionControl.TFS.GUI;
using MonoDevelop.Ide;

namespace MonoDevelop.VersionControl.TFS.Commands
{
    public class ConnectToServerHandler : CommandHandler
    {
        protected override void Run()
        {
            using (var dialog = new ConnectToServerDialog())
            {
                dialog.Run(Xwt.Toolkit.CurrentEngine.WrapWindow(MessageService.RootWindow));
            }
        }
    }
}

