//
// ResolveConflicsView.cs
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
using MonoDevelop.Ide.Gui;
using Xwt;
using MonoDevelop.Ide;
using MonoDevelop.Core;
using System.Collections.Generic;
using MonoDevelop.Projects;
using Microsoft.TeamFoundation.VersionControl.Client.Objects;
using Microsoft.TeamFoundation.VersionControl.Client;
using System.IO;
using GLib;
using MonoDevelop.VersionControl.TFS.Infrastructure.Objects;
using Microsoft.TeamFoundation.VersionControl.Client.Enums;
using System.Diagnostics;
using MonoDevelop.VersionControl.TFS.Infrastructure;

namespace MonoDevelop.VersionControl.TFS.GUI
{
    public class ResolveConflictsView : AbstractXwtViewContent
    {
        private TFSRepository repository;
        private readonly List<FilePath> paths = new List<FilePath>();
        private readonly VBox view = new VBox();
        private readonly ListView listView = new ListView();
        private readonly DataField<Conflict> itemField = new DataField<Conflict>();
        private readonly DataField<string> typeField = new DataField<string>();
        private readonly DataField<string> nameField = new DataField<string>();
        private readonly DataField<int> versionBaseField = new DataField<int>();
        private readonly DataField<int> versionTheirField = new DataField<int>();
        private readonly DataField<int> versionYourField = new DataField<int>();
        private readonly ListStore listStore;
        private readonly Button acceptYours = new Button(GettextCatalog.GetString("Accept Local"));
        private readonly Button acceptTheirs = new Button(GettextCatalog.GetString("Accept Server"));
        private readonly Button acceptMerge = new Button(GettextCatalog.GetString("Merge"));
        private readonly Button viewBase = new Button(GettextCatalog.GetString("View Base"));
        private readonly Button viewTheir = new Button(GettextCatalog.GetString("View Server"));

        private ResolveConflictsView()
        {
            this.ContentName = GettextCatalog.GetString("Resolve Conflicts");
            listStore = new ListStore(typeField, nameField, itemField, versionBaseField, versionTheirField, versionYourField);
            BuildGui();
        }

        private void SetData(TFSRepository repository, List<FilePath> paths)
        {
            this.repository = repository;
            this.paths.Clear();
            this.paths.AddRange(paths);
        }

        void BuildGui()
        {
            HBox topPanel = new HBox();
            topPanel.MarginTop = 5;
            VSeparator separator = new VSeparator();
            acceptYours.WidthRequest = acceptTheirs.WidthRequest = acceptMerge.WidthRequest = viewBase.WidthRequest = viewTheir.WidthRequest = 120;
            SetButtonSensitive();

            topPanel.PackStart(acceptYours);
            topPanel.PackStart(acceptTheirs);
            topPanel.PackStart(acceptMerge);
            topPanel.PackStart(separator);
            topPanel.PackStart(viewBase);
            topPanel.PackStart(viewTheir);
            
            topPanel.MinHeight = 30;
            view.PackStart(topPanel);
            listView.Columns.Add("Conflict Type", typeField);
            listView.Columns.Add("Item Name", nameField);
            listView.Columns.Add("Base Version", versionBaseField);
            listView.Columns.Add("Server Version", versionTheirField);
            listView.Columns.Add("Your Version", versionYourField);
            listView.DataSource = listStore;

            view.PackStart(listView, true, true);
            AttachEvents();
        }

        #region Events

        void AttachEvents()
        {
            listView.SelectionChanged += (sender, e) => SetButtonSensitive();
            listView.RowActivated += (sender, e) => RowClicked();
            viewBase.Clicked += (sender, e) => ViewBaseClicked();
            viewTheir.Clicked += (sender, e) => ViewTheirClicked();

            acceptMerge.Clicked += (sender, e) => AcceptMergeClicked();
            acceptYours.Clicked += (sender, e) => AcceptYoursClicked();
            acceptTheirs.Clicked += (sender, e) => AcceptTheirsClicked();
        }

        private void SetButtonSensitive()
        {
            acceptYours.Sensitive = acceptTheirs.Sensitive = acceptMerge.Sensitive = viewBase.Sensitive = viewTheir.Sensitive = (listView.SelectedRow > -1);
        }

        private void RowClicked()
        {
            var conflict = listStore.GetValue(listView.SelectedRow, itemField);
            var doc = IdeApp.Workbench.OpenDocument(conflict.TargetLocalItem, (Project)null);
            if (doc != null)
            {
                doc.Window.SwitchView(doc.Window.FindView<MonoDevelop.VersionControl.Views.IDiffView>());
//                var diff = doc.Window.ActiveViewContent as MonoDevelop.VersionControl.Views.DiffView;
//                if (diff != null)
//                {
//                    diff.ComparisonWidget.SetRevision(diff.ComparisonWidget.OriginalEditor, new TFSRevision(this.repository, conflict.TheirVersion, conflict.TheirServerItem));
//                    //diff.ComparisonWidget.SetRevision(diff.ComparisonWidget.OriginalEditor, new TFSRevision(this.repository, conflict.TheirVersion, ""));
//                }
            }
        }

        private void ViewBaseClicked()
        {
            var conflict = listStore.GetValue(listView.SelectedRow, itemField);
            var downloadService = this.repository.VersionControlService.Collection.GetService<VersionControlDownloadService>();
            var fileName = downloadService.DownloadToTemp(conflict.BaseDowloadUrl);
            var doc = IdeApp.Workbench.OpenDocument(fileName, (Project)null);
            doc.Window.ViewContent.ContentName = Path.GetFileName(conflict.TargetLocalItem) + " - v" + conflict.BaseVersion;
            doc.Closed += (o, e) =>
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
            };
        }

        private void ViewTheirClicked()
        {
            var conflict = listStore.GetValue(listView.SelectedRow, itemField);
            var downloadService = this.repository.VersionControlService.Collection.GetService<VersionControlDownloadService>();
            var fileName = downloadService.DownloadToTemp(conflict.TheirDowloadUrl);
            var doc = IdeApp.Workbench.OpenDocument(fileName, (Project)null);
            doc.Window.ViewContent.ContentName = Path.GetFileName(conflict.TargetLocalItem) + " - v" + conflict.TheirVersion;
            doc.Closed += (o, e) =>
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
            };
        }

        private void AcceptYoursClicked()
        {
            var conflict = listStore.GetValue(listView.SelectedRow, itemField);
            this.repository.Resolve(conflict, ResolutionType.AcceptYours);
            this.LoadConflicts();
        }

        private void AcceptTheirsClicked()
        {
            var conflict = listStore.GetValue(listView.SelectedRow, itemField);
            this.repository.Resolve(conflict, ResolutionType.AcceptTheirs);
            this.LoadConflicts();
        }

        private void AcceptMergeClicked()
        {
            var mergeToolInfo = TFSVersionControlService.Instance.MergeToolInfo;
            if (mergeToolInfo == null)
            {
                using (var mergeToolConfigDialog = new MergeToolConfigDialog())
                {
                    if (mergeToolConfigDialog.Run(this.Widget.ParentWindow) == Command.Ok)
                    {
                        TFSVersionControlService.Instance.MergeToolInfo = mergeToolConfigDialog.MergeToolInfo;
                        TFSVersionControlService.Instance.StorePrefs();
                        if (TFSVersionControlService.Instance.MergeToolInfo != null)
                            StartMerging();
                    }
                }
            }
            else
            {
                StartMerging();
            }
        }

        private void StartMerging()
        {
            var conflict = listStore.GetValue(listView.SelectedRow, itemField);
            var downloadService = this.repository.VersionControlService.Collection.GetService<VersionControlDownloadService>();

            var baseFile = downloadService.DownloadToTemp(conflict.BaseDowloadUrl);
            var theirsFile = downloadService.DownloadToTemp(conflict.TheirDowloadUrl);

            var mergeToolInfo = TFSVersionControlService.Instance.MergeToolInfo;
            var arguments = mergeToolInfo.Arguments;
            arguments = arguments.Replace("%1", "\"" + conflict.TargetLocalItem + "\"");
            arguments = arguments.Replace("%2", "\"" + baseFile + "\"");
            arguments = arguments.Replace("%3", "\"" + theirsFile + "\"");

            arguments = arguments.Replace("%4", "Local");
            arguments = arguments.Replace("%5", "Base");
            arguments = arguments.Replace("%6", "Their");

            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = mergeToolInfo.CommandName;
            info.Arguments = arguments;
            var process = System.Diagnostics.Process.Start(info);
            process.WaitForExit();
            //Move merged base file to target.
            if (File.Exists(conflict.TargetLocalItem))
                File.Delete(conflict.TargetLocalItem);

            File.Move(baseFile, conflict.TargetLocalItem);

            if (File.Exists(theirsFile))
                File.Delete(theirsFile);

            FileService.NotifyFileChanged(conflict.TargetLocalItem);

            EndMerging();
        }

        private void EndMerging()
        {
            var conflict = listStore.GetValue(listView.SelectedRow, itemField);
            this.repository.Resolve(conflict, ResolutionType.AcceptMerge);
            LoadConflicts();
        }

        #endregion

        public override void Load(string fileName)
        {
            throw new NotSupportedException();
        }

        private void LoadConflicts()
        {
            if (paths.Count == 0)
                return;
            var conflicts = repository.GetConflicts(paths);
            listStore.Clear();
            foreach (var conflict in conflicts)
            {
                var row = this.listStore.AddRow();
                this.listStore.SetValue(row, itemField, conflict);
                this.listStore.SetValue(row, typeField, conflict.ConflictType.ToString());
                var path = ((FilePath)conflict.TargetLocalItem).ToRelative(paths[0]);
                this.listStore.SetValue(row, nameField, path);
                this.listStore.SetValue(row, versionBaseField, conflict.BaseVersion);
                this.listStore.SetValue(row, versionTheirField, conflict.TheirVersion);
                this.listStore.SetValue(row, versionYourField, conflict.YourVersion);
            }
        }

        public override Widget Widget
        {
            get
            {
                return view;
            }
        }

        public static void Open(TFSRepository repository, List<FilePath> paths)
        {
            foreach (var view in IdeApp.Workbench.Documents)
            {
                var sourceDoc = view.GetContent<ResolveConflictsView>();
                if (sourceDoc != null)
                {
                    sourceDoc.SetData(repository, paths);
                    sourceDoc.LoadConflicts();
                    view.Window.SelectWindow();
                    return;
                }
            }

            ResolveConflictsView resolveConflictsView = new ResolveConflictsView();
            resolveConflictsView.SetData(repository, paths);
            resolveConflictsView.LoadConflicts();
            IdeApp.Workbench.OpenDocument(resolveConflictsView, true);
        }
    }
}

