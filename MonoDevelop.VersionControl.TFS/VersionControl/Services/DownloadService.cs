//
// DownloadService.cs
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
using System.Net;
using System.IO;
using System.IO.Compression;
using MonoDevelop.VersionControl.TFS.Core.Services;
using MonoDevelop.VersionControl.TFS.Core.Services.Resolvers;
using MonoDevelop.VersionControl.TFS.VersionControl.Services.Resolvers;
using MonoDevelop.VersionControl.TFS.Core.ServerAuthentication;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace MonoDevelop.VersionControl.TFS.VersionControl.Services
{
    [ServiceResolver(typeof(DownloadServiceResolver))]
    sealed class DownloadService : TFSService
    {
        internal DownloadService(Uri baseUri, string servicePath)
            : base(baseUri, servicePath)
        {
            
        }

        #region implemented abstract members of TfsService

        public override System.Xml.Linq.XNamespace MessageNs
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
                return new DownloadServiceResolver();
            }
        }

        #endregion

        readonly Random random = new Random();

        private string GetTempFileName(string extension)
        {
            var num = random.Next();
            var tempDir = Path.Combine(Path.GetTempPath(), "TfSAddinDownload");
            if (!Directory.Exists(tempDir))
                Directory.CreateDirectory(tempDir);
            return Path.Combine(tempDir, "tfsTemp" + num.ToString("X") + extension);// Files are gzipped
        }

        public string DownloadToTemp(string artifactUri)
        {
            try
            {
                var client = new WebClient();
                this.Server.Authorization.Authorize(client);
                var tempFileName = this.GetTempFileName(".gz");
                UriBuilder bulder = new UriBuilder(this.Url);
                bulder.Query = artifactUri;
                client.DownloadFile(bulder.Uri, tempFileName);

                if (string.Equals(client.ResponseHeaders[HttpResponseHeader.ContentType], "application/gzip"))
                {
                    string newTempFileName = this.GetTempFileName(".tmp");
                    using (var inStream = new GZipStream(File.OpenRead(tempFileName), CompressionMode.Decompress))
                    {
                        using (var outStream = File.Create(newTempFileName))
                        {
                            inStream.CopyTo(outStream);
                            outStream.Flush();
                            outStream.Close();
                        }
                        inStream.Close();
                    }
                    FileHelper.FileDelete(tempFileName); //Delete zipped tmp.
                    return newTempFileName;
                }
                else
                {
                    return tempFileName;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        public string DownloadToTempWithName(string downloadUri, string fileName)
        {
            var path = this.DownloadToTemp(downloadUri);
            if (File.Exists(path))
            {
                var name = Path.GetFileName(fileName);
                var newName = Path.Combine(Path.GetDirectoryName(path), name);
                if (FileHelper.FileMove(path, newName, true))
                    return newName;
                else
                    return string.Empty;
            }
            return string.Empty;
        }

        public string Download(string path, string artifactUri)
        {
            var tempPath = this.DownloadToTemp(artifactUri);
            if (string.IsNullOrEmpty(tempPath) || !FileHelper.FileMove(tempPath, path, true))
                return string.Empty;
            return path;
        }
    }
}

