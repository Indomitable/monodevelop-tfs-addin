//
// CredentialsDialog.cs
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
using System.Net;

namespace MonoDevelop.VersionControl.TFS.GUI
{
    public class CredentialsDialog : Dialog
    {
        readonly TextEntry domainEntry = new TextEntry();
        readonly TextEntry userNameEntry = new TextEntry();
        readonly PasswordEntry passwordEntry = new PasswordEntry();

        public CredentialsDialog()
        {
            BuildGui();
        }

        void BuildGui()
        {
            var table = new Table();
            table.Add(new Label(GettextCatalog.GetString("Domain") + ":"), 0, 0);
            table.Add(domainEntry, 1, 0);
            table.Add(new Label(GettextCatalog.GetString("User Name") + ":"), 0, 1);
            table.Add(userNameEntry, 1, 1);
            table.Add(new Label(GettextCatalog.GetString("Password") + ":"), 0, 2);
            table.Add(passwordEntry, 1, 2);

            this.Buttons.Add(Command.Ok, Command.Cancel);
            this.Content = table;
            AttachEvents();
        }

        void AttachEvents()
        {
            passwordEntry.KeyReleased += (sender, e) =>
            {
                if (e.Key == Key.Return)
                    this.Respond(Command.Ok);
            };
        }

        public NetworkCredential Credentials
        {
            get
            {
                if (string.IsNullOrEmpty(userNameEntry.Text))
                    return null;
                return new NetworkCredential { Domain = domainEntry.Text, UserName = userNameEntry.Text, Password = passwordEntry.Password };
            }
        }
    }
}
