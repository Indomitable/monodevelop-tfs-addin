//
// Microsoft.TeamFoundation.VersionControl.Client.Change
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
using System.Text;
using System.Xml;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
	public sealed class Change
	{
		private Item item;
		private ChangeType changeType;

		internal static Change FromXml(Repository repository, XmlReader reader)
		{
			Change change = new Change();

			string chgAttr = reader.GetAttribute("chg");
			if (String.IsNullOrEmpty(chgAttr))
				{
					chgAttr = reader.GetAttribute("type");
				}

			change.changeType = (ChangeType) Enum.Parse(typeof(ChangeType), chgAttr.Replace(" ", ","), true);

			reader.Read();
			change.item = Item.FromXml(repository, reader);

			return change;
		}

		internal void ToXml(XmlWriter writer, string element)
		{
			Console.WriteLine("WARNING: Change.ToXml not verified yet!");
			writer.WriteStartElement(element);
			writer.WriteElementString("ChangeType", ChangeType.ToString());
			writer.WriteElementString("Item", Item.ToString());
			writer.WriteEndElement();
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("Change instance ");
			sb.Append(GetHashCode());

			sb.Append("\n	 ChangeType: ");
			sb.Append(ChangeType);

			sb.Append("\n	 Item: ");
			sb.Append(Item.ToString());

			return sb.ToString();
		}

		public ChangeType ChangeType
		{
			get { return changeType; }
		}

		public Item Item
		{
			get { return item; }
		}
	}
}
