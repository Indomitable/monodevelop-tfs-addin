//
// ProjectCollectionConfig.cs
//
// Author:
//       Ventsislav Mladenov <ventsislav.mladenov@gmail.com>
//
// Copyright (c) 2015 Ventsislav Mladenov License MIT/X11
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
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using MonoDevelop.VersionControl.TFS.Core;

namespace MonoDevelop.VersionControl.TFS.Configuration
{
    public class ServerConfig
    {
        public ServerConfig()
        {
            this.Name = string.Empty;
            this.Domain = string.Empty;
            this.UserName = string.Empty;
            this.AuthUserName = string.Empty;
            this.ProjectCollections = new List<ProjectCollectionConfig>();
        }

        public ServerConfig(ServerType type, string name, Uri url, string userName, string domain, string authUserName, string password)
        {
            this.Type = type;
            this.Name = name;
            this.Url = url;
            this.UserName = userName;
            this.Domain = domain;
            this.AuthUserName = authUserName;
            this.Password = password;
        }

        /// <summary>
        /// The server type.
        /// </summary>
        /// <value>The type.</value>
        public ServerType Type { get; private set; }
        /// <summary>
        /// Friendly name of the server
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; private set; }
        /// <summary>
        /// The url to server
        /// </summary>
        /// <value>The URL.</value>
        public Uri Url { get; private set; }
        /// <summary>
        /// The user name, for on-premise servers this is the user with access to TFS and to version control
        /// </summary>
        /// <value>The name of the user.</value>
        public string UserName { get; private set; }
        /// <summary>
        /// Domain for login - it is used only by on-premise servers
        /// </summary>
        /// <value>The domain.</value>
        public string Domain { get; private set; }
        /// <summary>
        /// The authorization user, for Visual Studio Online there are two users one for authorization and one for access to TFS
        /// </summary>
        /// <value>The name of the auth user.</value>
        public string AuthUserName { get; private set; }
        /// <summary>
        /// Password. If the password is null then it is storred in Secure storage (Windows/MacOs).
        /// </summary>
        /// <value>The password.</value>
        public string Password { get; private set; }
        /// <summary>
        /// If password is stored in config
        /// </summary>
        /// <value><c>true</c> if this instance has password; otherwise, <c>false</c>.</value>
        public bool HasPassword { get { return Password != null; } }

        public List<ProjectCollectionConfig> ProjectCollections { get; private set; }


        public XElement ToConfigXml()
        {
            var element = new XElement("Server", 
                                new XAttribute("Name", this.Name),                
                                new XAttribute("Type", (int)this.Type),                                
                                new XAttribute("Url", this.Url),
                                new XAttribute("Domain", this.Domain),
                                new XAttribute("UserName", this.UserName),
                                new XAttribute("AuthUserName", this.AuthUserName));
            if (HasPassword)
                element.Add(new XAttribute("Password", this.Password));

            element.Add(this.ProjectCollections.Select(pc => pc.ToConfigXml()));

            return element;
        }

        public static ServerConfig FromConfigXml(XElement element)
        {
            if (!string.Equals(element.Name.LocalName, "Server", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Invalid xml element");

            var server = new ServerConfig();
            server.Name = element.Attribute("Name").Value;
            server.Type = (ServerType)Convert.ToInt32(element.Attribute("Type").Value);
            server.Url = new Uri(element.Attribute("Url").Value);
            server.Domain = element.Attribute("Domain").Value;
            server.UserName = element.Attribute("UserName").Value;
            server.AuthUserName = element.Attribute("AuthUserName").Value;
            server.Password = element.Attribute("Password") != null ? element.Attribute("Password").Value : null;

            server.ProjectCollections.AddRange(element.Elements("ProjectCollection").Select(ProjectCollectionConfig.FromConfigXml));
            server.ProjectCollections.ForEach(pc => pc.Server = server);

            return server;
        }
    }
}

