//
// Microsoft.TeamFoundation.VersionControl.Client.Message
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
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Web.Services;
using System.Web.Services.Description;
using System.Web.Services.Discovery;
using System.Web.Services.Protocols;
using Microsoft.TeamFoundation.VersionControl.Common;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
	internal class Message 
	{
		private const string SoapEnvelopeNamespace = "http://schemas.xmlsoap.org/soap/envelope/";
		private const string MessageNamespace =	"http://schemas.microsoft.com/TeamFoundation/2005/06/VersionControl/ClientServices/03";
		private WebRequest request;
		private XmlTextWriter xtw;
		private string methodName;

		public XmlTextWriter Body
		{
			get { return xtw; }
		}	 

		public WebRequest Request
		{
			get { return request; }
		}	 

		public string MethodName 
		{
			get { return methodName; }
		}

		public XmlReader ResponseReader(HttpWebResponse response)
		{
			//Console.WriteLine(new StreamReader(response.GetResponseStream()).ReadToEnd());
			StreamReader sr = new StreamReader(response.GetResponseStream(), new UTF8Encoding (false), false);
			XmlReader reader = new XmlTextReader(sr);

			string resultName = MethodName + "Result";
			while (reader.Read())
				{
					if (reader.NodeType == XmlNodeType.Element && reader.Name == resultName)
						break;
				}

			return reader;
		}

		public Message(WebRequest request, string methodName)
		{
			this.methodName = methodName;
			this.request = request;
			this.request.Method = "POST";

			// PreAuth can't be used with NTLM afaict
			//this.request.PreAuthenticate=true;

			// must allow buffering for NTLM authentication
			HttpWebRequest req = request as HttpWebRequest;
			req.AllowWriteStreamBuffering = true;

			//http://blogs.x2line.com/al/archive/2005/01/04/759.aspx#780
			//req.KeepAlive = false;
			//req.ProtocolVersion = HttpVersion.Version10;

			this.request.ContentType = "text/xml; charset=utf-8";
			this.request.Headers.Add ("X-TFS-Version", "1.0.0.0");
			this.request.Headers.Add ("accept-language", "en-US");
			this.request.Headers.Add ("X-VersionControl-Instance", "ac4d8821-8927-4f07-9acf-adbf71119886");

			xtw = new XmlTextWriter (request.GetRequestStream(), new UTF8Encoding (false));
			//xtw.Formatting = Formatting.Indented;
			
			xtw.WriteStartDocument();
			xtw.WriteStartElement("soap", "Envelope", SoapEnvelopeNamespace);
			xtw.WriteAttributeString("xmlns", "xsi", null, XmlSchema.InstanceNamespace);
			xtw.WriteAttributeString("xmlns", "xsd", null, XmlSchema.Namespace);

			xtw.WriteStartElement("soap", "Body", SoapEnvelopeNamespace);
			xtw.WriteStartElement("", methodName, MessageNamespace);
		}

		public void End ()
		{
			xtw.WriteEndElement(); // methodName
			xtw.WriteEndElement(); // soap:body
			xtw.WriteEndElement(); // soap:Envelope
			xtw.Flush();
			xtw.Close();
		}
	}
}
