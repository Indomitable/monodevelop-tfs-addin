using System;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;

namespace MonoDevelop.VersionControl.TFS.Commands
{
    public class TeamExplorerHandler : CommandHandler
    {
        protected override void Run()
        {
            var pad = IdeApp.Workbench.GetPad<MonoDevelop.VersionControl.TFS.GUI.TeamExplorerPad>();
            if (pad == null)
            {
                var content = new MonoDevelop.VersionControl.TFS.GUI.TeamExplorerPad();
                pad = IdeApp.Workbench.ShowPad(content, "MonoDevelop.VersionControl.TFS.TeamExplorerPad", "Team Explorer", "Right", null);
            }
            pad.Sticky = true;
            pad.AutoHide = false;
            pad.BringToFront();
        }
    }
}

