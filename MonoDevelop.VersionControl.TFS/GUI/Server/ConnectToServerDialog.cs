//
// ConnectToServerDialog.cs
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
using MonoDevelop.Core;
using MonoDevelop.VersionControl.TFS.Helpers;
using MonoDevelop.Ide;
using Microsoft.TeamFoundation.Client;
using System.Linq;
using System.Collections.Generic;

namespace MonoDevelop.VersionControl.TFS.GUI.Server
{
    public class ConnectToServerDialog : Dialog
    {
        private readonly ListView serverList = new ListView();
        private readonly DataField<string> nameField = new DataField<string>();
        private readonly DataField<string> urlField = new DataField<string>();
        private readonly DataField<BaseTeamFoundationServer> serverField = new DataField<BaseTeamFoundationServer>();
        private readonly ListStore serverStore;

        public ConnectToServerDialog()
        {
            serverStore = new ListStore(nameField, urlField, serverField);
            BuildGui();
            UpdateServersList();
        }

        private void BuildGui()
        {
            this.Title = GettextCatalog.GetString("Add/Remove Team Foundation Server");

            var table = new Table();

            table.Add(new Label(GettextCatalog.GetString("Team Foundation Server list")), 0, 0, 1, 2);

            serverList.SelectionMode = SelectionMode.Single;
            serverList.MinWidth = 500;
            serverList.MinHeight = 400;
            serverList.Columns.Add(new ListViewColumn("Name", new TextCellView(nameField) { Editable = false }));
            serverList.Columns.Add(new ListViewColumn("Url", new TextCellView(urlField) { Editable = false }));
            serverList.DataSource = serverStore;
            serverList.RowActivated += OnServerClicked;
            table.Add(serverList, 0, 1);

            VBox buttonBox = new VBox();
            var addButton = new Button(GettextCatalog.GetString("Add"));
            addButton.Clicked += OnAddServer;
            addButton.MinWidth = Constants.ButtonWidth;
            buttonBox.PackStart(addButton);

            var removeButton = new Button(GettextCatalog.GetString("Remove"));
            removeButton.MinWidth = Constants.ButtonWidth;
            removeButton.Clicked += OnRemoveServer;
            buttonBox.PackStart(removeButton);

            var closeButton = new Button(GettextCatalog.GetString("Close"));
            closeButton.MinWidth = Constants.ButtonWidth;
            closeButton.Clicked += (sender, e) => this.Respond(Command.Close);
            buttonBox.PackStart(closeButton);

            table.Add(buttonBox, 1, 1);

            this.Content = table;
            this.Resizable = false;
        }

        void OnServerClicked(object sender, ListViewRowEventArgs e)
        {
            var server = serverStore.GetValue(e.RowIndex, serverField);
            using (var projectsDialog = new ChooseProjectsDialog(server))
            {
                if (projectsDialog.Run(this) == Command.Ok && projectsDialog.SelectedProjects.Any())
                {
                    var selectedProjects = projectsDialog.SelectedProjects;
                    server.ProjectCollections = new List<ProjectCollection>(selectedProjects.Select(x => x.Collection).Distinct());
                    server.ProjectCollections.ForEach(pc => pc.Projects = new List<ProjectInfo>(selectedProjects.Where(pi => pi.Collection == pc)));
                    TFSVersionControlService.Instance.RaiseServersChange();
                    TFSVersionControlService.Instance.StorePrefs();
                }
            }
        }

        void OnAddServer(object sender, EventArgs e)
        {
            using (var dialog = new AddServerDialog())
            {
                if (dialog.Run(this) == Command.Ok && dialog.ServerInfo != null)
                {
                    if (TFSVersionControlService.Instance.HasServer(dialog.ServerInfo.Name))
                    {
                        MessageService.ShowError("Server with same name already exists!");
                        return;
                    }
                    using (var credentialsDialog = new CredentialsDialog(dialog.ServerType))
                    {
                        if (credentialsDialog.Run(this) == Command.Ok && credentialsDialog.Authentication != null)
                        {
                            CredentialsManager.StoreCredential(dialog.ServerInfo.Uri, credentialsDialog.Authentication.Password);
                            var password = CredentialsManager.GetPassword(dialog.ServerInfo.Uri); //Try Get Password
                            bool isPasswordSavedInXml = false;
                            if (password == null)
                            {
                                MessageService.ShowWarning("No keyring service found!\nPassword will be saved as plain text.");
                                isPasswordSavedInXml = true;
                            }
                            var server = TeamFoundationServerFactory.Create(dialog.ServerType, dialog.ServerInfo, 
                                                                            credentialsDialog.Authentication, isPasswordSavedInXml);
                            using (var projectsDialog = new ChooseProjectsDialog(server))
                            {
                                if (projectsDialog.Run(this) == Command.Ok && projectsDialog.SelectedProjects.Any())
                                {
                                    var selectedProjects = projectsDialog.SelectedProjects;
                                    server.ProjectCollections = new List<ProjectCollection>(selectedProjects.Select(x => x.Collection).Distinct());
                                    server.ProjectCollections.ForEach(pc => pc.Projects = new List<ProjectInfo>(selectedProjects.Where(pi => pi.Collection == pc)));
                                    TFSVersionControlService.Instance.AddServer(server);
                                    UpdateServersList();
                                }
                            }
                        }
                    }
                }
            }
        }

        void OnRemoveServer(object sender, EventArgs e)
        {
            if (MessageService.Confirm("Are you sure you want to delete this server!", AlertButton.Delete))
            {
                var serverName = serverStore.GetValue(serverList.SelectedRow, nameField);
                TFSVersionControlService.Instance.RemoveServer(serverName);
                UpdateServersList();
            }
        }

        private void UpdateServersList()
        {
            serverStore.Clear();
            foreach (var server in TFSVersionControlService.Instance.Servers)
            {
                var row = serverStore.AddRow();
                serverStore.SetValue(row, nameField, server.Name);
                serverStore.SetValue(row, urlField, server.Uri.ToString());
                serverStore.SetValue(row, serverField, server);
            }
        }
    }
}

