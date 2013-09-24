//
// Microsoft.TeamFoundation.VersionControl.Client.UploadFile
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
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Web.Services;
using System.Web.Services.Description;
using System.Web.Services.Discovery;
using System.Web.Services.Protocols;
using Microsoft.TeamFoundation.VersionControl.Common;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
	internal class UploadFile
	{
		private HttpWebRequest request;
		private string boundary;
		private StreamWriter stream;

		public UploadFile(string url, ICredentials credentials, string commandName)
		{
			request = (HttpWebRequest) WebRequest.Create(url);
			request.Method = "POST";
			request.Credentials = credentials;
			request.AllowWriteStreamBuffering = true;

			string sboundary = "------------" + DateTime.Now.Ticks.ToString ("x");
			request.ContentType = String.Format ("multipart/form-data; boundary={0}", sboundary);
			request.Headers.Add ("X-TFS-Version", "1.0.0.0");
			request.Headers.Add ("accept-language", "en-US");
			request.Headers.Add ("X-VersionControl-Instance", 
													 String.Format("ac4d8821-8927-4f07-9acf-adbf71119886, Command{0}", commandName));

			boundary = String.Format("--{0}\r\n", sboundary);
			stream = new StreamWriter(request.GetRequestStream());
			stream.Write(boundary);
		}

		public void AddValue(string name, string value)
		{
			stream.Write("Content-Disposition: form-data; name=\"" + name + "\"\r\n\r\n");
			stream.Write(value);

			stream.Write("\r\n");
			stream.Write(boundary);
		}

		public void AddFile(string filename)
		{
			stream.Write("Content-Disposition: form-data; name=\"" + RepositoryConstants.ContentField + "\"; filename=\"item\"\r\n");
			stream.Write("Content-Type: application/octet-stream\r\n\r\n");
			stream.Flush();

			int n = 0;
			byte[] bytes = new byte[4096];

			using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
				{
					using (BinaryReader reader = new BinaryReader(fs))
						{
							while ((n = reader.Read(bytes, 0, bytes.Length)) != 0) {
								stream.BaseStream.Write(bytes, 0, n);
							}
						}
				}

			stream.Write("\r\n");
			stream.Write(boundary);
		}
			
		public WebResponse Send()
		{
			stream.Close();
			return request.GetResponse();
		}
	}
}
