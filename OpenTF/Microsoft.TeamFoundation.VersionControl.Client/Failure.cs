//
// Microsoft.TeamFoundation.VersionControl.Client.Failure
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
	public sealed class Failure
	{
		private RequestType requestType;
		private string code;
		private string computerName;
		private string identityName;
		private string localItem;
		private string message;		
		private string serverItem;
		private string resourceName;
		private string workspaceOwner;
		private string workspaceName;

		internal static Failure FromXml(Repository repository, XmlReader reader)
		{
			Failure failure = new Failure();
			string requestType = reader.GetAttribute("req");
			if (!String.IsNullOrEmpty(requestType))
				{
					failure.requestType = (RequestType) Enum.Parse(typeof(RequestType), requestType, true);
				}

			failure.code = reader.GetAttribute("code");
			failure.localItem = TfsPath.ToPlatformPath(reader.GetAttribute("local"));

			string elementName = reader.Name;

			while (reader.Read())
				{
					if (reader.NodeType == XmlNodeType.EndElement && reader.Name == elementName)
						break;

					if (reader.NodeType == XmlNodeType.Element && reader.Name == "Message")
							failure.message = reader.ReadString();
				}

			return failure;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("Failure instance ");
			sb.Append(GetHashCode());

			sb.Append("\n	 Message: ");
			sb.Append(Message);

			sb.Append("\n	 Local Item: ");
			sb.Append(LocalItem);

			sb.Append("\n	 Request Type: ");
			sb.Append(RequestType);

			return sb.ToString();
		}

		public RequestType RequestType
		{
			get { return requestType; }
		}

		public string Code
		{
			get { return code; }
		}

		public string ComputerName
		{
			get { return computerName; }
		}

		public string IdentityName
		{
			get { return identityName; }
		}

		public string LocalItem
		{
			get { return localItem; }
		}

		public string Message
		{
			get { return message; }
		}

		public string ServerItem
		{
			get { return serverItem; }
		}

		public string ResourceName
		{
			get { return resourceName; }
		}

		public string WorkspaceOwner
		{
			get { return workspaceOwner; }
		}

		public string WorkspaceName
		{
			get { return workspaceName; }
		}

	}
}

