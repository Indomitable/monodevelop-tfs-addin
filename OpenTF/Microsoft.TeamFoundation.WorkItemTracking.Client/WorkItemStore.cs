//
// Microsoft.TeamFoundation.WorkItemTracking.Client.WorkItemStore
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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Xml;
using System.Web.Services;
using Microsoft.TeamFoundation.Client;

namespace Microsoft.TeamFoundation.WorkItemTracking.Client
{
	/// <remarks/>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://schemas.microsoft.com/TeamFoundation/2005/06/WorkItemTracking/ClientServices/03")]
	public class MetadataTableHaveEntry {

		public MetadataTableHaveEntry()
		{
		}

		public MetadataTableHaveEntry(string tableName, int rowVersion)
		{
			TableName = tableName; 
			RowVersion = rowVersion;
		}

		/// <remarks/>
		public string TableName;
		
		/// <remarks/>
		public long RowVersion;
	}

	public sealed class WorkItemStore
	{
		private ClientService clientService;
		private ProjectCollection project;
		private TeamFoundationServer teamFoundationServer;

		public WorkItemStore(TeamFoundationServer teamFoundationServer)
		{
			this.teamFoundationServer = teamFoundationServer;
			clientService = new ClientService(teamFoundationServer.Uri, teamFoundationServer.Credentials);
			List<MetadataTableHaveEntry> metadataHave = new List<MetadataTableHaveEntry>();
			metadataHave.Add(new MetadataTableHaveEntry("Hierarchy", 0));
			metadataHave.Add(new MetadataTableHaveEntry("Fields", 0));
			metadataHave.Add(new MetadataTableHaveEntry("HierarchyProperties", 0));
			metadataHave.Add(new MetadataTableHaveEntry("Constants", 0));
			metadataHave.Add(new MetadataTableHaveEntry("Rules", 0));
			metadataHave.Add(new MetadataTableHaveEntry("ConstantSets", 0));
			metadataHave.Add(new MetadataTableHaveEntry("FieldUsages", 0));
			metadataHave.Add(new MetadataTableHaveEntry("WorkItemTypes", 0));
			metadataHave.Add(new MetadataTableHaveEntry("Actions", 0));
			metadataHave.Add(new MetadataTableHaveEntry("WorkItemTypeUsages", 0));

			int mode = 0; int comparisonStyle = 0; int locale = 0;
			string dbStamp = String.Empty;
			DataSet dataSet = clientService.GetMetadataEx2(metadataHave.ToArray(), true, out dbStamp, out locale, out comparisonStyle, out mode); 

			foreach(DataTable table in dataSet.Tables)
				{
					Console.WriteLine(table.TableName);
					Console.WriteLine("========================================================");

					foreach(DataRow row in table.Rows)
						{
							foreach (DataColumn column in table.Columns)
								{
									Console.Write(column.ColumnName + "=" + row[column] + ", ");
								}
							Console.WriteLine();
						}

						Console.WriteLine();
				}
		}

		public WorkItem GetWorkItem (int id)
		{
			return null;
		}

		public ProjectCollection Projects
		{
			get { return project; }
		}

	}
}
