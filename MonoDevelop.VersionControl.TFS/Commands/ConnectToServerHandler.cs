using System;
using MonoDevelop.Components.Commands;
using MonoDevelop.VersionControl.TFS.GUI;

namespace MonoDevelop.VersionControl.TFS.Commands
{
    public class ConnectToServerHandler : CommandHandler
    {
        protected override void Run()
        {
            using (var dialog = new ConnectToServerDialog())
            {
                dialog.Run();
            }
        }
    }
}

