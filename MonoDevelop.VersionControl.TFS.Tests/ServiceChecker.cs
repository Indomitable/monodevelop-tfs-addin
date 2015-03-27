//
// ServiceChecker.cs
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
using System.Web.Services.Protocols;

namespace Tests
{
    public class ServiceChecker
    {
        public HttpWebRequest CreateRequest(string serviceUrl)
        {
            var request = (HttpWebRequest)WebRequest.Create(new Uri("https://tfs.codeplex.com/tfs" + serviceUrl)); 
            request.Credentials = new NetworkCredential { Domain = "snd", UserName = "mono_tfs_plugin_cp", Password = "mono_tfs_plugin" };
            request.AllowWriteStreamBuffering = true;
            request.Method = "POST";
            request.ContentType = "text/xml; charset=utf-8";
            return request;
        }

        public XDocument CreateEnvelope(string methodName, XNamespace messageNamespace, out XElement innerMessage)
        {
            XNamespace xsiNs = XmlSchema.InstanceNamespace;
            XNamespace xsdNs = XmlSchema.Namespace;
            XNamespace SoapNs = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace msgNs = messageNamespace;

            var document = new XDocument(new XDeclaration("1.0", "utf-8", "no"));
            innerMessage = new XElement(msgNs + methodName);
            XElement messageEl = new XElement(SoapNs + "Envelope", 
                                     new XAttribute(XNamespace.Xmlns + "xsi", xsiNs),
                                     new XAttribute(XNamespace.Xmlns + "xsd", xsdNs),
                                     new XAttribute(XNamespace.Xmlns + "soap", SoapNs),
                                     new XElement(SoapNs + "Body", innerMessage));
            document.Add(messageEl);
            return document;
        }

        public HttpWebResponse GetResponse(HttpWebRequest request)
        {
            return (HttpWebResponse)request.GetResponse();
        }
    }
}

