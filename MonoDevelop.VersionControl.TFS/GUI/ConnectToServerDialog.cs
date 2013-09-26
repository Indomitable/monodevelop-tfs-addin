using System;
using Xwt;
using MonoDevelop.Core;
using System.Text.RegularExpressions;
using System.Net;
using System.Security;
using MonoDevelop.Ide.TypeSystem;

namespace MonoDevelop.VersionControl.TFS.GUI
{
    public class ConnectToServerDialog : Dialog
    {
        private ListView _serverList;

        public ConnectToServerDialog()
        {
            BuildGui();
        }

        private void BuildGui()
        {
            this.Title = GettextCatalog.GetString("Add/Remove Team Foundation Server");

            var table = new Table();

            table.Add(new Label(GettextCatalog.GetString("Team Foundation Server list")), 0, 0, 1, 2);

            _serverList = new ListView();
            _serverList.MinWidth = 500;
            _serverList.MinHeight = 400;
            table.Add(_serverList, 0, 1);

            DataField<string> name = new DataField<string>();
            DataField<string> url = new DataField<string>();
            ListStore store = new ListStore(name, url);
            _serverList.DataSource = store;
            _serverList.Columns.Add(new ListViewColumn("Name", new TextCellView(name) { Editable = false }));
            _serverList.Columns.Add(new ListViewColumn("Url", new TextCellView(name) { Editable = false }));

            VBox buttonBox = new VBox();
            const int buttonWidth = 80;
            var addButton = new Button(GettextCatalog.GetString("Add"));
            addButton.Clicked += OnAddServer;
            addButton.MinWidth = buttonWidth;
            buttonBox.PackStart(addButton);

            var removeButton = new Button(GettextCatalog.GetString("Remove"));
            removeButton.MinWidth = buttonWidth;
            buttonBox.PackStart(removeButton);

            var closeButton = new Button(GettextCatalog.GetString("Close"));
            closeButton.MinWidth = buttonWidth;
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
                    using (var credentialsDialog = new CredentialsDialog())
                    {
                        if (credentialsDialog.Run(this) == Command.Ok && credentialsDialog.Credentials != null)
                        {
                            var userName = credentialsDialog.Credentials.Domain + "\\" + credentialsDialog.Credentials.UserName;
                            var password = credentialsDialog.Credentials.Password;
                            PasswordService.AddWebUserNameAndPassword(dialog.Url, userName, password);
                            var userPass = PasswordService.GetWebUserNameAndPassword(dialog.Url);
                            if (userPass == null) //No Password Service Provider
                            {

                            }
                        }
                    }
                }
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

        public string Name
        {
            get
            {
                return string.IsNullOrWhiteSpace(_nameEntry.Text) ? Url.ToString() : _nameEntry.Text;
            }
        }

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
}

