//
// Microsoft.TeamFoundation.VersionControl.Client.ChangeRequest
//
// Authors:
//	Joel Reed (joelwreed@gmail.com)
//
// Copyright (C) 2007 Joel Reed
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

using System;
using System.IO;
using System.Xml;
using Microsoft.TeamFoundation.VersionControl.Common;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
	internal class ChangeRequest
	{
		private RequestType requestType = RequestType.None;
		private int deletionId = 0;
		private int encoding = 65001; // used to be -2
		private ItemType itemType = ItemType.Any;
		private LockLevel @lockLevel = LockLevel.None;
		private string target;
		private ItemType targetType = ItemType.Any;
		private ItemSpec item;
		private VersionSpec versionSpec = VersionSpec.Latest;

		public ChangeRequest(string path, RequestType requestType, ItemType itemType)
		{
			this.item = new ItemSpec(path, RecursionType.None);
			this.requestType = requestType;
			this.itemType = itemType;
		}

		public ChangeRequest(string path, RequestType requestType, ItemType itemType,
												 RecursionType recursion, LockLevel lockLevel)
		{
			this.item = new ItemSpec(path, recursion);
			this.requestType = requestType;
			this.itemType = itemType;
			this.lockLevel = lockLevel;
		}

		public ChangeRequest(string path, string target, RequestType requestType, ItemType itemType)
		{
			this.item = new ItemSpec(path, RecursionType.None);
			this.target = target;
			this.requestType = requestType;
			this.itemType = itemType;
		}

		public LockLevel LockLevel
		{
			get { return lockLevel; }
		}

		public ItemSpec Item
		{
			get { return item; }
		}

		public VersionSpec VersionSpec
		{
			get { return versionSpec; }
		}

		public ItemType ItemType
		{
			get { return itemType; }
			set { itemType = value; }
		}

		public ItemType TargetType
		{
			get { return targetType; }
			set { targetType = value; }
		}

		public RequestType RequestType
		{
			get { return requestType; }
			set { requestType = value; }
		}

		public int DeletionId
		{
			get { return deletionId; }
			set { deletionId = value; }
		}

		public int Encoding 
		{
			get { return encoding; }
			set { encoding = value; }
		}

		public string Target
		{
			get { return target; }
			set { target = value; }
		}

		internal void ToXml(XmlWriter writer, string element)
		{
			writer.WriteStartElement("ChangeRequest");
			writer.WriteAttributeString("req", RequestType.ToString());

			if (RequestType == RequestType.Lock || LockLevel != LockLevel.None)
				writer.WriteAttributeString("lock", LockLevel.ToString());

			if (RequestType == RequestType.Add)
				writer.WriteAttributeString("enc", Encoding.ToString());

			writer.WriteAttributeString("type", ItemType.ToString());
			//writer.WriteAttributeString("did", DeletionId.ToString());
			//writer.WriteAttributeString("targettype", TargetType.ToString());

			if (!String.IsNullOrEmpty(Target))
				{
					// convert local path specs from platform paths to tfs paths as needed
					string fxdTarget;
					if (VersionControlPath.IsServerItem(Target)) fxdTarget = Target;
					else fxdTarget = TfsPath.FromPlatformPath(Target);
					writer.WriteAttributeString("target", fxdTarget);
				}

			this.Item.ToXml(writer, "item");
			//this.VersionSpec.ToXml(writer, "vspec");

			writer.WriteEndElement();
		}

	}
}

