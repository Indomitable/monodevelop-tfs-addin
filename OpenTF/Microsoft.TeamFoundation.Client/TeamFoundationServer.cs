//
// Microsoft.TeamFoundation.Client.TeamFoundationServer
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
using System.Net;
using Microsoft.TeamFoundation.Server;

namespace Microsoft.TeamFoundation.Client
{
	public sealed class TeamFoundationServer : IServiceProvider, IDisposable
	{
		private string name;
		private Uri uri;
		private ICredentials credentials;
		private bool hasAuthenticated = false;

		public TeamFoundationServer(string url)
		{
			this.uri = new Uri(url);
			this.name = url;
		}

		public TeamFoundationServer(string url, ICredentials creds)
		{
			this.uri = new Uri(url);
			this.name = this.uri.Host;

			credentials = creds;
		}

		public TeamFoundationServer(string url, ICredentialsProvider creds)
		{
			this.uri = new Uri(url);
			this.name = this.uri.Host;

			credentials = creds.GetCredentials(Uri, null);
		}

		public object GetService(Type serviceType)
		{
			EnsureAuthenticated();

			if (serviceType == typeof(ICommonStructureService))
				return new CommonStructureService(this);
			else if (serviceType == typeof(ILinking))
				return new Linking(this);
			else if (serviceType == typeof(IRegistration))
				return new Registration(this);
			else if (serviceType == typeof(IGroupSecurityService))
				return new GroupSecurityService(this);
			else
				return Activator.CreateInstance(serviceType, new object[]{ this });
		}

		public void EnsureAuthenticated()
		{
			if (!hasAuthenticated) Authenticate();
		}

		public void Authenticate()
		{
			Authenticator authenticator = new Authenticator(uri, credentials);
			authenticator.CheckAuthentication();
			hasAuthenticated = true;
		}
		
		public void Dispose()
		{
		}

		public ICredentials Credentials {
			get { return credentials; }
		}

		public string Name {
			get { return name; }
		}
		
		public Uri Uri {
			get { return uri; }
		}

		public bool HasAuthenticated
		{
			get { return hasAuthenticated; }
		}

		public static string ClientCacheDirectory {
			get {
				string personal = Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
				string tf = Path.Combine(personal, ".tf");
				return Path.Combine(tf, "Cache");
			}
		}

		public static string ClientSettingsDirectory {
			get {
				string personal = Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
				return Path.Combine(personal, ".tf");
			}
		}

	}
}

