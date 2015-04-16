// UserPasswordAuthorization.cs
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
using System.Xml.Linq;
using MonoDevelop.VersionControl.TFS.Core.ServerAuthorization.Config;
using MonoDevelop.VersionControl.TFS.Helpers;

namespace MonoDevelop.VersionControl.TFS.Core.ServerAuthorization
{
    class UserPasswordAuthorization
    {
        protected UserPasswordAuthorization()
        {
            
        }

        public string UserName { get; protected set; }
        public string Password { get; private set; }
        public bool ClearSavePassword { get; private set; }

        protected void ReadConfig(XElement element, Uri serverUri)
        {
            this.UserName = element.GetAttributeValue("UserName");
            if (element.Attribute("Password") != null)
            {
                this.Password = element.Attribute("Password").Value;
                this.ClearSavePassword = true;
            }
            else
            {
                this.Password = CredentialsManager.GetPassword(serverUri);
                this.ClearSavePassword = false;
            }
        }

        protected void ReadConfig(IUserPasswordAuthorizationConfig config)
        {
            this.UserName = config.UserName;
            this.Password = config.Password;
            this.ClearSavePassword = config.ClearSavePassword;
        }
    }
}
