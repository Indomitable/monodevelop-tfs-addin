//
// RenameDialog.cs
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
using Microsoft.TeamFoundation.VersionControl.Client.Objects;
using MonoDevelop.Core;
using System.IO;
using MonoDevelop.Ide;
using GLib;
using System.Collections.Generic;
using System.Linq;

namespace MonoDevelop.VersionControl.TFS.GUI.VersionControl.Dialogs
{
    public class RenameDialog : Dialog
    {
        readonly ExtendedItem item;
        readonly TextEntry nameEntry = new TextEntry();

        public RenameDialog(ExtendedItem item)
        {
            this.item = item;
            BuildGui();
        }

        void BuildGui()
        {
            this.Title = GettextCatalog.GetString("Rename item") + ": " + item.ServerPath.ItemName;
            this.Resizable = false;
            var content = new HBox();
            content.PackStart(new Label(GettextCatalog.GetString("New name") + ":"));
            nameEntry.Text = item.ServerPath.ItemName;
            nameEntry.WidthRequest = 200;
            content.PackStart(nameEntry);
            this.Buttons.Add(Command.Ok, Command.Cancel);
            this.Content = content;
        }

        string NewPath
        {
            get
            {
                var dir = Path.GetDirectoryName(item.LocalItem);
                return Path.Combine(dir, nameEntry.Text);
            }
        }

        internal static string Open(ExtendedItem item, Microsoft.TeamFoundation.VersionControl.Client.Workspace workspace)
        {
            using (var dialog = new RenameDialog(item))
            {
                if (dialog.Run(Xwt.Toolkit.CurrentEngine.WrapWindow(MessageService.RootWindow)) == Command.Ok)
                {
                    using (var progress = VersionControlService.GetProgressMonitor("Undo", VersionControlOperationType.Pull))
                    {
                        progress.BeginTask("Rename", 1);
                        List<Failure> failures;
                        if (item.ItemType == Microsoft.TeamFoundation.VersionControl.Client.Enums.ItemType.File)
                            workspace.PendRenameFile(item.LocalItem, dialog.NewPath, out failures);
                        else
                            workspace.PendRenameFolder(item.LocalItem, dialog.NewPath, out failures);

                        if (failures != null && failures.Any(f => f.SeverityType == Microsoft.TeamFoundation.VersionControl.Client.Enums.SeverityType.Error))
                        {
                            progress.EndTask();
                            foreach (var failure in failures.Where(f => f.SeverityType == Microsoft.TeamFoundation.VersionControl.Client.Enums.SeverityType.Error))
                            {
                                progress.ReportError(failure.Code, new Exception(failure.Message));
                            }
                            return string.Empty;
                        }
                        progress.EndTask();
                        progress.ReportSuccess("Finish Undo");
                        return dialog.NewPath;
                    }
                }
                return string.Empty;
            }
        }
    }
}

