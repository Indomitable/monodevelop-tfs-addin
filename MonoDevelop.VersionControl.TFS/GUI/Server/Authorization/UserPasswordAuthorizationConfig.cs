// //
// // UserPasswordAuthorizationConfig.cs
// //
// // Author:
// //       Ventsislav Mladenov
// //
// // Copyright (c) 2013-2015 Ventsislav Mladenov
// //
// // Permission is hereby granted, free of charge, to any person obtaining a copy
// // of this software and associated documentation files (the "Software"), to deal
// // in the Software without restriction, including without limitation the rights
// // to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// // copies of the Software, and to permit persons to whom the Software is
// // furnished to do so, subject to the following conditions:
// //
// // The above copyright notice and this permission notice shall be included in
// // all copies or substantial portions of the Software.
// //
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// // FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// // THE SOFTWARE.

using System;
using MonoDevelop.Core;
using MonoDevelop.VersionControl.TFS.Core.ServerAuthorization.Config;
using MonoDevelop.VersionControl.TFS.Helpers;
using Xwt;

namespace MonoDevelop.VersionControl.TFS.GUI.Server.Authorization
{
    class UserPasswordAuthorizationConfig : IServerAuthorizationConfig, IUserPasswordAuthorizationConfig
    {
        private readonly Uri _serverUri;
        protected readonly VBox Container = new VBox();
        private readonly TextEntry domainEntry = new TextEntry();
        private readonly TextEntry userNameEntry = new TextEntry();
        private readonly PasswordEntry passwordEntry = new PasswordEntry();
        private bool? clearSavePassword = null;

        public UserPasswordAuthorizationConfig(Uri serverUri)
        {
            _serverUri = serverUri;
            this.BuildUI();
        }

        private void BuildUI()
        {
            Container.PackStart(new Label(GettextCatalog.GetString("User Name") + ":"));
            Container.PackStart(userNameEntry);
            Container.PackStart(new Label(GettextCatalog.GetString("Password") + ":"));
            Container.PackStart(passwordEntry);
        }

        public Widget Widget
        {
            get { return Container; }
        }

        public string UserName
        {
            get { return userNameEntry.Text; }
        }

        public string Password
        {
            get
            {
                //Try store credentials in storage
                CredentialsManager.StoreCredential(_serverUri, passwordEntry.Password);
                var password = CredentialsManager.GetPassword(_serverUri); //Try Get Password
                clearSavePassword = password != passwordEntry.Password;
                return passwordEntry.Password;
            }
        }

        public bool ClearSavePassword
        {
            get
            {
                if (!clearSavePassword.HasValue)
                    throw new Exception("Get Password before getting this property");
                return clearSavePassword.Value;
            }
        }
    }
}