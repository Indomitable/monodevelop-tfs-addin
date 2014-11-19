//
// SoapInvoker.cs
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
using System.Xml.Linq;
using System.Xml.Schema;
using System.IO;
using System.Text;
using Microsoft.TeamFoundation.Client.Services;

namespace Microsoft.TeamFoundation.Client
{
    public class SoapEnvelope
    {
        public XElement Header { get; set; }

        public XElement Body { get; set; }
    }

    public class SoapInvoker
    {
        readonly XNamespace xsiNs = XmlSchema.InstanceNamespace;
        readonly XNamespace xsdNs = XmlSchema.Namespace;
        readonly XNamespace soapNs = "http://schemas.xmlsoap.org/soap/envelope/";
        readonly static object locker = new object();

        readonly TFSService service;
        readonly XNamespace messagegNs;
        readonly Uri url;
        readonly XDocument document;
        string methodName;

        public SoapInvoker(TFSService service)
        {
            this.service = service;
            this.url = service.Url;
            this.messagegNs = service.MessageNs;
            this.document = new XDocument(
                new XDeclaration("1.0", "utf-8", "no"),
                new XElement(soapNs + "Envelope", 
                    new XAttribute(XNamespace.Xmlns + "xsi", xsiNs),
                    new XAttribute(XNamespace.Xmlns + "xsd", xsdNs),
                    new XAttribute(XNamespace.Xmlns + "soap", soapNs)));
        }

        public XElement CreateEnvelope(string methodName)
        {
            this.methodName = methodName;
            var innerMessage = new XElement(messagegNs + methodName);
            this.document.Root.Add(new XElement(soapNs + "Body", innerMessage));
            return innerMessage;
        }

        public SoapEnvelope CreateEnvelope(string methodName, string headerName)
        {
            this.methodName = methodName;
            var headerMessage = new XElement(messagegNs + headerName);
            var bodyMessage = new XElement(messagegNs + methodName);
            this.document.Root.Add(new XElement(soapNs + "Header", headerMessage));
            this.document.Root.Add(new XElement(soapNs + "Body", bodyMessage));
            return new SoapEnvelope { Header = headerMessage, Body = bodyMessage };
        }

        public XElement MethodResultExtractor(XElement responseElement)
        {
            return responseElement.Element(this.messagegNs + (this.methodName + "Result"));
        }

        public XElement InvokeResult()
        {
            var responseElement = InvokeResponse();
            return MethodResultExtractor(responseElement);
        }

        private FileStream GetLogFileStream()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "TFS.VersionControl.Debug.log");
            return File.Open(path, FileMode.OpenOrCreate, FileAccess.Write);
        }

        public XElement InvokeResponse()
        {
            var logBuilder = new StringBuilder();
            logBuilder.AppendLine("Date: " + DateTime.Now.ToString("s"));
            logBuilder.AppendLine("Request:");
            logBuilder.AppendLine(this.ToString());

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                if (service.Server is INetworkServer)
                {
                    var server = (INetworkServer)service.Server;
                    logBuilder.AppendLine("Domain: " + server.Credentials.Domain);
                    logBuilder.AppendLine("UserName: " + server.Credentials.UserName);
                    logBuilder.AppendLine("Password: " + server.Credentials.Password);

                    request.Credentials = server.Credentials;
                }
                else if (service.Server is IAuthServer) 
                {
                    var server = (IAuthServer)service.Server;
                    request.Headers.Add(HttpRequestHeader.Authorization, server.AuthString);
                }
                else
                {
                    throw new Exception("Known server");
                }
                request.AllowWriteStreamBuffering = true;
                request.Method = "POST";
                request.ContentType = "text/xml; charset=utf-8";
                request.Headers["SOAPAction"] = messagegNs.NamespaceName.TrimEnd('/') + '/' + this.methodName;

                this.document.Save(request.GetRequestStream());

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(responseStream, Encoding.UTF8))
                        {
                            var responseTxt = sr.ReadToEnd();
                            logBuilder.AppendLine("Response:");

                            if (response.StatusCode != HttpStatusCode.OK)
                            {
                                logBuilder.AppendLine(responseTxt);
                                throw new Exception("Error!!!\n" + responseTxt);
                            }
                            else
                            {
                                var resultDocument = XDocument.Parse(responseTxt);
                                logBuilder.AppendLine(resultDocument.ToString());
                                return resultDocument.Root.Element(soapNs + "Body").Element(this.messagegNs + (this.methodName + "Response"));
                            }
                        }
                    }
                }
            }
            catch (WebException wex)
            {
                if (wex.Response != null && wex.Response.ContentType.IndexOf("xml", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    XDocument doc = XDocument.Load(wex.Response.GetResponseStream());
                    logBuilder.AppendLine(doc.ToString());
                    throw new Exception(doc.ToString()); 
                }
                else
                {
                    logBuilder.AppendLine(wex.Message);
                    logBuilder.AppendLine(wex.StackTrace);
                    throw;
                }
            }
            catch (Exception ex)
            {
                logBuilder.AppendLine(ex.Message);
                logBuilder.AppendLine(ex.StackTrace);
                throw;
            }
            finally
            {
                if (service.Server.IsDebuMode)
                {
                    lock (locker)
                    {
                        using (var stream = GetLogFileStream())
                        {
                            stream.Seek(0, SeekOrigin.End);
                            byte[] bytes = Encoding.UTF8.GetBytes(logBuilder.ToString());
                            stream.Write(bytes, 0, bytes.Length);
                            stream.Flush();
                        }
                    }
                }
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(document.ToString());
            return builder.ToString();
        }
    }
}
