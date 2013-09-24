//
// Microsoft.TeamFoundation.VersionControl.Client.DownloadFile
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
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Xml;
using System.Web.Services;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
	internal static class DownloadFile
	{
		public static void WriteTo(string fileName, Repository repository,
															 Uri artifactUri)
		{
			int n; 
			byte[] bytes = new byte[4096];

			// setup request
			HttpWebRequest request = (HttpWebRequest) WebRequest.Create(artifactUri);
			request.Credentials = repository.Credentials;
			request.Timeout = 90000;
			//request.KeepAlive = false;
			//request.ProtocolVersion = HttpVersion.Version10;

			// write output to file
			string tempFileName = Path.GetTempFileName();
			string contentType;
			using (WebResponse response = request.GetResponse())
				{
					contentType = response.ContentType;
					using (Stream stream = response.GetResponseStream())
						{
							using (BinaryWriter writer = new BinaryWriter(File.Create(tempFileName)))
								{
									while ((n = stream.Read(bytes, 0, bytes.Length)) != 0) {
										writer.Write(bytes, 0, n);
									}
								}
						}
				}

			// clear out old file as needed
			if (File.Exists(fileName))
			{
				File.SetAttributes(fileName, FileAttributes.Normal);
				File.Delete(fileName);
			}

			// do we need to decompress the data?
			if (contentType != "application/gzip") File.Move(tempFileName, fileName);
			else
				{
					// note: mono stackdumped when i tried to do GZipStream over response.GetResponseStream
					// with thousands of files (see: http://lists.ximian.com/pipermail/mono-devel-list/2007-March/022712.html)
					// so this may not be as fast, but it works
					using (GZipStream stream = new GZipStream(File.OpenRead(tempFileName), CompressionMode.Decompress))
						{
							using (BinaryWriter writer = new BinaryWriter(File.Create(fileName)))
								{
									while ((n = stream.Read(bytes, 0, bytes.Length)) != 0) {
										writer.Write(bytes, 0, n);
									}										
								}
						}

					File.Delete(tempFileName);
				}
		}
	}
}