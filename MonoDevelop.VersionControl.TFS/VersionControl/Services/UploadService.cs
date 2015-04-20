// UploadService.cs
// 
// Author:
//       Ventsislav Mladenov
// 
// The MIT License (MIT)
// 
// Copyright (c) 2013-2015 Ventsislav Mladenov
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
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using MonoDevelop.VersionControl.TFS.Core.Services;
using MonoDevelop.VersionControl.TFS.Core.Services.Resolvers;
using MonoDevelop.VersionControl.TFS.VersionControl.Infrastructure;
using MonoDevelop.VersionControl.TFS.VersionControl.Services.Resolvers;

namespace MonoDevelop.VersionControl.TFS.VersionControl.Services
{
    [ServiceResolver(typeof(UploadServiceResolver))]
    internal sealed class UploadService: TFSService
    {
        private UploadService(Uri baseUri, string servicePath)
            : base(baseUri, servicePath)
        {
            
        }

        const string NewLine = "\r\n";
        const string Boundary = "----------------------------8e5m2D6l5Q4h6";
        const int ChunkSize = 512 * 1024; //Chunk Size 512 K
        private static readonly string uncompressedContentType = "application/octet-stream";
//        private static readonly string compressedContentType = "application/gzip";



        #region implemented abstract members of TfsService

        public override XNamespace MessageNs
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IServiceResolver ServiceResolver
        {
            get
            {
                return new UploadServiceResolver();
            }
        }

        #endregion

        private void CopyStream(Stream source, Stream destination)
        {
            byte[] buffer = new byte[ChunkSize];
            int cnt;
            while((cnt = source.Read(buffer, 0, ChunkSize)) > 0)
            {
                destination.Write(buffer, 0, cnt);
            }
            destination.Flush();
        }

        private void CopyBytes(byte[] source, Stream destination)
        {
            using (var memorySource = new MemoryStream(source))
            {
                CopyStream(memorySource, destination);
            }
        }

        public async Task UploadFileAsync(string workspaceName, string workspaceOwner, CommitItem item)
        {
            var fileContent = File.ReadAllBytes(item.LocalPath);
            var fileHash = Hash(fileContent);
            string contentType = uncompressedContentType;

            using (var memory = new MemoryStream(fileContent))
            {
                byte[] buffer = new byte[ChunkSize];
                int cnt;
                while((cnt = memory.Read(buffer, 0, ChunkSize)) > 0)
                {
                    var range = GetRange(memory.Position - cnt, memory.Position, fileContent.Length);
                    await UploadPart(item.RepositoryPath, workspaceName, workspaceOwner, fileContent.Length, fileHash, range, contentType, buffer, cnt);
                }
            }
        }

        private void AddContent(MultipartFormDataContent container, string value, string name, string fileName = "")
        {
            if (string.IsNullOrEmpty(fileName))
                container.Add(new ByteArrayContent(Encoding.UTF8.GetBytes(value)), name);
            else
                container.Add(new ByteArrayContent(Encoding.UTF8.GetBytes(value)), name, fileName);
        }

        private async Task<HttpResponseMessage> UploadPart(string fileName, string workspaceName, string workspaceOwner, int fileSize, string fileHash, string range, string contentType, byte[] bytes, int copyBytes)
        {
            var handler = new HttpClientHandler { PreAuthenticate = true };
            var message = new HttpRequestMessage(HttpMethod.Post, this.Url);
            this.Server.Authorization.Authorize(handler, message);

            HttpClient client = new HttpClient(handler);
            
            var content = new MultipartFormDataContent(Boundary);
            AddContent(content, fileName, "item");
            AddContent(content, workspaceName, "wsname");
            AddContent(content, workspaceOwner, "wsowner");
            AddContent(content, fileSize.ToString(), "filelength");
            AddContent(content, fileHash, "hash");
            AddContent(content, range, "range");

            var fileContent = new ByteArrayContent(bytes, 0, copyBytes);
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "content",
                FileName = "item"
            };
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            content.Add(fileContent);

            message.Content = content;
            return await client.SendAsync(message);
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

        private string GetRange(long start, long end, long length)
        {
            var builder = new StringBuilder(100);
            builder.Append("bytes=");
            builder.Append(start);
            builder.Append('-');
            builder.Append(end - 1);
            builder.Append('/');
            builder.Append(length);
            builder.AppendLine();
            return builder.ToString();
        }
    }
}

