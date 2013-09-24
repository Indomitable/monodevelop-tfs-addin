//
// Microsoft.TeamFoundation.Server.RegistrationEntry
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
using System.Text;
using System.Collections.Generic;

namespace Microsoft.TeamFoundation.Server
{
	public class RegistrationEntry
	{
		private List<ArtifactType> artifactTypes;
		private ChangeType changeType;
		private List<Database> databases;
		private List<ServiceInterface> serviceInterfaces;
		private string type;

		public ArtifactType[] ArtifactTypes
		{
			get { return artifactTypes.ToArray(); }
			set { artifactTypes = new List<ArtifactType>(value); }
		}

		public ChangeType ChangeType
		{
			get { return changeType; }
			set { changeType = value; }
		}

		public Database[] Databases
		{
			get { return databases.ToArray(); }
			set { databases = new List<Database>(value); }
		}

		public string Type
		{
			get { return type; }
			set { type = value; }
		}

		public ServiceInterface[] ServiceInterfaces
		{
			get { return serviceInterfaces.ToArray(); }
			set { serviceInterfaces = new List<ServiceInterface>(value); }
		}

		public RegistrationEntry ()
		{
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("RegistrationEntry instance ");
			sb.Append(GetHashCode());

			sb.Append("\n	 ArtifactTypes: ");
			foreach (ArtifactType artifactType in ArtifactTypes)
				{
					sb.Append(artifactType.ToString());
				}

			sb.Append("\n	 ChangeType: ");
			sb.Append(ChangeType);

			sb.Append("\n	 Databases: ");
			foreach (Database database in Databases)
				{
					sb.Append(database.ToString());
				}

			sb.Append("\n	 ServiceInterfaces: ");
			foreach (ServiceInterface serviceInterface in ServiceInterfaces)
				{
					sb.Append(serviceInterface.ToString());
				}

			sb.Append("\n	 Type: ");
			sb.Append(Type);

			return sb.ToString();
		}
	}
}

