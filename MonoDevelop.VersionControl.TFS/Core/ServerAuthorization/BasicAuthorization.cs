// BasicAuthorization.cs
// 
// Author:
//       Ventsislav Mladenov
// 
// The MIT License (MIT)
// 
// Copyright (c) 2013-2015 Ventsislav Mladenov
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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;
using MonoDevelop.VersionControl.TFS.Core.ServerAuthorization.Config;

namespace MonoDevelop.VersionControl.TFS.Core.ServerAuthorization
{
    sealed class BasicAuthorization : UserPasswordAuthorization, IServerAuthorization
    {
        private BasicAuthorization()
        {
            
        }

        public void Authorize(HttpWebRequest request)
        {
            request.Headers.Add(HttpRequestHeader.Authorization, this.AuthorizationHeader);
        }

        public void Authorize(WebClient client)
        {
            client.Headers.Add(HttpRequestHeader.Authorization, this.AuthorizationHeader);
        }

        public void Authorize(HttpClientHandler clientHandler, HttpRequestMessage message)
        {
            message.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(this.UserName + ":" + this.Password)));
        }

        public static BasicAuthorization FromConfigXml(XElement element, Uri serverUri)
        {
            var auth = new BasicAuthorization();
            auth.ReadConfig(element, serverUri);
            return auth;
        }

        public XElement ToConfigXml()
        {
            var element = new XElement("Basic",
                            new XAttribute("UserName", UserName));
            if (ClearSavePassword)
            {
                element.Add(new XAttribute("Password", Password));
            }
            return element;
        }

        private string AuthorizationHeader
        {
            get
            {
                var credentialBuffer = Encoding.UTF8.GetBytes(this.UserName + ":" + this.Password);
                return "Basic " + Convert.ToBase64String(credentialBuffer);
            }
        }

        public static IServerAuthorization FromConfigWidget(IUserPasswordAuthorizationConfig serverAuthorizationConfig)
        {
            var auth = new BasicAuthorization();
            auth.ReadConfig(serverAuthorizationConfig);
            return auth;
        }
    }
}