//
// Microsoft.TeamFoundation.Client.Authenticator
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
using System.Web.Services.Protocols;
using Microsoft.TeamFoundation.Server;

namespace Microsoft.TeamFoundation.Client
{
	[System.Web.Services.WebServiceBindingAttribute(Name="ServerStatusSoap", Namespace="http://schemas.microsoft.com/TeamFoundation/2005/06/Services/ServerStatus/03")]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	internal class Authenticator : System.Web.Services.Protocols.SoapHttpClientProtocol 
	{
		public Authenticator(Uri uri, ICredentials credentials) 
			{
				this.Url = String.Format("{0}/{1}", uri.ToString(), "Services/v1.0/ServerStatus.asmx");
				this.Credentials = credentials;
			}
		
		[System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://schemas.microsoft.com/TeamFoundation/2005/06/Services/ServerStatus/03/CheckAuthentication", RequestNamespace="http://schemas.microsoft.com/TeamFoundation/2005/06/Services/ServerStatus/03", ResponseNamespace="http://schemas.microsoft.com/TeamFoundation/2005/06/Services/ServerStatus/03", ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped, Use=System.Web.Services.Description.SoapBindingUse.Literal)]
			public string CheckAuthentication() 
			{
				try
				{
					object[] results = this.Invoke("CheckAuthentication", new object[0]);
					return ((string)(results[0]));
				}
				catch (SoapException ex)
				{
					//	string msg = String.Format("TF30063: You are not authorized to access {0} ({1}).\n--> Did you supply the correct username, password, and domain?", 
					//(new Uri(this.Url)).Host, "CheckAuthentication");
					Console.WriteLine(ex.ToString());
				}

				return String.Empty;
			}
	}
}
