//
// Microsoft.TeamFoundation.VersionControl.Client.GetOperation
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
using System.Net;
using System.Text;
using System.Xml;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
	internal class GetOperation : ILocalUpdateOperation
	{
		public ChangeType ChangeType
		{
			get { return chg; }
		}

		public int DeletionId 
		{
			get { return did; }
		}

		public int ItemId
		{
			get { return itemId; }
		}

		public ItemType ItemType
		{
			get { return itemType; }
		}

		public string TargetLocalItem
		{
			get { return tlocal; }
		}

		public string SourceLocalItem
		{
			get { return slocal; }
		}

		public string TargetServerItem
		{
			get { return titem; }
		}

		public int VersionLocal
		{
			get { return lver; }
		}

		public int VersionServer
		{
			get { return sver; }
		}

		public Uri ArtifactUri
		{
			get { return artifactUri; }
		}

		private ItemType itemType = ItemType.File;
		private int itemId = 0;
		private int did = 0;

		private string slocal;
		private string tlocal;
		private string titem;
		private int sver = 0;
		private int lver = 0;
		private ChangeType chg = ChangeType.None;
		//private LockLevel @lock = LockLevel.None;
		//private bool il = true;
		//private int pcid = 0;
		//private bool cnflct = false;
		//private int cnflctitemid = 0;
		private Uri artifactUri;
		//private System.Byte[] hashValue;

		internal static GetOperation FromXml(string itemUrl, XmlReader reader)
		{
			GetOperation getOperation = new GetOperation();
			string elementName = reader.Name;

			string stype = reader.GetAttribute("type");
			if (!String.IsNullOrEmpty(stype))
				getOperation.itemType = (ItemType) Enum.Parse(typeof(ItemType), stype, true);

			getOperation.itemId = Convert.ToInt32(reader.GetAttribute("itemid"));
			getOperation.slocal = TfsPath.ToPlatformPath(reader.GetAttribute("slocal"));
			getOperation.tlocal = TfsPath.ToPlatformPath(reader.GetAttribute("tlocal"));

			getOperation.titem = reader.GetAttribute("titem");
			getOperation.sver = Convert.ToInt32(reader.GetAttribute("sver"));
			getOperation.lver = Convert.ToInt32(reader.GetAttribute("lver"));

			string chgAttr = reader.GetAttribute("chg");
			if (!String.IsNullOrEmpty(chgAttr))
				getOperation.chg = (ChangeType) Enum.Parse(typeof(ChangeType), chgAttr.Replace(" ", ","), true);

			// setup download url if found
			string durl = reader.GetAttribute("durl");
			if (!String.IsNullOrEmpty(durl))
				getOperation.artifactUri = new Uri(String.Format("{0}?{1}", itemUrl, durl));

			// here's what you get if you remap a working folder from one
			// team project to another team project with the same file
			// first you get the update getOperation, then you get this later on
			// <GetOperation type="File" itemid="159025" slocal="foo.xml" titem="$/bar/foo.xml" lver="12002"><HashValue /></GetOperation>

			// look for a deletion id
			getOperation.did = Convert.ToInt32(reader.GetAttribute("did"));

			while (reader.Read())
				{
					if (reader.NodeType == XmlNodeType.EndElement && reader.Name == elementName)
						break;

					if (reader.NodeType == XmlNodeType.Element && (!reader.IsEmptyElement))
						{
							switch (reader.Name)
								{
								case "HashValue":
									break;
								}
						}
				}

			return getOperation;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("GetOperation instance ");
			sb.Append(GetHashCode());

			sb.Append("\n	 type: ");
			sb.Append(ItemType.ToString());

			sb.Append("\n	 itemid: ");
			sb.Append(ItemId);

			sb.Append("\n	 slocal: ");
			sb.Append(slocal);

			sb.Append("\n	 tlocal: ");
			sb.Append(tlocal);

			sb.Append("\n	 titem: ");
			sb.Append(titem);

			sb.Append("\n	 sver: ");
			sb.Append(sver);

			sb.Append("\n	 lver: ");
			sb.Append(lver);

			sb.Append("\n	 did: ");
			sb.Append(DeletionId);

			sb.Append("\n	 ArtifactUri: ");
			sb.Append(artifactUri);

			sb.Append("\n	 ChangeType: ");
			sb.Append(ChangeType.ToString());

			return sb.ToString();
		}
	}
}

// 			if (getOperation.did != 0) getOperation.chg = ChangeType.Delete;
// 			//else if (getOperation.sver == 0) getOperation.chg = ChangeType.None;
// 			else
// 				{
// 					if (String.IsNullOrEmpty(getOperation.slocal))
// 						getOperation.chg = ChangeType.Add;
// 					else
// 						{
// 							// seems to be only way to tell if this is a rename operation
// 							if ((!String.IsNullOrEmpty(getOperation.tlocal))&&
// 									(getOperation.slocal != getOperation.tlocal))
// 								getOperation.chg = ChangeType.Rename;
// 						}
// 				}
