// FailuresDisplayDialog.cs
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

using System.Collections.Generic;
using Gtk;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.VersionControl.TFS.VersionControl.Models;

namespace MonoDevelop.VersionControl.TFS.GUI.VersionControl
{
    public class FailuresDisplayDialog : Dialog
    {
        readonly TreeView failuresView = new TreeView();
        readonly ListStore failuresStore;

        protected FailuresDisplayDialog(System.IntPtr raw)
            : base(raw)
        {
            
        }

        private FailuresDisplayDialog()
        {
            failuresStore = new ListStore(typeof(string), typeof(string), typeof(Failure));
            BuildGui();
        }

        void BuildGui()
        {
            this.Title = GettextCatalog.GetString("Messages");
            var lbl = new Label(this.Title + ":");
            var align = new Alignment(0, 0, 0, 0);
            lbl.Justify = Justification.Left;
            align.Add(lbl);
            this.VBox.PackStart(align, false, false, 0);
            failuresView.WidthRequest = 300;
            failuresView.HeightRequest = 200;
            failuresView.AppendColumn("Type", new CellRendererText(), "text", 0);
            failuresView.AppendColumn("Message", new CellRendererText(), "text", 1);
            failuresView.HasTooltip = true;
            failuresView.QueryTooltip += OnQueryTooltip;
            failuresView.Model = failuresStore;
            this.VBox.PackStart(failuresView, true, true, 0);
            this.AddButton(Stock.Ok, ResponseType.Ok);
            this.ShowAll();
        }

        void FillData(ICollection<Failure> failures)
        {
            failuresStore.Clear();
            foreach (var item in failures)
            {
                failuresStore.AppendValues(item.SeverityType.ToString(), item.Message, item);
            }
        }

        void OnQueryTooltip(object o, QueryTooltipArgs args)
        {
            int binX;
            int binY;
            failuresView.ConvertWidgetToBinWindowCoords(args.X, args.Y, out binX, out binY);
            TreePath path;
            TreeIter iter;
            if (failuresView.GetPathAtPos(binX, binY, out path) && failuresStore.GetIter(out iter, path))
            {
                string message = (string)failuresStore.GetValue(iter, 1);
                args.Tooltip.Text = message;
                args.RetVal = true;
            }
        }

        public static void ShowFailures(ICollection<Failure> failures)
        {
            if (failures == null || failures.Count == 0)
                return;
            var dialog = new FailuresDisplayDialog();
            dialog.FillData(failures);
            //Leave Destroy to Message Service.
            MessageService.ShowCustomDialog(dialog);
        }
    }
}

