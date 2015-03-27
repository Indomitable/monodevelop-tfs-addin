using System;
using Xwt;
using MonoDevelop.Core;
using MonoDevelop.VersionControl.TFS.Core;

namespace MonoDevelop.VersionControl.TFS.GUI.Server
{
    public class AddCloudServerWidget : VBox, IAddServerWidget
    {
        readonly TextEntry _nameEntry = new TextEntry();
        readonly TextEntry _urlEntry = new TextEntry();
        readonly TextEntry _tfsNameEntry = new TextEntry();

        public AddCloudServerWidget()
        {
            this.BuildGui();
        }

        void BuildGui()
        {
            this.Margin = new WidgetSpacing(5, 5, 5, 5);
            var tableDetails = new Table();
            tableDetails.Add(new Label(GettextCatalog.GetString("Name") + ":"), 0, 0);
            tableDetails.Add(_nameEntry, 1, 0);
            tableDetails.Add(new Label(GettextCatalog.GetString("Url") + ":"), 0, 1);
            tableDetails.Add(_urlEntry, 1, 1);
            tableDetails.Add(new Label(GettextCatalog.GetString("https://<<User Name>>.visualstudio.com")), 2, 1);
            tableDetails.Add(new Label(GettextCatalog.GetString("User Name") + ":"), 0, 2);
            tableDetails.Add(_tfsNameEntry, 1, 2);
            tableDetails.Add(new Label(GettextCatalog.GetString("User name with access to TFS. Usually your Microsoft account.")), 2, 2);
            this.PackStart(tableDetails);
        }

        public AddServerResult Result
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_urlEntry.Text) || string.IsNullOrWhiteSpace(_tfsNameEntry.Text))
                    return null;
                var name = string.IsNullOrWhiteSpace(_nameEntry.Text) ? _urlEntry.Text : _nameEntry.Text;
                return new AddServerResult
                {
                    Type = ServerType.Cloud,
                    Name = name,
                    Url = new Uri(_urlEntry.Text),
                    UserName = _tfsNameEntry.Text
                };
            }
        }
    }
}