//
// Microsoft.TeamFoundation.WorkItemTracking.Client.ClientService
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
using System.Data;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Web.Services;
using System.Web.Services.Description;
using System.Web.Services.Discovery;
using System.Web.Services.Protocols;
using Microsoft.TeamFoundation;

namespace Microsoft.TeamFoundation.WorkItemTracking.Client
{
	[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://schemas.microsoft.com/TeamFoundation/2005/06/WorkItemTracking/ClientServices/03")]
	[System.Xml.Serialization.XmlRootAttribute(Namespace="http://schemas.microsoft.com/TeamFoundation/2005/06/WorkItemTracking/ClientServices/03", IsNullable=true)]
	public class RequestHeader : System.Web.Services.Protocols.SoapHeader {
		/// <remarks/>
		public string Id = "uuid:15b75849-cb6b-42c0-af40-c6734135130a";
		
		/// <remarks/>
		[System.Xml.Serialization.XmlAnyAttribute()]
			public System.Xml.XmlAttribute[] AnyAttribute;
	}

	[System.Web.Services.WebServiceBinding(Name="ClientServiceSoap", Namespace="http://schemas.microsoft.com/TeamFoundation/2005/06/WorkItemTracking/ClientServices/03")]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	//	[System.Xml.Serialization.XmlIncludeAttribute(typeof(SecurityChange))]
	internal class ClientService : System.Web.Services.Protocols.SoapHttpClientProtocol {
		
		public RequestHeader RequestHeaderValue = new RequestHeader();

		public ClientService(Uri url, ICredentials credentials) 
			{
				this.Url = String.Format("{0}/{1}", url, "WorkItemTracking/v1.0/ClientService.asmx");
				this.Credentials = credentials;
			}

		public string GetExceptionMessage(HttpWebResponse response)
		{
			StreamReader sr = new StreamReader(response.GetResponseStream(), new UTF8Encoding (false), false);
			XmlReader reader = new XmlTextReader(sr);
			string msg = String.Empty;
			while (reader.Read())
				{
					if (reader.NodeType == XmlNodeType.Element && reader.Name == "faultstring")
						{
							msg = reader.ReadElementContentAsString();
							break;
						}
				}

			response.Close();
			return msg;
		}

		public System.Data.DataSet GetStoredQuery(string queryId) {
			object[] results = this.Invoke("GetStoredQuery", new object[] {
					queryId});
			return ((System.Data.DataSet)(results[0]));
		}
		
		[System.Web.Services.Protocols.SoapHeaderAttribute("RequestHeaderValue", Required=false)]
			[System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://schemas.microsoft.com/TeamFoundation/2005/06/WorkItemTracking/ClientServices/03/GetStoredQueries", RequestNamespace="http://schemas.microsoft.com/TeamFoundation/2005/06/WorkItemTracking/ClientServices/03", ResponseNamespace="http://schemas.microsoft.com/TeamFoundation/2005/06/WorkItemTracking/ClientServices/03", ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped, Use=System.Web.Services.Description.SoapBindingUse.Literal)]
			[return: System.Xml.Serialization.XmlElementAttribute("queriesPayload")]
			public System.Data.DataSet GetStoredQueries(long rowVersion, int projectId) {
			object[] results = this.Invoke("GetStoredQueries", new object[] {
					rowVersion,
					projectId});
			return ((System.Data.DataSet)(results[0]));
		}

		[System.Web.Services.Protocols.SoapHeaderAttribute("RequestHeaderValue", Required=false)]
		[System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://schemas.microsoft.com/TeamFoundation/2005/06/WorkItemTracking/ClientServices/03/GetMetadataEx2", RequestNamespace="http://schemas.microsoft.com/TeamFoundation/2005/06/WorkItemTracking/ClientServices/03", ResponseNamespace="http://schemas.microsoft.com/TeamFoundation/2005/06/WorkItemTracking/ClientServices/03", ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped, Use=System.Web.Services.Description.SoapBindingUse.Literal)]
		[return: System.Xml.Serialization.XmlElementAttribute("metadata")]
		public System.Data.DataSet GetMetadataEx2([System.Xml.Serialization.XmlArrayItem(IsNullable=false)] MetadataTableHaveEntry[] metadataHave, bool useMaster, out string dbStamp, out int locale, out int comparisonStyle, out int mode) 
			{
				object[] results = this.Invoke("GetMetadataEx2", new object[] { metadataHave, useMaster});
				mode = ((int)(results[4]));
				comparisonStyle = ((int)(results[3]));
				locale = ((int)(results[2]));
				dbStamp = ((string)(results[1]));
				return ((System.Data.DataSet)(results[0]));
			}
	}
}
