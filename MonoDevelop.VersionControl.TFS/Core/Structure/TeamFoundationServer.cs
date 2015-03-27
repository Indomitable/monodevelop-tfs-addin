//
// Microsoft.TeamFoundation.Client.TeamFoundationServer
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
using MonoDevelop.VersionControl.TFS.Configuration;
using MonoDevelop.VersionControl.TFS.Core.ServerAuthentication;

namespace MonoDevelop.VersionControl.TFS.Core.Structure
{
    sealed class TeamFoundationServer : BaseTeamFoundationServer, INetworkCredentialsAuthorizatedServer
    {
        public TeamFoundationServer(ServerConfig config)
            : base(config)
        {
            Credentials = new NetworkCredential(config.AuthUserName, Password, Domain);
        }

        /// <summary>
        /// When using on-premise server the authuser is the user with access to tfs.
        /// </summary>
        /// <value>The name of the user.</value>
        public override string UserName
        {
            get
            {
                return Config.AuthUserName;
            }
        }

        string Domain { get { return Config.Domain; } }

        public NetworkCredential Credentials { get; private set; }
    }
}

