//
// Microsoft.TeamFoundation.VersionControl.Client.ConflictEventArgs
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

namespace Microsoft.TeamFoundation.VersionControl.Client
{
	public class ConflictEventArgs : System.EventArgs
	{
		private Workspace workspace;
		private string message;
		private string serverItem;

		internal ConflictEventArgs(Workspace workspace, string message,
															 string serverItem)
			{
				this.workspace = workspace;
				this.message = message;
				this.serverItem = serverItem;
			}

		public Workspace Workspace 
		{
			get { return workspace; }
		}

		public string Message
		{
			get { return message; }
		}

		public string ServerItem
		{
			get { return serverItem; }
		}
	}
}

