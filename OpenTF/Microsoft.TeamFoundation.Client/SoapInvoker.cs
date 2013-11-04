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

namespace Microsoft.TeamFoundation.Client
{
	public class SoapInvoker
	{
		readonly XNamespace xsiNs = XmlSchema.InstanceNamespace;
		readonly XNamespace xsdNs = XmlSchema.Namespace;
		readonly XNamespace soapNs = "http://schemas.xmlsoap.org/soap/envelope/";
		readonly Uri url;
		readonly ICredentials credentials;
		XNamespace messagegNs;
		XDocument document;
		string methodName;

		public SoapInvoker(string serverUrl, string serviceUrl, ICredentials credentials)
		{
			const string urlSeparator = "/";
			if (!serverUrl.EndsWith(urlSeparator, StringComparison.Ordinal))
				serverUrl = serverUrl + urlSeparator;
			if (serviceUrl.StartsWith(urlSeparator, StringComparison.Ordinal))
				serviceUrl = serviceUrl.Substring(urlSeparator.Length); 
			this.url = new Uri(new Uri(serverUrl), serviceUrl);
			this.credentials = credentials;
		}

		public SoapInvoker(Uri fullUrl, ICredentials credentials)
		{
			this.url = fullUrl;
			this.credentials = credentials;
		}

		public SoapInvoker(TfsService service)
		{
			this.url = service.Url;
			this.credentials = service.Collection.Server.Credentials;
			this.messagegNs = service.MessageNs;
		}

		public XElement CreateEnvelope(string methodName)
		{
			this.methodName = methodName;
			document = new XDocument(new XDeclaration("1.0", "utf-8", "no"));
			var innerMessage = new XElement(messagegNs + methodName);
			document.Add(new XElement(soapNs + "Envelope", 
				new XAttribute(XNamespace.Xmlns + "xsi", xsiNs),
				new XAttribute(XNamespace.Xmlns + "xsd", xsdNs),
				new XAttribute(XNamespace.Xmlns + "soap", soapNs),
				new XElement(soapNs + "Body", innerMessage)));
			return innerMessage;
		}

		public XElement CreateEnvelope(string methodName, XNamespace messageNamespace)
		{
			this.messagegNs = messageNamespace;
			return CreateEnvelope(methodName);
		}

		public XElement Invoke()
		{
			var request = (HttpWebRequest)WebRequest.Create(url); 
			request.Credentials = credentials;
			request.AllowWriteStreamBuffering = true;
			request.Method = "POST";
			request.ContentType = "text/xml; charset=utf-8";
			this.document.Save(request.GetRequestStream());
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
					return resultDocument.Root.Element(soapNs + "Body")
                                                  .Element(this.messagegNs + (this.methodName + "Response"))
                                                  .Element(this.messagegNs + (this.methodName + "Result"));
				}
			}

		}

		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine("User Name:" + ((NetworkCredential)credentials).UserName);
			builder.AppendLine("Domain:" + ((NetworkCredential)credentials).Domain);
			builder.AppendLine("Password:" + ((NetworkCredential)credentials).Password);
			builder.Append(document.ToString());
			return builder.ToString();
		}
	}
}
