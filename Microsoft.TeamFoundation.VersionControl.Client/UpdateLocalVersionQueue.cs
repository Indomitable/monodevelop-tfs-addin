//
// Microsoft.TeamFoundation.VersionControl.Client.UpdateLocalVersionQueue
//
// Authors:
//	Joel Reed (joelwreed@gmail.com)
//  Ventsislav Mladenov (ventsislav.mladenov@gmail.com)
//
// Copyright (C) 2013 Joel Reed, Ventsislav Mladenov
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Linq;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
    internal class UpdateLocalVersion
    {
        public UpdateLocalVersion(int itemId, string targetLocalItem, int localVersion)
        {
            this.ItemId = itemId;
            this.TargetLocalItem = targetLocalItem;
            this.LocalVersion = localVersion;
        }

        public int ItemId { get; private set; }

        public string TargetLocalItem { get; private set; }

        public int LocalVersion { get; private set; }

        public XElement ToXml(XNamespace ns)
        {
            var el = new XElement(ns + "LocalVersionUpdate",
                         new XAttribute("itemid", ItemId),
                         new XAttribute("lver", LocalVersion));
            if (!string.IsNullOrEmpty(TargetLocalItem))
                el.Add(new XAttribute("tlocal", TfsPath.FromPlatformPath(TargetLocalItem)));
            return el;
        }
    }

    internal sealed class UpdateLocalVersionQueue
    {
        private readonly List<UpdateLocalVersion> updates;
        private readonly Workspace workspace;

        public UpdateLocalVersionQueue(Workspace workspace)
        {
            this.workspace = workspace;
            updates = new List<UpdateLocalVersion>();
        }

        public int Count { get { return updates.Count; } }

        public void Flush()
        {
            if (updates.Count > 0)
                workspace.VersionControlService.UpdateLocalVersion(this);
            updates.Clear();
        }

        internal void QueueUpdate(UpdateLocalVersion update)
        {
            if (!this.updates.Any(u => u.ItemId == update.ItemId && u.LocalVersion == update.LocalVersion && string.Equals(u.TargetLocalItem, update.TargetLocalItem)))
                this.updates.Add(update);
        }

        internal IEnumerable<XElement> ToXml(XNamespace ns)
        {
            yield return new XElement(ns + "workspaceName", workspace.Name);
            yield return new XElement(ns + "ownerName", workspace.OwnerName);
            yield return new XElement(ns + "updates", updates.Select(u => u.ToXml(ns)));
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("UpdateLocalVersionQueue instance ");
            sb.Append(GetHashCode());

            return sb.ToString();
        }
    }
}