// TFSOptionsWidget.cs
// 
// Author:
//       Ventsislav Mladenov
// 
// The MIT License (MIT)
// 
// Copyright (c) 2013-2015 Ventsislav Mladenov
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
using Autofac;
using MonoDevelop.Core;
using MonoDevelop.VersionControl.TFS.GUI.VersionControl;
using MonoDevelop.VersionControl.TFS.Infrastructure;
using MonoDevelop.VersionControl.TFS.VersionControl.Enums;
using Xwt;

namespace MonoDevelop.VersionControl.TFS.GUI
{
    public class TFSOptionsWidget : VBox
    {
        readonly ComboBox lockLevelBox = GuiHelper.GetLockLevelComboBox();
        readonly CheckBox debugModeBox = new CheckBox(GettextCatalog.GetString("Debug Mode"));
        private readonly TFSVersionControlService _service;

        public TFSOptionsWidget()
        {
            _service = DependencyInjection.Container.Resolve<TFSVersionControlService>();
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
            debugModeBox.Active = _service.IsDebugMode;
            this.PackStart(debugModeBox);
        }

        void OnConfigMergeTool(object sender, EventArgs e)
        {
            using (var mergeToolDialog = new MergeToolConfigDialog(_service.MergeToolInfo))
            {
                if (mergeToolDialog.Run(this.ParentWindow) == Command.Ok)
                {
                    _service.MergeToolInfo = mergeToolDialog.MergeToolInfo;
                }
            }
        }

        public void ApplyChanges()
        {
            _service.CheckOutLockLevel = (LockLevel)lockLevelBox.SelectedItem;
            _service.IsDebugMode = debugModeBox.Active;
        }
    }
}

