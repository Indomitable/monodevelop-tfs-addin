//
// Microsoft.TeamFoundation.VersionControl.Client.Workstation
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
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Xml;
using System.IO;
using Microsoft.TeamFoundation.Client;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
	public sealed class Workstation
	{
		internal static readonly string ConfigFile = "VersionControl.config";
		internal static Workstation current = new Workstation();
		internal static object mutex;
		internal static WorkstationSettings settings = new WorkstationSettings();

		public static Workstation Current
		{
			get { return current; }
		}

		internal static WorkstationSettings Settings
		{
			get {	return settings; }
		}

		private Workstation()
		{
		}

		public WorkspaceInfo GetLocalWorkspaceInfo(string path)
		{
			WorkspaceInfo[] wInfos = GetAllLocalWorkspaceInfo();

			WorkspaceInfo returnedInfo = null;
			int maxPath = 0;

			foreach (WorkspaceInfo wInfo in wInfos)
				{
					foreach (string mPath in wInfo.MappedPaths)
						{
							char[] charsToTrim = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar};
							string trimmedPath = mPath.TrimEnd(charsToTrim);

							if (path.StartsWith(trimmedPath, TfsPath.PlatformComparison)
									&& trimmedPath.Length > maxPath)
								{
									returnedInfo = wInfo;
									maxPath = trimmedPath.Length;
								}
						}
				}
		
			return returnedInfo;
		}

		public WorkspaceInfo GetLocalWorkspaceInfo(VersionControlServer versionControl, 
																							 string workspaceName,
																							 string workspaceOwner)
		{
			InternalServerInfo[] servers = ReadCachedWorkspaceInfo();
			foreach (InternalServerInfo sInfo in servers)
				{
					if (sInfo.Uri != versionControl.Uri) continue;

					foreach (WorkspaceInfo info in sInfo.Workspaces)
						{
							if (info.Name == workspaceName && info.OwnerName == workspaceOwner)
								{
									return info;
								}
						}
				}

			return null;
		}

		public WorkspaceInfo[] GetAllLocalWorkspaceInfo()
		{
			InternalServerInfo[] servers = ReadCachedWorkspaceInfo();

			List<WorkspaceInfo> wInfos = new List<WorkspaceInfo>();
			foreach (InternalServerInfo sInfo in servers)
				{
					foreach (WorkspaceInfo wInfo in sInfo.Workspaces)
						{
							wInfos.Add(wInfo);
						}
				}

			return wInfos.ToArray();
		}

		internal void AddCachedWorkspaceInfo(Guid serverGuid,
																				 Uri serverUri, Workspace workspace)
		{
			InternalServerInfo[] serverInfos = ReadCachedWorkspaceInfo();
			XmlElement servers = InitWorkspaceInfoCache();

			bool added = false;
			foreach (InternalServerInfo sInfo in serverInfos)
				{
					if (sInfo.Uri == serverUri)
						{
							List<WorkspaceInfo> workspaces = new List<WorkspaceInfo>();
							foreach (WorkspaceInfo info in sInfo.Workspaces)
								{
									workspaces.Add(info);
								}

							added = true;
							workspaces.Add(new WorkspaceInfo(sInfo, workspace));
							sInfo.Workspaces = workspaces.ToArray();
						}

					if (sInfo.Workspaces.Length == 0) continue;

					XmlElement serverInfoElement = sInfo.ToXml(servers.OwnerDocument);
					servers.AppendChild(serverInfoElement);
				}

			if (!added)
				{
					InternalServerInfo sInfo = new InternalServerInfo(serverUri.ToString(), serverGuid, workspace);
					XmlElement serverInfoElement = sInfo.ToXml(servers.OwnerDocument);
					servers.AppendChild(serverInfoElement);
				}

			SaveWorkspaceInfoCache(servers.OwnerDocument);
		}

		public void RemoveCachedWorkspaceInfo(Uri serverUri, string workspaceName)
		{
			InternalServerInfo[] serverInfos = ReadCachedWorkspaceInfo();
			XmlElement servers = InitWorkspaceInfoCache();

			foreach (InternalServerInfo sInfo in serverInfos)
				{
					if (sInfo.Uri == serverUri)
						{
							List<WorkspaceInfo> workspaces = new List<WorkspaceInfo>();
							foreach (WorkspaceInfo info in sInfo.Workspaces)
								{
									if (info.Name != workspaceName)
										workspaces.Add(info);
								}

							sInfo.Workspaces = workspaces.ToArray();
						}

					if (sInfo.Workspaces.Length == 0) continue;

					XmlElement serverInfoElement = sInfo.ToXml(servers.OwnerDocument);
					servers.AppendChild(serverInfoElement);
				}

			SaveWorkspaceInfoCache(servers.OwnerDocument);
		}

		public void UpdateWorkspaceInfoCache(VersionControlServer versionControl,
																				 string ownerName)
		{
			InternalServerInfo[] serverInfos = ReadCachedWorkspaceInfo();
			XmlElement servers = InitWorkspaceInfoCache();

			Workspace[] workspaces = versionControl.QueryWorkspaces(null, ownerName, Name);
			InternalServerInfo newServerInfo = new InternalServerInfo(versionControl.Uri.ToString(), versionControl.ServerGuid, workspaces);

			bool found = false;
			foreach (InternalServerInfo sInfo in serverInfos)
				{
					InternalServerInfo finalInfo = sInfo;
					if (sInfo.Uri == versionControl.Uri)
						{
							finalInfo = newServerInfo;
							found = true;
						}

					XmlElement serverInfoElement = finalInfo.ToXml(servers.OwnerDocument);
					servers.AppendChild(serverInfoElement);
				}

			if (!found)
				{
					XmlElement serverInfoElement = newServerInfo.ToXml(servers.OwnerDocument);
					servers.AppendChild(serverInfoElement);
				}

			SaveWorkspaceInfoCache(servers.OwnerDocument);
		}

		internal void SaveWorkspaceInfoCache(XmlDocument doc)
		{
			string dataDirectory = TeamFoundationServer.ClientCacheDirectory;
			if (!Directory.Exists(dataDirectory)) Directory.CreateDirectory(dataDirectory);

			string cacheFilename = Path.Combine(dataDirectory, ConfigFile);

			using (XmlTextWriter writer = new XmlTextWriter(cacheFilename, null))
				{
					writer.Formatting = Formatting.Indented;
					doc.Save(writer);			
				}
		}

		internal InternalServerInfo[] ReadCachedWorkspaceInfo()
		{
			string dataDirectory = TeamFoundationServer.ClientCacheDirectory;
			string configFilePath = Path.Combine(dataDirectory, ConfigFile);

			List<InternalServerInfo> servers = new List<InternalServerInfo>();

			if (!File.Exists(configFilePath)) return servers.ToArray();

			using (XmlTextReader reader = new XmlTextReader(configFilePath))
				{
					while (reader.Read())
						{
							if (reader.NodeType == XmlNodeType.Element && reader.Name == "ServerInfo")
								servers.Add(InternalServerInfo.FromXml(reader));
						}
				}

			return servers.ToArray();
		}

		internal XmlElement InitWorkspaceInfoCache()
		{
			XmlDocument doc = new XmlDocument();

			XmlElement vcs = doc.CreateElement("VersionControlServer");
			doc.AppendChild(vcs);

			XmlElement servers = doc.CreateElement("Servers");
			vcs.AppendChild(servers);

			return servers;
		}

		public string Name
		{
			get { return Environment.MachineName; }
		}
	}
}		
