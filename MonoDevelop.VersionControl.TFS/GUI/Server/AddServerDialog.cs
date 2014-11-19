//
// AddServerDialog.cs
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
using System;
using Microsoft.TeamFoundation.Client;

namespace MonoDevelop.VersionControl.TFS.GUI.Server
{
    public class AddServerDialog : Dialog
    {
        readonly AddServerWidget widget = new AddServerWidget();
        readonly AddVisualStudioOnlineServerWidget vsoWidget = new AddVisualStudioOnlineServerWidget();
        readonly Notebook notebook = new Notebook();

        public AddServerDialog()
        {
            BuildGui();
        }

        void BuildGui()
        {
            this.Title = GettextCatalog.GetString("Add Team Foundation Server");
            this.Buttons.Add(Command.Ok, Command.Cancel);
            notebook.Add(widget, GettextCatalog.GetString("TFS Server"));
            notebook.Add(vsoWidget, GettextCatalog.GetString("Visual Studio Online"));
            this.Content = notebook;
            this.Resizable = false;
        }

        public ServerType ServerType { get { return notebook.CurrentTabIndex == 0 ? ServerType.TFS : ServerType.VisualStudio; } }

        public BaseServerInfo ServerInfo { get { return ServerType == ServerType.TFS ? widget.ServerInfo : vsoWidget.ServerInfo; } }

    }
}
