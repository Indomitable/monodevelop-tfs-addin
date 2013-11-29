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
        readonly XNamespace messagegNs;
        readonly Uri url;
        readonly ICredentials credentials;
        readonly XDocument document;
        string methodName;

        public SoapInvoker(TFSService service)
        {
            this.url = service.Url;
            this.credentials = service.Server.Credentials;
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

        public XElement InvokeResponse()
        {
            var request = (HttpWebRequest)WebRequest.Create(url); 
            request.Credentials = credentials;
            request.AllowWriteStreamBuffering = true;
            request.Method = "POST";
            request.ContentType = "text/xml; charset=utf-8";
            request.Headers["SOAPAction"] = messagegNs.NamespaceName.TrimEnd('/') + '/' + this.methodName;
            this.document.Save(request.GetRequestStream());
            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        using (StreamReader sr = new StreamReader(response.GetResponseStream(), new UTF8Encoding(false), false))
                        {
                            throw new Exception("Error!!!\n" + sr.ReadToEnd());
                        }
                    }
                    else
                    {
                        var resultDocument = XDocument.Load(response.GetResponseStream());
                        return resultDocument.Root.Element(soapNs + "Body").Element(this.messagegNs + (this.methodName + "Response"));
                    }
                }
            }
            catch (WebException wex)
            {
                if (wex.Response.ContentType.IndexOf("xml", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    XDocument doc = XDocument.Load(wex.Response.GetResponseStream());
                    throw new Exception(doc.ToString()); 
                }
                else
                {
                    throw;
                }
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(document.ToString());
            return builder.ToString();
        }
    }
}
