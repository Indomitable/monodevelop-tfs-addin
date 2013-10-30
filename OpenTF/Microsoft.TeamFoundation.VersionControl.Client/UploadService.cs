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
using System.Xml;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
	public class UploadService: TfsService
	{
		const string newLine = "\r\n";

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

		private void AddParam(StreamWriter writer, string paramName, string paramValue, string boundary)
		{
			writer.Write("Content-Disposition: form-data; name=\"" + paramName + "\"" + newLine + newLine);
			writer.Write(paramValue);
			writer.Write(newLine);
			writer.Write(boundary);
		}

		public async Task UploadFile(string workspaceName, string workspaceOwner, PendingChange change)
		{
			await UploadFile4(workspaceName, workspaceOwner, change);
		}

		public void UploadFile1(string workspaceName, string workspaceOwner, PendingChange change)
		{
			var request = (HttpWebRequest)WebRequest.Create(this.Url);
			request.Method = "POST";
			request.Credentials = this.Collection.Server.Credentials;
			request.AllowWriteStreamBuffering = true;
			var boundary = "------------" + DateTime.Now.Ticks.ToString("x");
			request.ContentType = "multipart/form-data; boundary=" + boundary;
			boundary = "--" + boundary + newLine;

			using (var requestStream = request.GetRequestStream())
			{
				using (var writer = new StreamWriter(requestStream))
				{
					writer.Write(boundary);
					var content = File.ReadAllBytes(change.LocalItem);
					// Write the values
					this.AddParam(writer, "item", change.ServerItem, boundary);
					this.AddParam(writer, "wsname", workspaceName, boundary);
					this.AddParam(writer, "wsowner", workspaceOwner, boundary);
					this.AddParam(writer, "filelength", content.Length.ToString(), boundary);
					this.AddParam(writer, "hash", Convert.ToBase64String(change.UploadHashValue), boundary);


					writer.Write(@"Content-Disposition: form-data; name=""content""; filename=""item""" + newLine);
					writer.Write("Content-Type: application/octet-stream" + newLine + newLine);
					writer.Flush();
					writer.BaseStream.Write(content, 0, content.Length);

					writer.Write(newLine);
					writer.Write(boundary);
					writer.Flush();
				}
				requestStream.Close();
			}

			request.GetResponse();
		}

		public void UploadFile2(string workspaceName, string workspaceOwner, PendingChange change)
		{
			NameValueCollection parameters = new NameValueCollection();
			parameters.Add("item", change.ServerItem);
			parameters.Add("wsname", workspaceName);
			parameters.Add("wsowner", workspaceOwner);
			parameters.Add("filelength", new FileInfo(change.LocalItem).Length.ToString());
			parameters.Add("hash", Convert.ToBase64String(change.UploadHashValue));

			this.HttpUploadFile(this.Url, change.LocalItem, "content", "application/octet-stream", parameters);
		}

		private void HttpUploadFile(Uri url, string file, string paramName, string contentType, NameValueCollection nvc)
		{
			string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
			byte[] boundarybytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

			HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
			wr.ContentType = "multipart/form-data; boundary=" + boundary;
			wr.Headers.Add("X-TFS-Version", "1.0.0.0");
			wr.Headers.Add("accept-language", "en-US");
//			wr.Headers.Add("X-VersionControl-Instance", "ac4d8821-8927-4f07-9acf-adbf71119886, Checkin");
			wr.Method = "POST";
			wr.KeepAlive = true;
			wr.Credentials = this.Collection.Server.Credentials;
			wr.AllowWriteStreamBuffering = true;

			using (Stream rs = wr.GetRequestStream())
			{
				string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
				foreach (string key in nvc.Keys)
				{
					rs.Write(boundarybytes, 0, boundarybytes.Length);
					string formitem = string.Format(formdataTemplate, key, nvc[key]);
					byte[] formitembytes = Encoding.UTF8.GetBytes(formitem);
					rs.Write(formitembytes, 0, formitembytes.Length);
				}
				rs.Write(boundarybytes, 0, boundarybytes.Length);

				string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
				string header = string.Format(headerTemplate, paramName, /*file*/"item", contentType);
				byte[] headerbytes = Encoding.UTF8.GetBytes(header);
				rs.Write(headerbytes, 0, headerbytes.Length);

				using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
				{
					byte[] buffer = new byte[4096];
					int bytesRead = 0;
					while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
					{
						rs.Write(buffer, 0, bytesRead);
					}
					fileStream.Close();
				}
				byte[] trailer = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");
				rs.Write(trailer, 0, trailer.Length);
				rs.Close();
			}
			wr.GetResponse();
		}

		public void UploadFile3(string workspaceName, string workspaceOwner, PendingChange change)
		{
			WebClient client = new WebClient();
			client.Credentials = this.Collection.Server.Credentials;
			client.Headers["item"] = change.ServerItem; 
			client.Headers["wsname"] = workspaceName;
			client.Headers["wsowner"] = workspaceOwner;
			client.Headers["filelength"] = new FileInfo(change.LocalItem).Length.ToString();
			client.Headers["hash"] = Convert.ToBase64String(change.UploadHashValue);
			client.UploadFile(this.Url, "POST", change.LocalItem);
		}

		public async Task UploadFile4(string workspaceName, string workspaceOwner, PendingChange change)
		{
			using (var handler = new HttpClientHandler())
			{
				handler.Credentials = this.Collection.Server.Credentials;
				using (var client = new HttpClient(handler))
				{
					using (var f = File.OpenRead(change.LocalItem))
					{
						using (var content = new StreamContent(f))
						{
							var mpcontent = new MultipartFormDataContent();
							content.Headers.Add("item", change.ServerItem);
							content.Headers.Add("wsname", workspaceName);
							content.Headers.Add("wsowner", workspaceOwner);
							content.Headers.Add("filelength", new FileInfo(change.LocalItem).Length.ToString());
							content.Headers.Add("hash", Convert.ToBase64String(change.UploadHashValue));
							content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
							mpcontent.Add(content);   
							await client.PostAsync(this.Url, mpcontent);
						}
					}
				}
			}
		}
	}
}

