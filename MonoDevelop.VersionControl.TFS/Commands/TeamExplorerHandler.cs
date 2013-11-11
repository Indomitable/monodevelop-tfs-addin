//
// TeamExplorerHandler.cs
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
                //pad = IdeApp.Workbench.AddPad(content, "MonoDevelop.VersionControl.TFS.TeamExplorerPad", "Team Explorer", "Right", null);
                pad = IdeApp.Workbench.ShowPad(content, "MonoDevelop.VersionControl.TFS.TeamExplorerPad", "Team Explorer", "Right", null);
                if (pad == null)
                    return;
            }
            pad.Sticky = true;
            pad.AutoHide = false;
            pad.BringToFront();
        }

        protected override void Update(CommandInfo info)
        {
            if (MonoDevelop.VersionControl.VersionControlService.IsGloballyDisabled)
            {
                info.Enabled = false;
                return;
            }
            base.Update(info);
        }
    }
}

