//
// Microsoft.TeamFoundation.Server.Database
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

namespace Microsoft.TeamFoundation.Server
{
	public sealed class Database
	{
		private string connectionString;
		private string databaseName;
		private bool excludeFromBackup;
		private string sqlServerName;
		private string name;

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public string SQLServerName
		{
			get { return sqlServerName; }
			set { sqlServerName = value; }
		}

		public string ConnectionString
		{
			get { return connectionString; }
			set { connectionString = value; }
		}
	
		public string DatabaseName
		{
			get { return databaseName; }
			set { databaseName = value; }
		}

		public bool ExcludeFromBackup
		{
			get { return excludeFromBackup; }
			set { excludeFromBackup = value; }
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("Database instance ");
			sb.Append(GetHashCode());

			sb.Append("\n	 Name: ");
			sb.Append(Name);

			sb.Append("\n	 DatabaseName: ");
			sb.Append(DatabaseName);

			return sb.ToString();
		}
	}
}

