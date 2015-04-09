//
// GetSpecVersionDialog.cs
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
using System.Collections.Generic;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Client.Enums;
using Microsoft.TeamFoundation.VersionControl.Client.Objects;
using MonoDevelop.Core;
using MonoDevelop.VersionControl.TFS.VersionControl;
using MonoDevelop.VersionControl.TFS.VersionControl.Models;
using MonoDevelop.VersionControl.TFS.VersionControl.Structure;
using Xwt;

namespace MonoDevelop.VersionControl.TFS.GUI.VersionControl
{
    public class GetSpecVersionDialog : Dialog
    {
        readonly ListView listView = new ListView();
        readonly DataField<ExtendedItem> itemField = new DataField<ExtendedItem>();
        readonly DataField<bool> isSelectedField = new DataField<bool>();
        readonly DataField<string> nameField = new DataField<string>();
        readonly DataField<string> pathField = new DataField<string>();
        ListStore listStore;
        readonly ComboBox versionBox = new ComboBox();
        readonly CheckBox forceGet = new CheckBox(GettextCatalog.GetString("Force get of file versions already in workspace"));
        //readonly CheckBox overrideGet = new CheckBox(GettextCatalog.GetString("Overwrite writeable files that are not checked out"));
        readonly SpinButton changeSetNumber = new SpinButton();
        readonly IWorkspace workspace;

        internal GetSpecVersionDialog(IWorkspace workspace)
        {
            this.workspace = workspace;
            BuildGui();
        }

        void BuildGui()
        {
            this.Title = "Get";
            this.Resizable = false;
            VBox content = new VBox();
            content.PackStart(new Label(GettextCatalog.GetString("Files") + ":"));

            listStore = new ListStore(itemField, isSelectedField, nameField, pathField);
            var checkSell = new CheckBoxCellView(isSelectedField);
            checkSell.Editable = true;
            listView.Columns.Add("Name", checkSell, new TextCellView(nameField));
            listView.Columns.Add("Folder", new TextCellView(pathField));
            listView.MinHeight = 200;
            listView.DataSource = listStore;

            content.PackStart(listView);

            HBox typeBox = new HBox();
            typeBox.PackStart(new Label(GettextCatalog.GetString("Version") + ":"));
            versionBox.Items.Add(0, "Changeset");
            versionBox.Items.Add(1, "Latest Version");
            versionBox.SelectedItem = 1;
            versionBox.SelectionChanged += (sender, e) => changeSetNumber.Visible = (int)versionBox.SelectedItem == 0;
            typeBox.PackStart(versionBox);
            changeSetNumber.Visible = false;
            changeSetNumber.WidthRequest = 100;
            changeSetNumber.MinimumValue = 1;
            changeSetNumber.MaximumValue = int.MaxValue;
            changeSetNumber.Value = 0;
            changeSetNumber.IncrementValue = 1;
            changeSetNumber.Digits = 0;
            typeBox.PackStart(changeSetNumber);
            content.PackStart(typeBox);

            content.PackStart(forceGet);
            //content.PackStart(overrideGet);

            HBox buttonBox = new HBox();
            Button okButton = new Button(GettextCatalog.GetString("Get"));
            okButton.Clicked += OnGet;
            Button cancelButton = new Button(GettextCatalog.GetString("Cancel"));
            cancelButton.Clicked += (sender, e) => Respond(Command.Cancel);
            okButton.WidthRequest = cancelButton.WidthRequest = Constants.ButtonWidth;

            buttonBox.PackEnd(cancelButton);
            buttonBox.PackEnd(okButton);
            content.PackStart(buttonBox);

            this.Content = content;
        }

        internal void AddItems(List<ExtendedItem> items)
        {
            listStore.Clear();
            foreach (var item in items)
            {
                var row = listStore.AddRow();
                listStore.SetValue(row, itemField, item);
                listStore.SetValue(row, isSelectedField, true);
                RepositoryPath path = item.ServerPath;
                listStore.SetValue(row, nameField, path.ItemName);
                listStore.SetValue(row, pathField, path.ParentPath);
            }
        }

        void OnGet(object sender, System.EventArgs e)
        {
            var requests = new List<GetRequest>();
            for (int row = 0; row < listStore.RowCount; row++)
            {
                var isChecked = listStore.GetValue(row, isSelectedField);
                if (isChecked)
                {
                    var item = listStore.GetValue(row, itemField);
                    var spec = new ItemSpec(item.ServerPath, item.ItemType == ItemType.File ? RecursionType.None : RecursionType.Full);
                    var version = (int)versionBox.SelectedItem == 0 ? new ChangesetVersionSpec(Convert.ToInt32(changeSetNumber.Value)) : VersionSpec.Latest;
                    requests.Add(new GetRequest(spec, version));
                    if (forceGet.State == CheckBoxState.On)
                    {
                        workspace.ResetDownloadStatus(item.ItemId);
                    }
                }
            }
            var option = GetOptions.None;
            if (forceGet.State == CheckBoxState.On)
            {
                option |= GetOptions.GetAll;
            }
//            if (overrideGet.State == CheckBoxState.On)
//                option |= GetOptions.Overwrite;
            using (var progress = VersionControlService.GetProgressMonitor("Get", VersionControlOperationType.Pull))
            {
                progress.Log.WriteLine("Start downloading items. GetOption: " + option);
                foreach (var request in requests)
                {
                    progress.Log.WriteLine(request);
                }
                workspace.Get(requests, option);
                progress.ReportSuccess("Finish Downloading.");
            }
            Respond(Command.Ok);
        }
    }
}

