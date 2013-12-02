//
// TFSRepositoryEditor.cs
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
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace MonoDevelop.VersionControl.TFS.GUI.Server
{
    public class TFSRepositoryEditor : IRepositoryEditor
    {
        //        readonly AddServerWidget widget;
        //        readonly TFSRepository repo;
        public TFSRepositoryEditor(TFSRepository repo)
        {
//            this.repo = repo;
//            widget = new AddServerWidget();
        }

        #region IRepositoryEditor implementation

        public bool Validate()
        {
            return false;
//            var res = !string.IsNullOrEmpty(widget.ServerName) && widget.ServerUrl != null;
//            if (res)
//            {
//                this.repo.Name = widget.ServerName;
//                using (var credentialsDialog = new CredentialsDialog())
//                {
//                    if (credentialsDialog.Run(widget.ParentWindow) == Command.Ok && credentialsDialog.Credentials != null)
//                    {
//                        var server = new TeamFoundationServer(widget.ServerUrl, widget.ServerName, 
//                                         credentialsDialog.Credentials.Domain, 
//                                         credentialsDialog.Credentials.UserName, 
//                                         credentialsDialog.Credentials.Password, 
//                                         true);
//                        server.LoadProjectConnections();
//                        if (server.ProjectCollections.Count != 1)
//                            return false;
//                        this.repo.VersionControlService = server.ProjectCollections[0].GetService<RepositoryService>();
//                        return true;
//                    }
//                }
//            }
//            return false;
        }

        public Gtk.Widget Widget
        {
            get
            {
                Gtk.VBox vbox = new Gtk.VBox();
                vbox.PackStart(new Gtk.Label("Plase Use Source Control Explorer for Checkout"));
                vbox.ShowAll();
                return vbox; //(Gtk.Widget)Toolkit.CurrentEngine.GetNativeWidget(widget);
            }
        }

        #endregion
    }
}

