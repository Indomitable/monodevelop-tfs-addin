//
// Microsoft.TeamFoundation.VersionControl.Client.ChangeRequest
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

using Microsoft.TeamFoundation.VersionControl.Common;
using System.Xml.Linq;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
    internal class ChangeRequest
    {
        // used to be -2
        private readonly LockLevel lockLevel = LockLevel.None;
        private readonly ItemSpec item;
        private readonly VersionSpec versionSpec = VersionSpec.Latest;

        public ChangeRequest(string path, RequestType requestType, ItemType itemType)
        {
            this.item = new ItemSpec(path, RecursionType.None);
            this.RequestType = requestType;
            this.ItemType = itemType;
        }

        public ChangeRequest(string path, RequestType requestType, ItemType itemType,
                             RecursionType recursion, LockLevel lockLevel)
        {
            this.item = new ItemSpec(path, recursion);
            this.RequestType = requestType;
            this.ItemType = itemType;
            this.lockLevel = lockLevel;
        }

        public ChangeRequest(string path, string target, RequestType requestType, ItemType itemType)
        {
            this.item = new ItemSpec(path, RecursionType.None);
            this.Target = target;
            this.RequestType = requestType;
            this.ItemType = itemType;
        }

        public LockLevel LockLevel { get { return lockLevel; } }

        public ItemSpec Item { get { return item; } }

        public VersionSpec VersionSpec { get { return versionSpec; } }

        public ItemType ItemType { get; set; }

        public ItemType TargetType { get; set; }

        public RequestType RequestType { get; set; }

        public int DeletionId { get; set; }

        public int Encoding { get; set; }

        public string Target { get; set; }

        internal XElement ToXml()
        {
            var result = new XElement("ChangeRequest", 
                             new XAttribute("req", RequestType),
                             new XAttribute("type", ItemType));

            if (RequestType == RequestType.Lock || LockLevel != LockLevel.None)
                result.Add(new XAttribute("lock", LockLevel));

            if (RequestType == RequestType.Add)
                result.Add("enc", Encoding);

            if (!string.IsNullOrEmpty(Target))
            {
                // convert local path specs from platform paths to tfs paths as needed
                string fxdTarget = VersionControlPath.IsServerItem(Target) ? Target : TfsPath.FromPlatformPath(Target);
                result.Add(new XAttribute("target", fxdTarget));
            }

            result.Add(this.Item.ToXml("item"));
            return result;
        }
    }
}

