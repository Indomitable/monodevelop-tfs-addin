// NtlmAuthorization.cs
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
using System.Xml.Linq;
using MonoDevelop.VersionControl.TFS.Core.ServerAuthorization.Config;
using MonoDevelop.VersionControl.TFS.Helpers;

namespace MonoDevelop.VersionControl.TFS.Core.ServerAuthorization
{
    sealed class NtlmAuthorization : UserPasswordAuthorization, IServerAuthorization
    {
        private NtlmAuthorization()
        {
            
        }

        public void Authorize(HttpWebRequest request)
        {
            request.Credentials = new NetworkCredential(UserName, Password, Domain);
        }

        public void Authorize(WebClient client)
        {
            client.Credentials = new NetworkCredential(UserName, Password, Domain);
        }

        public void Authorize(HttpClientHandler clientHandler, HttpRequestMessage message)
        {
            clientHandler.Credentials = new NetworkCredential(UserName, Password, Domain);
        }


        public string Domain { get; private set; }

        public static NtlmAuthorization FromConfigXml(XElement element, Uri serverUri)
        {
            var auth = new NtlmAuthorization();
            auth.ReadConfig(element, serverUri);
            auth.Domain = element.GetAttributeValue("Domain");
            return auth;
        }

        public XElement ToConfigXml()
        {
            var element = new XElement("Ntlm", 
                            new XAttribute("UserName", UserName),
                            new XAttribute("Domain", Domain));
            if (ClearSavePassword)
            {
                element.Add(new XAttribute("Password", Password));
            }
            return element;
        }

        public static NtlmAuthorization FromConfigWidget(INtlmAuthorizationConfig config)
        {
            var authorization = new NtlmAuthorization();
            authorization.ReadConfig(config);
            authorization.Domain = config.Domain;
            return authorization;
        }
    }
}