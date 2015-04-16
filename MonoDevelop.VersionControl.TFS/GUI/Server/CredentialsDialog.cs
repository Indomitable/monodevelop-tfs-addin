// CredentialsDialog.cs
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
using MonoDevelop.Core;
using MonoDevelop.VersionControl.TFS.Core.ServerAuthorization;
using MonoDevelop.VersionControl.TFS.GUI.Server.Authorization;
using Xwt;

namespace MonoDevelop.VersionControl.TFS.GUI.Server
{
    public class CredentialsDialog : Dialog
    {
        private readonly Uri _serverUri;
        readonly VBox typeContainer = new VBox();
        readonly ComboBox comboBox = new ComboBox();
        private IServerAuthorizationConfig currentConfig;

        public CredentialsDialog(Uri serverUri)
        {
            _serverUri = serverUri;
            BuildGui();
        }

        void BuildGui()
        {
            VBox container = new VBox();
            container.PackStart(new Label(GettextCatalog.GetString("Select authorization type") + ":"));
            foreach (var type in Enum.GetValues(typeof(ServerAuthorizationType)))
            {
                comboBox.Items.Add(type, Enum.GetName(typeof(ServerAuthorizationType), type));
            }
            container.PackStart(comboBox);
            comboBox.SelectionChanged += (sender, args) => SetTypeConfig();
            comboBox.SelectedIndex = 0;
            SetTypeConfig();

            container.PackStart(typeContainer);

            this.Buttons.Add(Command.Ok, Command.Cancel);
            this.Content = container;
        }


        private void SetTypeConfig()
        {
            var type = (ServerAuthorizationType)comboBox.SelectedItem;
            currentConfig = ServerAuthorizationFactory.GetServerAuthorizationConfig(type, _serverUri);
            typeContainer.Clear();
            typeContainer.PackStart(currentConfig.Widget, true, true);
        }

        internal IServerAuthorization Result
        {
            get
            {
                var type = (ServerAuthorizationType)comboBox.SelectedItem;
                return ServerAuthorizationFactory.GetServerAuthorization(type, currentConfig);
            }
        }
    }
}
