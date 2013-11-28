//
// ChooseProjectsDialog.cs
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
using Microsoft.TeamFoundation.Client;
using System.Linq;
using System.Collections.Generic;

namespace MonoDevelop.VersionControl.TFS.GUI
{
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
            if (server.ProjectCollections == null)
                SelectedProjects = new List<ProjectInfo>();
            else
                SelectedProjects = new List<ProjectInfo>(server.ProjectCollections.SelectMany(pc => pc.Projects));
            LoadData(server);
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
                if (isSelected && !SelectedProjects.Any(p => string.Equals(p.Uri, project.Uri)))
                {
                    SelectedProjects.Add(project);
                }
                if (!isSelected && SelectedProjects.Any(p => string.Equals(p.Uri, project.Uri)))
                {
                    SelectedProjects.RemoveAll(p => string.Equals(p.Uri, project.Uri));
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
                        var project1 = project;
                        var isSelected = SelectedProjects.Any(x => string.Equals(x.Uri, project1.Uri, StringComparison.OrdinalIgnoreCase));
                        node.SetValue(isProjectSelected, isSelected);
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
