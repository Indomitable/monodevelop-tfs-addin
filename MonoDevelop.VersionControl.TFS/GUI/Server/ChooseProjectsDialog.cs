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
using System.Linq;
using System.Collections.Generic;
using MonoDevelop.VersionControl.TFS.Configuration;
using MonoDevelop.VersionControl.TFS.Core.Structure;

namespace MonoDevelop.VersionControl.TFS.GUI.Server
{
    public class ChooseProjectsDialog : Dialog
    {
        readonly ListStore collectionStore;
        readonly ListBox collectionsList = new ListBox();
        readonly DataField<string> collectionName = new DataField<string>();
        readonly DataField<ProjectCollectionConfig> collectionItem = new DataField<ProjectCollectionConfig>();
        readonly TreeStore projectsStore;
        readonly TreeView projectsList = new TreeView();
        readonly DataField<bool> isProjectSelected = new DataField<bool>();
        readonly DataField<string> projectName = new DataField<string>();
        readonly DataField<ProjectConfig> projectItem = new DataField<ProjectConfig>();

        internal List<ProjectCollectionConfig> SelectedProjectColletions { get; set; }

        internal ChooseProjectsDialog(ServerConfig serverConfig)
        {
            collectionStore = new ListStore(collectionName, collectionItem);
            projectsStore = new TreeStore(isProjectSelected, projectName, projectItem);
            BuildGui();
            if (!serverConfig.ProjectCollections.Any())
                SelectedProjectColletions = new List<ProjectCollectionConfig>();
            else
                SelectedProjectColletions = new List<ProjectCollectionConfig>(serverConfig.ProjectCollections);
            LoadData(serverConfig);
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
                if (isSelected) //Should add the project
                {
                    var collection = SelectedProjectColletions.SingleOrDefault(pc => pc == project.Collection);
                    if (collection == null)
                    {
                        collection = project.Collection.Copy();
                        collection.Projects.Add(project);
                        SelectedProjectColletions.Add(collection);
                    }
                    else
                    {
                        //Should not exists because now is selected
                        collection.Projects.Add(project);
                    }
                }
                else
                {
                    //Should exists because the project has been checked
                    var collection = SelectedProjectColletions.Single(pc => pc == project.Collection);
                    collection.Projects.Remove(project);
                    if (!collection.Projects.Any())
                    {
                        SelectedProjectColletions.Remove(collection);
                    }
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

        void LoadData(ServerConfig serverConfig)
        {
            var server = TeamFoundationServerFactory.Create(serverConfig);
            var newServerConfig = server.FetchServerStructure();
            foreach (var collection in newServerConfig.ProjectCollections)
            {
                var row = collectionStore.AddRow();
                collectionStore.SetValue(row, collectionName, collection.Name);
                collectionStore.SetValue(row, collectionItem, collection);
            }
            collectionsList.SelectionChanged += (sender, e) =>
            {
                if (collectionsList.SelectedRow > -1)
                {
                    var collection = collectionStore.GetValue(collectionsList.SelectedRow, collectionItem);
                    var selectedColletion = SelectedProjectColletions.SingleOrDefault(pc => pc == collection);
                    projectsStore.Clear();
                    foreach (var project in collection.Projects)
                    {
                        var node = projectsStore.AddNode();
                        var project1 = project;
                        var isSelected = selectedColletion != null && selectedColletion.Projects.Any(p => p == project1);
                        node.SetValue(isProjectSelected, isSelected);
                        node.SetValue(projectName, project.Name);    
                        node.SetValue(projectItem, project);
                    }
                }
            };
            if (newServerConfig.ProjectCollections.Any())
                collectionsList.SelectRow(0);
        }
    }
}
