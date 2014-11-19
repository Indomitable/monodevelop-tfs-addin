using System;
using Xwt;
using MonoDevelop.Core;
using Microsoft.TeamFoundation.Client;

namespace MonoDevelop.VersionControl.TFS.GUI.Server
{
    public class AddVisualStudioOnlineServerWidget : VBox, IAddServerWidget
    {
        readonly TextEntry _nameEntry = new TextEntry();
        readonly TextEntry _urlEntry = new TextEntry();
        readonly TextEntry _tfsNameEntry = new TextEntry();

        public AddVisualStudioOnlineServerWidget()
        {
            this.BuildGui();
        }

        void BuildGui()
        {
            this.Margin = new WidgetSpacing(5, 5, 5, 5);
            var tableDetails = new Table();
            tableDetails.Add(new Label(GettextCatalog.GetString("Name of connection") + ":"), 0, 0);
            tableDetails.Add(_nameEntry, 1, 0);
            tableDetails.Add(new Label(GettextCatalog.GetString("Visual Studio Online Url") + ":"), 0, 1);
            tableDetails.Add(_urlEntry, 1, 1);
            tableDetails.Add(new Label(GettextCatalog.GetString("http://<<User Name>>.visualstudio.com")), 2, 1);
            tableDetails.Add(new Label(GettextCatalog.GetString("TFS User") + ":"), 0, 2);
            tableDetails.Add(_tfsNameEntry, 1, 2);
            tableDetails.Add(new Label(GettextCatalog.GetString("User name with access to TFS. Usually your Microsoft account.")), 2, 2);
            this.PackStart(tableDetails);
        }

        public BaseServerInfo ServerInfo
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_urlEntry.Text) || string.IsNullOrWhiteSpace(_tfsNameEntry.Text))
                    return null;
                var name = string.IsNullOrWhiteSpace(_nameEntry.Text) ? _urlEntry.Text : _nameEntry.Text;
                return new VisualStudioServerInfo(name, _urlEntry.Text, _tfsNameEntry.Text);
            }
        }
    }
}