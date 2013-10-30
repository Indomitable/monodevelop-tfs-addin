//
// UploadService.cs
//
// Author:
//       Ventsislav Mladenov <vmladenov.mladenov@gmail.com>
//
// Copyright (c) 2013 Ventsislav Mladenov
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
using System;
using Microsoft.TeamFoundation.Client;
using System.Net;
using System.Globalization;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Collections.Specialized;
using System.IO.Compression;
using System.Diagnostics;
using System.Security.Cryptography;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
	public class UploadService: TfsService
	{
		const string newLine = "\r\n";
		const string boundary = "----------------------------8e5m2D6l5Q4h6";
		private static string uncompressedContentType = "application/octet-stream";
		private static string compressedContentType = "application/gzip";

		class UploadServiceResolver : IServiceResolver
		{
			public string Id
			{
				get
				{
					return "1c04c122-7ad1-4f02-87ba-979b9d278bee";
				}
			}

			public string ServiceType
			{
				get
				{
					return "Upload";
				}
			}
		}

		#region implemented abstract members of TfsService

		public override System.Xml.Linq.XNamespace MessageNs
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override IServiceResolver ServiceResolver
		{
			get
			{
				return new UploadServiceResolver();
			}
		}

		#endregion

		public void UploadFile(string workspaceName, string workspaceOwner, PendingChange change)
		{
			var request = (HttpWebRequest)WebRequest.Create(this.Url);
			request.Method = "POST";
			request.Credentials = this.Collection.Server.Credentials;
			request.AllowWriteStreamBuffering = true;
			request.ContentType = "multipart/form-data; boundary=" + boundary.Substring(2);

			var fileContent = File.ReadAllBytes(change.LocalItem);
			var fileHash = Hash(fileContent);
			var template = GetTemplate();
			var content = string.Format(template, change.ServerItem, workspaceName, workspaceOwner, fileContent.Length, fileHash, GetRange(fileContent.Length), "item", uncompressedContentType);
			var contentBytes = Encoding.UTF8.GetBytes(content.Replace(Environment.NewLine, newLine));
			using (var stream = new MemoryStream())
			{
				stream.Write(contentBytes, 0, contentBytes.Length);
				stream.Write(fileContent, 0, fileContent.Length);
				var footContent = Encoding.UTF8.GetBytes(newLine + boundary + "--" + newLine);
				stream.Write(footContent, 0, footContent.Length);
				stream.Flush();
				contentBytes = stream.ToArray();
			}

			request.ContentLength = contentBytes.Length;

			using (var requestStream = request.GetRequestStream())
			{
				requestStream.Write(contentBytes, 0, contentBytes.Length);
				requestStream.Close();
			}

			request.GetResponse();

		}

		private string GetTemplate()
		{
			var builder = new StringBuilder();
			builder.AppendLine(boundary);
			builder.Append("Content-Disposition: form-data; name=\"");
			builder.Append("item");
			builder.AppendLine("\"");
			builder.AppendLine();
			builder.Append("{0}");
			builder.AppendLine();
			builder.AppendLine(boundary);
			builder.Append("Content-Disposition: form-data; name=\"");
			builder.Append("wsname");
			builder.AppendLine("\"");
			builder.AppendLine();
			builder.Append("{1}");
			builder.AppendLine();
			builder.AppendLine(boundary);
			builder.Append("Content-Disposition: form-data; name=\"");
			builder.Append("wsowner");
			builder.AppendLine("\"");
			builder.AppendLine();
			builder.Append("{2}");
			builder.AppendLine();
			builder.AppendLine(boundary);
			builder.Append("Content-Disposition: form-data; name=\"");
			builder.Append("filelength");
			builder.AppendLine("\"");
			builder.AppendLine();
			builder.Append("{3}");
			builder.AppendLine();
			builder.AppendLine(boundary);
			builder.Append("Content-Disposition: form-data; name=\"");
			builder.Append("hash");
			builder.AppendLine("\"");
			builder.AppendLine();
			builder.Append("{4}");
			builder.AppendLine();
			builder.AppendLine(boundary);
			builder.Append("Content-Disposition: form-data; name=\"");
			builder.Append("range");
			builder.AppendLine("\"");
			builder.AppendLine();
			builder.Append("{5}");
			builder.AppendLine();
			builder.AppendLine(boundary);
			builder.Append("Content-Disposition: form-data; name=\"");
			builder.Append("content");
			builder.Append("\"; filename=\"");
			builder.Append("{6}");
			builder.AppendLine("\"");
			builder.Append("Content-Type: ");
			builder.Append("{7}");
			builder.AppendLine();
			builder.AppendLine();
//			builder.Append("{8}");
//			builder.AppendLine();
//			builder.Append("----------------------------8e5m2D6l5Q4h6");
//			builder.AppendLine("--");
			return builder.ToString();
		}

		private byte[] Compress(byte[] input)
		{
			using (var memoryStream = new MemoryStream())
			{
				using (GZipStream stream = new GZipStream(memoryStream, CompressionMode.Compress))
				{
					stream.Write(input, 0, input.Length);
					stream.Flush();
				}
				return memoryStream.ToArray();
			}
		}

		private string Hash(byte[] input)
		{
			using (var md5 = new MD5CryptoServiceProvider())
			{
				return Convert.ToBase64String(md5.ComputeHash(input));
			}

		}

		private string GetRange(int length)
		{
			var builder = new StringBuilder(100);
			builder.Append("bytes=");
			builder.Append(0);
			builder.Append('-');
			builder.Append(length - 1);
			builder.Append('/');
			builder.Append(length);
			builder.AppendLine();
			return builder.ToString();
		}
	}
}

