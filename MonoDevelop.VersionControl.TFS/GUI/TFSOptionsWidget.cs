﻿//
// TFSOptionsWidget.cs
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
using MonoDevelop.Core;
using Xwt;
using Microsoft.TeamFoundation.VersionControl.Client.Enums;
using MonoDevelop.VersionControl.TFS.GUI.VersionControl;

namespace MonoDevelop.VersionControl.TFS.GUI
{
    public class TFSOptionsWidget : VBox
    {
        readonly ComboBox lockLevelBox = GuiHelper.GetLockLevelComboBox();
        readonly CheckBox debugModeBox = new CheckBox(GettextCatalog.GetString("Debug Mode"));

        public TFSOptionsWidget()
        {
            BuildGui();
        }

        void BuildGui()
        {
            this.PackStart(new Label(GettextCatalog.GetString("Lock Level:")));
            this.PackStart(lockLevelBox);

            var mergeToolButton = new Button(GettextCatalog.GetString("Config merge tool"));
            mergeToolButton.Clicked += OnConfigMergeTool;
            this.PackStart(mergeToolButton);

            debugModeBox.AllowMixed = false;
            debugModeBox.Active = TFSVersionControlService.Instance.IsDebugMode;
            this.PackStart(debugModeBox);
        }

        void OnConfigMergeTool(object sender, EventArgs e)
        {
            using (var mergeToolDialog = new MergeToolConfigDialog(TFSVersionControlService.Instance.MergeToolInfo))
            {
                if (mergeToolDialog.Run(this.ParentWindow) == Command.Ok)
                {
                    TFSVersionControlService.Instance.MergeToolInfo = mergeToolDialog.MergeToolInfo;
                    TFSVersionControlService.Instance.StorePrefs();
                }
            }
        }

        public void ApplyChanges()
        {
            TFSVersionControlService.Instance.CheckOutLockLevel = (CheckOutLockLevel)lockLevelBox.SelectedItem;
            TFSVersionControlService.Instance.IsDebugMode = debugModeBox.Active;
        }
    }
}

