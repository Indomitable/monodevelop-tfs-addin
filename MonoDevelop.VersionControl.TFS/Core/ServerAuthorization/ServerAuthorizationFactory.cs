// ServerAuthorizationFactory.cs
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
using MonoDevelop.VersionControl.TFS.GUI.Server.Authorization;

namespace MonoDevelop.VersionControl.TFS.Core.ServerAuthorization
{
    static class ServerAuthorizationFactory
    {
        public static IServerAuthorization GetServerAuthorization(XElement element, Uri serverUri)
        {
            ServerAuthorizationType authorizationType;
            Enum.TryParse(element.Name.LocalName, out authorizationType);
            switch (authorizationType)
            {
                case ServerAuthorizationType.Ntlm:
                    return NtlmAuthorization.FromConfigXml(element, serverUri);
                case ServerAuthorizationType.Basic:
                    return BasicAuthorization.FromConfigXml(element, serverUri);
                default:
                    return new NoAuthorization();
            }
        }

        public static IServerAuthorizationConfig GetServerAuthorizationConfig(ServerAuthorizationType authorizationType, Uri serverUri)
        {
            switch (authorizationType)
            {
                case ServerAuthorizationType.Ntlm:
                    return new NtlmAuthorizationConfig(serverUri);
                case ServerAuthorizationType.Basic:
                    return new UserPasswordAuthorizationConfig(serverUri);
                default:
                    return new NoAuthorizationConfig();
            }
        }

        public static IServerAuthorization GetServerAuthorization(ServerAuthorizationType authorizationType, IServerAuthorizationConfig serverAuthorizationConfig)
        {
            switch (authorizationType)
            {
                case ServerAuthorizationType.Ntlm:
                    return NtlmAuthorization.FromConfigWidget((INtlmAuthorizationConfig) serverAuthorizationConfig);
                case ServerAuthorizationType.Basic:
                    return BasicAuthorization.FromConfigWidget((IUserPasswordAuthorizationConfig)serverAuthorizationConfig);
                default:
                    return new NoAuthorization();
            }
        }
    }
}