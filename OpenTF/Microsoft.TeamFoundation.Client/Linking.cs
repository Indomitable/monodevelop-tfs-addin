//
// Microsoft.TeamFoundation.Client.Linking
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
using System.Net;
using System.Web.Services;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Client;

namespace Microsoft.TeamFoundation.Client
{
	[System.Web.Services.WebServiceBinding(Name="IntegrationServiceSoap", Namespace="http://schemas.microsoft.com/TeamFoundation/2005/06/Services/Linking/03")]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	public class Linking : System.Web.Services.Protocols.SoapHttpClientProtocol, ILinking
	{
		public Linking(TeamFoundationServer teamFoundationServer) 
			{
				this.Url = String.Format("{0}/{1}", teamFoundationServer.Uri, "WorkItemTracking/v1.0/Integration.asmx");
				this.Credentials = teamFoundationServer.Credentials;
			}
		
		[System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://schemas.microsoft.com/TeamFoundation/2005/06/Services/Linking/03/GetReferencingArtifacts", RequestNamespace="http://schemas.microsoft.com/TeamFoundation/2005/06/Services/Linking/03", ResponseNamespace="http://schemas.microsoft.com/TeamFoundation/2005/06/Services/Linking/03", ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped, Use=System.Web.Services.Description.SoapBindingUse.Literal)]
			public Artifact[] GetReferencingArtifacts(string[] uriList, LinkFilter[] filters) 
			{
				Console.WriteLine("GetReferencingArtifacts");
				if (filters != null) 
					GetReferencingArtifactsWithFilter(uriList, filters);

				object[] results = this.Invoke("GetReferencingArtifacts", new object[] {
						uriList});
				return ((Artifact[])(results[0]));
			}
		
		[System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://schemas.microsoft.com/TeamFoundation/2005/06/Services/Linking/03/GetReferencingArtifactsWithFilter", RequestNamespace="http://schemas.microsoft.com/TeamFoundation/2005/06/Services/Linking/03", ResponseNamespace="http://schemas.microsoft.com/TeamFoundation/2005/06/Services/Linking/03", ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped, Use=System.Web.Services.Description.SoapBindingUse.Literal)]
			public Artifact[] GetReferencingArtifactsWithFilter(string[] uriList, LinkFilter[] filters) 
			{
				object[] results = this.Invoke("GetReferencingArtifactsWithFilter", new object[] {
						uriList,
						filters});
				return ((Artifact[])(results[0]));
			}
		
		[System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://schemas.microsoft.com/TeamFoundation/2005/06/Services/Linking/03/GetArtifacts", RequestNamespace="http://schemas.microsoft.com/TeamFoundation/2005/06/Services/Linking/03", ResponseNamespace="http://schemas.microsoft.com/TeamFoundation/2005/06/Services/Linking/03", ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped, Use=System.Web.Services.Description.SoapBindingUse.Literal)]
			public Artifact[] GetArtifacts(string[] artifactUris) 
			{
				object[] results = this.Invoke("GetArtifacts", new object[] {
						artifactUris});
				return ((Artifact[])(results[0]));
			}
		
		[System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://schemas.microsoft.com/TeamFoundation/2005/06/Services/Notification/03/Notify", RequestNamespace="http://schemas.microsoft.com/TeamFoundation/2005/06/Services/Notification/03", ResponseNamespace="http://schemas.microsoft.com/TeamFoundation/2005/06/Services/Linking/03", ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped, Use=System.Web.Services.Description.SoapBindingUse.Literal)]
			public void Notify(string eventXml, string tfsIdentityXml) 
			{
				this.Invoke("Notify", new object[] {
						eventXml,
						tfsIdentityXml});
			}
	}
}

