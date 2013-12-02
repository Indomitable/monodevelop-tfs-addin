//
// MergeToolConfigDialog.cs
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
using MonoDevelop.VersionControl.TFS.Infrastructure;
using System.Text;

namespace MonoDevelop.VersionControl.TFS.GUI.VersionControl
{
    public class MergeToolConfigDialog : Dialog
    {
        private readonly VBox content = new VBox();
        private readonly TextEntry commandNameEntry = new TextEntry();
        private readonly TextEntry argumentsEntry = new TextEntry();

        public MergeToolConfigDialog()
        {
            BuildGui();
        }

        public MergeToolConfigDialog(MergeToolInfo mergeInfo)
            : this()
        {
            if (mergeInfo != null)
            {
                this.commandNameEntry.Text = mergeInfo.CommandName;
                this.argumentsEntry.Text = mergeInfo.Arguments;
            }
        }

        void BuildGui()
        {
            this.Title = GettextCatalog.GetString("Merge Tool Config");
            var table = new Table();
            commandNameEntry.Sensitive = false;
            commandNameEntry.WidthRequest = 350;
            table.Add(commandNameEntry, 1, 1);
            var commandSelectButton = new Button(GettextCatalog.GetString("Choose"));
            commandSelectButton.Clicked += (sender, e) => SelectCommand();
            commandSelectButton.WidthRequest = Constants.ButtonWidth;
            table.Add(commandSelectButton, 2, 1);
            StringBuilder argumentsTooltip = new StringBuilder();
            argumentsTooltip.AppendLine("%1 - Local File");
            argumentsTooltip.AppendLine("%2 - Base File");
            argumentsTooltip.AppendLine("%3 - Their File");

            argumentsTooltip.AppendLine("%4 - Local label");
            argumentsTooltip.AppendLine("%5 - Base label");
            argumentsTooltip.Append("%6 - Their label");

            argumentsEntry.TooltipText = argumentsTooltip.ToString();
            argumentsEntry.Text = "%1 %2 %3";
            table.Add(argumentsEntry, 1, 2, 1, 2);

            content.PackStart(table);
            HBox buttonBox = new HBox();

            var buttonOk = new Button(GettextCatalog.GetString("Ok"));
            buttonOk.Clicked += (sender, e) => this.Respond(Command.Ok);
            var buttonCancel = new Button(GettextCatalog.GetString("Cancel"));
            buttonCancel.Clicked += (sender, e) => this.Respond(Command.Cancel);

            buttonOk.WidthRequest = buttonCancel.WidthRequest = Constants.ButtonWidth;
            buttonBox.PackEnd(buttonOk);
            buttonBox.PackEnd(buttonCancel);
            content.PackStart(buttonBox);
            this.Content = content;
            this.Resizable = false;
        }

        private void SelectCommand()
        {
            using (var fileSelect = new OpenFileDialog("Choose executable"))
            {
                fileSelect.Multiselect = false;
                if (fileSelect.Run(this))
                {
                    commandNameEntry.TooltipText = commandNameEntry.Text = fileSelect.FileName;
                }
            }
        }

        public MergeToolInfo MergeToolInfo
        { 
            get
            { 
                if (string.IsNullOrEmpty(commandNameEntry.Text))
                    return null;
                return new MergeToolInfo { CommandName = commandNameEntry.Text, Arguments = argumentsEntry.Text };
            } 
        }
    }
}

