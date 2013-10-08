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
using System.Text.RegularExpressions;
using System.Net;
using MonoDevelop.VersionControl.TFS.Helpers;
using MonoDevelop.Ide;
using Microsoft.TeamFoundation.Client;
using System.Linq;
using GLib;
using System.Collections.Generic;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Projects;

namespace MonoDevelop.VersionControl.TFS.GUI
{
    public class ConnectToServerDialog : Dialog
    {
        private readonly ListView _serverList = new ListView();
        private readonly DataField<string> _name = new DataField<string>();
        private readonly DataField<string> _url = new DataField<string>();
        private readonly ListStore _store;

        public ConnectToServerDialog()
        {
            _store = new ListStore(_name, _url);
            BuildGui();
            UpdateServersList();
        }

        private void BuildGui()
        {
            this.Title = GettextCatalog.GetString("Add/Remove Team Foundation Server");

            var table = new Table();

            table.Add(new Label(GettextCatalog.GetString("Team Foundation Server list")), 0, 0, 1, 2);

            _serverList.SelectionMode = SelectionMode.Single;
            _serverList.MinWidth = 500;
            _serverList.MinHeight = 400;
            _serverList.Columns.Add(new ListViewColumn("Name", new TextCellView(_name) { Editable = false }));
            _serverList.Columns.Add(new ListViewColumn("Url", new TextCellView(_url) { Editable = false }));
            _serverList.DataSource = _store;
            table.Add(_serverList, 0, 1);

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

        void OnAddServer(object sender, EventArgs e)
        {
            using (var dialog = new AddServerDialog())
            {
                if (dialog.Run(this) == Command.Ok && dialog.Url != null)
                {
                    if (TFSVersionControlService.Instance.HasServer(dialog.Name))
                    {
                        MessageService.ShowError("Server with same name already exists!");
                        return;
                    }
                    using (var credentialsDialog = new CredentialsDialog())
                    {
                        if (credentialsDialog.Run(this) == Command.Ok && credentialsDialog.Credentials != null)
                        {
                            var uriBuilder = new UriBuilder(dialog.Url);
                            uriBuilder.UserName = credentialsDialog.Credentials.Domain + "\\" + credentialsDialog.Credentials.UserName;
                            if (!CredentialsManager.StoreCredential(uriBuilder.Uri, credentialsDialog.Credentials.Password))
                            {
                                MessageService.ShowWarning("No keyring service found!\nPassword has been saved as plain text in server URL");
                            }
                            uriBuilder.Password = credentialsDialog.Credentials.Password;
                            TeamFoundationServer server = new TeamFoundationServer(uriBuilder.Uri, dialog.Name, credentialsDialog.Credentials);
                            using (var projectCollectionDialog = new ChooseProjectsDialog(server))
                            {
                                if (projectCollectionDialog.Run(this) == Command.Ok && projectCollectionDialog.SelectedProjects.Any())
                                {
                                    var newServer = new TeamFoundationServer(uriBuilder.Uri, dialog.Name, credentialsDialog.Credentials);
                                    newServer.LoadProjectConnections(projectCollectionDialog.SelectedProjects.Select(x => x.Collection.Id).ToList());
                                    foreach (var c in newServer.ProjectCollections)
                                    {
                                        var c1 = c;
                                        c.LoadProjects(projectCollectionDialog.SelectedProjects.Where(p => string.Equals(c1.Name, p.Collection.Name)).Select(x => x.Name).ToList());
                                    }
                                    TFSVersionControlService.Instance.AddServer(newServer);
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
                var serverName = _store.GetValue(_serverList.SelectedRow, _name);
                TFSVersionControlService.Instance.RemoveServer(serverName);
                UpdateServersList();
            }
        }

        private void UpdateServersList()
        {
            _store.Clear();
            foreach (var server in TFSVersionControlService.Instance.Servers)
            {
                var row = _store.AddRow();
                _store.SetValue(row, _name, server.Name);
                _store.SetValue(row, _url, server.Uri.ToString());
            }
        }
    }

    public class AddServerDialog : Dialog
    {
        readonly TextEntry _nameEntry = new TextEntry();
        readonly TextEntry _hostEntry = new TextEntry();
        readonly TextEntry _pathEntry = new TextEntry();
        readonly SpinButton _portEntry = new SpinButton();
        readonly RadioButtonGroup _protocolGroup = new RadioButtonGroup();
        readonly RadioButton _httpRadio = new RadioButton(GettextCatalog.GetString("HTTP"));
        readonly RadioButton _httpsRadio = new RadioButton(GettextCatalog.GetString("HTTPS"));
        readonly TextEntry _previewEntry = new TextEntry();

        public AddServerDialog()
        {
            BuildGui();
        }

        void BuildGui()
        {
            this.Title = GettextCatalog.GetString("Add Team Foundation Server");
            var content = new VBox();
            content.PackStart(new Label(GettextCatalog.GetString("Name of connection")));
            _nameEntry.Changed += (sender, e) => _hostEntry.Text = _nameEntry.Text;
            content.PackStart(_nameEntry);

            content.PackStart(new Label(GettextCatalog.GetString("Host or URL of Team Foundation Server")));

            _hostEntry.Text = "";
            _hostEntry.Changed += OnUrlChanged;
            content.PackStart(_hostEntry);

            var tableDetails = new Table();
            tableDetails.Add(new Label(GettextCatalog.GetString("Connection Details")), 0, 0, 1, 2);
            tableDetails.Add(new Label(GettextCatalog.GetString("Path") + ":"), 0, 1);
            _pathEntry.Text = "tfs";
            _pathEntry.Changed += OnUrlChanged;
            tableDetails.Add(_pathEntry, 1, 1);

            tableDetails.Add(new Label(GettextCatalog.GetString("Port number") + ":"), 0, 2);
            _portEntry.MinimumValue = 1;
            _portEntry.MaximumValue = short.MaxValue;
            _portEntry.Value = 8080;
            _portEntry.IncrementValue = 1;
            _portEntry.Digits = 0;
            _portEntry.ValueChanged += OnUrlChanged;
            tableDetails.Add(_portEntry, 1, 2);

            tableDetails.Add(new Label(GettextCatalog.GetString("Protocol") + ":"), 0, 3);

            var protocolBox = new HBox();

            _httpRadio.Group = _protocolGroup;
            _httpRadio.Active = true;
            protocolBox.PackStart(_httpRadio);

            _httpsRadio.Group = _protocolGroup;
            _httpsRadio.Active = false;
            protocolBox.PackStart(_httpsRadio);

            _protocolGroup.ActiveRadioButtonChanged += (sender, e) =>
            {
                if (_protocolGroup.ActiveRadioButton == _httpRadio)
                {
                    _portEntry.Value = 8080;
                }
                else
                {
                    _portEntry.Value = 443;
                }
                BuildUrl();
            };

            tableDetails.Add(protocolBox, 1, 3);

            content.PackStart(tableDetails);

            var previewBox = new HBox();
            previewBox.PackStart(new Label(GettextCatalog.GetString("Preview") + ":"));
            previewBox.Sensitive = false;
            _previewEntry.BackgroundColor = Xwt.Drawing.Colors.LightGray;
            _previewEntry.MinWidth = 400;

            previewBox.PackStart(_previewEntry, true, true);
            content.PackStart(previewBox);

            this.Buttons.Add(Command.Ok, Command.Cancel);

            this.Content = content;
            this.Resizable = false;
            BuildUrl();
        }

        void OnUrlChanged(object sender, EventArgs e)
        {
            BuildUrl();            
        }

        void BuildUrl()
        {
            if (string.IsNullOrWhiteSpace(_hostEntry.Text))
            {
                _previewEntry.Text = "Sever name cannot be empty.";
                return;
            }
            var hostIsUrl = Regex.IsMatch(_hostEntry.Text, "^http[s]?://");
            _pathEntry.Sensitive = !hostIsUrl;
            _portEntry.Sensitive = !hostIsUrl;
            _httpRadio.Sensitive = !hostIsUrl;
            _httpsRadio.Sensitive = !hostIsUrl;
            if (hostIsUrl)
            {
                _previewEntry.Text = _hostEntry.Text;
            }
            else
            {
                UriBuilder ub = new UriBuilder();
                ub.Host = _hostEntry.Text;
                ub.Scheme = _httpRadio.Active ? "http" : "https";
                ub.Port = (int)_portEntry.Value;
                ub.Path = _pathEntry.Text;
                _previewEntry.Text = ub.ToString();
            }
        }

        public string Name { get { return string.IsNullOrWhiteSpace(_nameEntry.Text) ? Url.ToString() : _nameEntry.Text; } }

        public Uri Url
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_hostEntry.Text))
                    return null;
                return new Uri(_previewEntry.Text); 
            }
        }
    }

    public class CredentialsDialog : Dialog
    {
        readonly TextEntry _domain = new TextEntry();
        readonly TextEntry _userName = new TextEntry();
        readonly PasswordEntry _password = new PasswordEntry();

        public CredentialsDialog()
        {
            BuildGui();
        }

        void BuildGui()
        {
            var table = new Table();
            table.Add(new Label(GettextCatalog.GetString("Domain") + ":"), 0, 0);
            table.Add(_domain, 1, 0);
            table.Add(new Label(GettextCatalog.GetString("User Name") + ":"), 0, 1);
            table.Add(_userName, 1, 1);
            table.Add(new Label(GettextCatalog.GetString("Password") + ":"), 0, 2);
            table.Add(_password, 1, 2);

            this.Buttons.Add(Command.Ok, Command.Cancel);
            this.Content = table;
        }

        public NetworkCredential Credentials
        {
            get
            {
                if (string.IsNullOrEmpty(_userName.Text))
                    return null;
                return new NetworkCredential { Domain = _domain.Text, UserName = _userName.Text, Password = _password.Password };
            }
        }
    }

    public class ChooseProjectsDialog : Dialog
    {
        readonly ListStore collectionStore;
        readonly ListBox collectionsList = new ListBox();
        readonly DataField<string> collectionName = new DataField<string>();
        readonly DataField<ProjectCollection> collectionItem = new DataField<ProjectCollection>();
        readonly TreeStore projectsStore;
        readonly TreeView projectsList = new TreeView();
        readonly DataField<bool> isProjectSelected = new DataField<bool>();
        readonly DataField<string> projectName = new DataField<string>();
        readonly DataField<ProjectInfo> projectItem = new DataField<ProjectInfo>();

        public List<ProjectInfo> SelectedProjects { get; set; }

        public ChooseProjectsDialog(TeamFoundationServer server)
        {
            collectionStore = new ListStore(collectionName, collectionItem);
            projectsStore = new TreeStore(isProjectSelected, projectName, projectItem);
            BuildGui();
            LoadData(server);
            SelectedProjects = new List<ProjectInfo>();
        }

        void BuildGui()
        {
            this.Title = "Select Projects";
            this.Resizable = false;
            var vBox = new VBox();
            var hbox = new HBox();
            collectionsList.DataSource = collectionStore;
            collectionsList.Views.Add(new TextCellView(collectionName));
            collectionsList.MinWidth = 200;
            collectionsList.MinHeight = 300;
            hbox.PackStart(collectionsList);

            projectsList.DataSource = projectsStore;
            projectsList.MinWidth = 200;
            projectsList.MinHeight = 300;
            var checkView = new CheckBoxCellView(isProjectSelected) { Editable = true };
            checkView.Toggled += (sender, e) =>
            {
                var row = projectsList.CurrentEventRow;
                var node = projectsStore.GetNavigatorAt(row);
                var isSelected = !node.GetValue(isProjectSelected); //Xwt gives previous value
                var project = node.GetValue(projectItem);
                if (isSelected && !SelectedProjects.Any(p => string.Equals(p.Name, project.Name)))
                {
                    SelectedProjects.Add(project);
                }
                if (!isSelected && SelectedProjects.Any(p => string.Equals(p.Name, project.Name)))
                {
                    SelectedProjects.RemoveAll(p => string.Equals(p.Name, project.Name));
                }
            };
            projectsList.Columns.Add(new ListViewColumn("", checkView));
            projectsList.Columns.Add(new ListViewColumn("Name", new TextCellView(projectName)));
            hbox.PackEnd(projectsList);

            vBox.PackStart(hbox);

            Button ok = new Button(GettextCatalog.GetString("OK"));
            ok.Clicked += (sender, e) => Respond(Command.Ok);

            Button cancel = new Button(GettextCatalog.GetString("Cancel"));
            cancel.Clicked += (sender, e) => Respond(Command.Cancel);

            ok.MinWidth = cancel.MinWidth = Constants.ButtonWidth;

            var buttonBox = new HBox();
            buttonBox.PackEnd(ok);
            buttonBox.PackEnd(cancel);
            vBox.PackStart(buttonBox);

            this.Content = vBox;
        }

        void LoadData(TeamFoundationServer server)
        {
            server.LoadProjectConnections();
            server.ProjectCollections.ForEach(c => c.LoadProjects());
            foreach (var col in server.ProjectCollections)
            {
                var row = collectionStore.AddRow();
                collectionStore.SetValue(row, collectionName, col.Name);
                collectionStore.SetValue(row, collectionItem, col);
            }
            collectionsList.SelectionChanged += (sender, e) =>
            {
                if (collectionsList.SelectedRow > -1)
                {
                    var collection = collectionStore.GetValue(collectionsList.SelectedRow, collectionItem);
                    projectsStore.Clear();
                    foreach (var project in collection.Projects)
                    {
                        var node = projectsStore.AddNode();
                        node.SetValue(isProjectSelected, false);    
                        node.SetValue(projectName, project.Name);    
                        node.SetValue(projectItem, project);
                    }
                }
            };
            if (server.ProjectCollections.Any())
                collectionsList.SelectRow(0);
        }
    }
}

