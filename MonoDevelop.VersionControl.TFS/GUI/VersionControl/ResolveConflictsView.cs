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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Autofac;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Client.Enums;
using Microsoft.TeamFoundation.VersionControl.Client.Objects;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using MonoDevelop.VersionControl.TFS.Infrastructure;
using MonoDevelop.VersionControl.TFS.VersionControl;
using MonoDevelop.VersionControl.TFS.VersionControl.Structure;
using Xwt;

namespace MonoDevelop.VersionControl.TFS.GUI.VersionControl
{
    public class ResolveConflictsView : AbstractXwtViewContent
    {
        private IWorkspace workspace;
        private readonly List<LocalPath> paths = new List<LocalPath>();
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
        private TFSVersionControlService _versionControlService;

        private ResolveConflictsView()
        {
            this.ContentName = GettextCatalog.GetString("Resolve Conflicts");
            _versionControlService = DependencyInjection.Container.Resolve<TFSVersionControlService>();
            listStore = new ListStore(typeField, nameField, itemField, versionBaseField, versionTheirField, versionYourField);
            BuildGui();
        }

        private void SetData(IWorkspace workspace, List<LocalPath> paths)
        {
            this.workspace = workspace;
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
            var doc = IdeApp.Workbench.OpenDocument(new FilePath(conflict.TargetLocalItem), (MonoDevelop.Projects.Project)null);
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
            var fileName = this.workspace.DownloadToTemp(conflict.BaseDowloadUrl);
            var doc = IdeApp.Workbench.OpenDocument(fileName, (MonoDevelop.Projects.Project)null);
            doc.Window.ViewContent.ContentName = Path.GetFileName(conflict.TargetLocalItem) + " - v" + conflict.BaseVersion;
            doc.Closed += (o, e) => FileHelper.FileDelete(fileName);
        }

        private void ViewTheirClicked()
        {
            var conflict = listStore.GetValue(listView.SelectedRow, itemField);
            var fileName = this.workspace.DownloadToTemp(conflict.TheirDowloadUrl);
            var doc = IdeApp.Workbench.OpenDocument(fileName, (MonoDevelop.Projects.Project)null);
            doc.Window.ViewContent.ContentName = Path.GetFileName(conflict.TargetLocalItem) + " - v" + conflict.TheirVersion;
            doc.Closed += (o, e) => FileHelper.FileDelete(fileName);
        }

        private void AcceptYoursClicked()
        {
            var conflict = listStore.GetValue(listView.SelectedRow, itemField);
            this.workspace.Resolve(conflict, ResolutionType.AcceptYours);
            this.LoadConflicts();
        }

        private void AcceptTheirsClicked()
        {
            var conflict = listStore.GetValue(listView.SelectedRow, itemField);
            this.workspace.Resolve(conflict, ResolutionType.AcceptTheirs);
            this.LoadConflicts();
        }

        private void AcceptMergeClicked()
        {
            var mergeToolInfo = _versionControlService.MergeToolInfo;
            if (string.IsNullOrEmpty(mergeToolInfo.CommandName))
            {
                using (var mergeToolConfigDialog = new MergeToolConfigDialog())
                {
                    if (mergeToolConfigDialog.Run(this.Widget.ParentWindow) == Command.Ok)
                    {
                        _versionControlService.MergeToolInfo = mergeToolConfigDialog.MergeToolInfo;
                        if (!string.IsNullOrEmpty(_versionControlService.MergeToolInfo.CommandName))
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
            var baseFile = workspace.DownloadToTemp(conflict.BaseDowloadUrl);
            var theirsFile = workspace.DownloadToTemp(conflict.TheirDowloadUrl);

            var mergeToolInfo = _versionControlService.MergeToolInfo;
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
            FileHelper.FileMove(baseFile, conflict.TargetLocalItem, true);
            FileHelper.FileDelete(theirsFile);
            EndMerging();
        }

        private void EndMerging()
        {
            var conflict = listStore.GetValue(listView.SelectedRow, itemField);
            workspace.Resolve(conflict, ResolutionType.AcceptMerge);
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
            var conflicts = workspace.GetConflicts(paths);
            listStore.Clear();
            foreach (var conflict in conflicts)
            {
                var row = this.listStore.AddRow();
                this.listStore.SetValue(row, itemField, conflict);
                this.listStore.SetValue(row, typeField, conflict.ConflictType.ToString());
                var path = conflict.TargetLocalItem.ToRelativeOf(paths[0]);
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

        internal static void Open(IWorkspace workspace, List<LocalPath> paths)
        {
            foreach (var view in IdeApp.Workbench.Documents)
            {
                var sourceDoc = view.GetContent<ResolveConflictsView>();
                if (sourceDoc != null)
                {
                    sourceDoc.SetData(workspace, paths);
                    sourceDoc.LoadConflicts();
                    view.Window.SelectWindow();
                    return;
                }
            }

            ResolveConflictsView resolveConflictsView = new ResolveConflictsView();
            resolveConflictsView.SetData(workspace, paths);
            resolveConflictsView.LoadConflicts();
            IdeApp.Workbench.OpenDocument(resolveConflictsView, true);
        }
    }
}

