//
// FailuresDisplayDialog.cs
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
using Microsoft.TeamFoundation.VersionControl.Client.Objects;
using System.Collections.Generic;
using MonoDevelop.Core;

namespace MonoDevelop.VersionControl.TFS.GUI.VersionControl
{
    public class FailuresDisplayDialog : Dialog
    {
        readonly ListView failuresView = new ListView();
        readonly DataField<string> codeField = new DataField<string>();
        readonly DataField<string> messageField = new DataField<string>();
        readonly DataField<Failure> failureField = new DataField<Failure>();
        readonly ListStore failuresStore;

        private FailuresDisplayDialog()
        {
            failuresStore = new ListStore(codeField, messageField, failureField);
            BuildGui();
        }

        void BuildGui()
        {
            var content = new VBox();
            this.Title = GettextCatalog.GetString("Failures");
            content.PackStart(new Label(this.Title + ":"));
            failuresView.WidthRequest = 300;
            failuresView.HeightRequest = 200;
            failuresView.Columns.Add(new ListViewColumn("Code", new TextCellView(codeField)));
            failuresView.Columns.Add(new ListViewColumn("Message", new TextCellView(messageField)));
            failuresView.DataSource = failuresStore;
            this.Buttons.Add(Command.Ok);
            content.PackStart(failuresView);
            this.Content = content;
        }

        void FillData(List<Failure> failures)
        {
            failuresStore.Clear();
            foreach (var item in failures)
            {
                var row = failuresStore.AddRow();
                failuresStore.SetValue(row, codeField, item.Code);
                failuresStore.SetValue(row, messageField, item.Message);
                failuresStore.SetValue(row, failureField, item);
            }
        }

        public static void ShowFailures(List<Failure> failures)
        {
            if (failures == null || failures.Count == 0)
                return;
            using (var dialog = new FailuresDisplayDialog())
            {
                dialog.FillData(failures);
                dialog.Run();
            }
        }
    }
}

