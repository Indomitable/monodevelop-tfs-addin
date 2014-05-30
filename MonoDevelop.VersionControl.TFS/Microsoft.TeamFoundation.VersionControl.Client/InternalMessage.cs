//
// Microsoft.TeamFoundation.VersionControl.Client.Message
//
// Authors:
//	Joel Reed (joelwreed@gmail.com)
//  Ventsislav Mladenov (ventsislav.mladenov@gmail.com)
//
// Copyright (C) 2013 Joel Reed, Ventsislav Mladenov
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

using System.Net;
using System.Xml.Schema;
using System.Xml.Linq;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
    internal class Message
    {
        private static readonly XNamespace SoapNs = "http://schemas.xmlsoap.org/soap/envelope/";
        private readonly WebRequest request;
        private readonly XDocument document;
        private readonly XElement messageElement;
        private readonly string methodName;

        public WebRequest Request { get { return request; } }

        public string MethodName { get { return methodName; } }

        public XElement ResponseReader(HttpWebResponse response)
        {
            XDocument doc = XDocument.Load(response.GetResponseStream());

            return doc.Root.Element(SoapNs + "Body").Element(XmlNamespaces.GetMessageElementName(MethodName + "Response")).Element(XmlNamespaces.GetMessageElementName(MethodName + "Result"));
        }

        public Message(WebRequest request, string methodName)
        {
            this.methodName = methodName;
            this.request = request;
            this.request.Method = "POST";

            // PreAuth can't be used with NTLM afaict
            //this.request.PreAuthenticate=true;

            // must allow buffering for NTLM authentication
            HttpWebRequest req = (HttpWebRequest)request;
            req.AllowWriteStreamBuffering = true;

            //http://blogs.x2line.com/al/archive/2005/01/04/759.aspx#780
            //req.KeepAlive = false;
            //req.ProtocolVersion = HttpVersion.Version10;

            this.request.ContentType = "text/xml; charset=utf-8";
            this.request.Headers.Add("X-TFS-Version", "1.0.0.0");
            this.request.Headers.Add("accept-language", "en-US");
            this.request.Headers.Add("X-VersionControl-Instance", "ac4d8821-8927-4f07-9acf-adbf71119886");

            XNamespace xsiNs = XmlSchema.InstanceNamespace;
            XNamespace xsdNs = XmlSchema.Namespace;

            this.document = new XDocument(new XDeclaration("1.0", "utf-8", "no"));
            this.messageElement = new XElement(XmlNamespaces.GetMessageElementName(methodName));
            XElement messageEl = new XElement(SoapNs + "Envelope", 
                                     new XAttribute(XNamespace.Xmlns + "xsi", xsiNs),
                                     new XAttribute(XNamespace.Xmlns + "xsd", xsdNs),
                                     new XAttribute(XNamespace.Xmlns + "soap", SoapNs),
                                     new XElement(SoapNs + "Body", this.messageElement));

            this.document.Add(messageEl);
        }

        public void AddParam(string name, params object[] content)
        {
            this.messageElement.Add(new XElement(XmlNamespaces.GetMessageElementName(name), content));
        }

        public void AddParam(XElement param)
        {
            this.messageElement.Add(param);
        }

        public void Save()
        {
            document.Save(this.request.GetRequestStream());
        }
    }
}
