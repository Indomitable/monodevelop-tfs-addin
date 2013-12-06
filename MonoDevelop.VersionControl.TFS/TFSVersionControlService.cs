//
// TFSVersionControlService.cs
//
// Author:
//       Ventsislav Mladenov <vmladenov.mladenov@gmail.com>
//
// Copyright (c) 2013 Ventsislav Mladenov
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
using System.Collections.Generic;
using MonoDevelop.Core;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System;
using Microsoft.TeamFoundation.Client;
using MonoDevelop.VersionControl.TFS.Helpers;
using MonoDevelop.Ide;
using MonoDevelop.VersionControl.TFS.Infrastructure;
using Microsoft.TeamFoundation.VersionControl.Client.Enums;

namespace MonoDevelop.VersionControl.TFS
{
    public class TFSVersionControlService
    {
        private readonly List<TeamFoundationServer> _registredServers = new List<TeamFoundationServer>();
        private readonly Dictionary<string, string> _activeWorkspaces = new Dictionary<string, string>();
        private static TFSVersionControlService instance;

        public TFSVersionControlService()
        {
            LoadPrefs();
        }

        public static TFSVersionControlService Instance { get { return instance ?? (instance = new TFSVersionControlService()); } }

        private string ConfigFile { get { return UserProfile.Current.ConfigDir.Combine("VersionControl.TFS.config"); } }
        //<TFSRoot>
        //    <Servers>
        //      <Server Name="ServerName" Url="ServerUrl" Workspace="activeWorkspaceName">
        //         <ProjectCollection id="GUID" url="URL">
        //              <Project name="projectName" />
        //          </ProjectCollection>
        //      </Server>
        //    </Servers>
        //</TFSRoot>
        public void StorePrefs()
        {
            using (var file = File.Create(ConfigFile))
            {
                XDocument doc = new XDocument();
                doc.Add(new XElement("TFSRoot"));
                doc.Root.Add(new XElement("Servers", _registredServers.Select(x => x.ToLocalXml())));
                doc.Root.Add(new XElement("Workspaces", _activeWorkspaces.Select(a => new XElement("Workspace", new XAttribute("Id", a.Key), new XAttribute("Name", a.Value)))));
                if (this.MergeToolInfo != null)
                    doc.Root.Add(new XElement("MergeTool", new XAttribute("Command", this.MergeToolInfo.CommandName), new XAttribute("Arguments", this.MergeToolInfo.Arguments)));
                doc.Root.Add(new XElement("CheckOutLockLevel", (int)CheckOutLockLevel));
                doc.Save(file);
                file.Close();
            }
        }

        public void LoadPrefs()
        {
            _registredServers.Clear();
            if (!File.Exists(ConfigFile))
                return;
            try
            {
                using (var file = File.OpenRead(ConfigFile))
                {
                    XDocument doc = XDocument.Load(file);
                    foreach (var serverElement in doc.Root.Element("Servers").Elements("Server"))
                    {
                        var isPasswordSavedInXml = serverElement.Attribute("Password") != null;
                        var password = isPasswordSavedInXml ? serverElement.Attribute("Password").Value : CredentialsManager.GetPassword(new Uri(serverElement.Attribute("Url").Value));
                        if (password == null)
                            throw new Exception("TFS Addin: No Password found for TFS server: " + serverElement.Attribute("Name").Value);
                        var server = TeamFoundationServer.FromLocalXml(serverElement, password, isPasswordSavedInXml);
                        if (server != null)
                            _registredServers.Add(server);
                    }
                    foreach (var workspace in doc.Root.Element("Workspaces").Elements("Workspace"))
                    {
                        _activeWorkspaces.Add(workspace.Attribute("Id").Value, workspace.Attribute("Name").Value);
                    }
                    var mergeToolElement = doc.Root.Element("MergeTool");
                    if (mergeToolElement != null)
                    {
                        this.MergeToolInfo = new MergeToolInfo
                        {
                            CommandName = mergeToolElement.Attribute("Command").Value,
                            Arguments = mergeToolElement.Attribute("Arguments").Value,
                        };
                    }
                    if (doc.Root.Element("CheckOutLockLevel") == null)
                    {
                        checkOutLockLevel = CheckOutLockLevel.Unchanged;
                    }
                    else
                    {
                        checkOutLockLevel = (CheckOutLockLevel)Convert.ToInt32(doc.Root.Element("CheckOutLockLevel").Value);
                    }
                    file.Close();
                }
            }
            catch (Exception e)
            {
                MessageService.ShowException(e);
                return;
            }
            if (_registredServers.Any())
            {
                RaiseServersChange();
            }
        }

        public void AddServer(TeamFoundationServer server)
        {
            if (HasServer(server.Name))
                RemoveServer(server.Name);
            _registredServers.Add(server);
            RaiseServersChange();
            StorePrefs();
        }

        public void RemoveServer(string name)
        {
            if (!HasServer(name))
                return;
            _registredServers.RemoveAll(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            RaiseServersChange();
            StorePrefs();
        }

        public TeamFoundationServer GetServer(string name)
        {
            return _registredServers.SingleOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public bool HasServer(string name)
        {
            return _registredServers.Any(x => string.Equals(x.Name, name, System.StringComparison.OrdinalIgnoreCase));
        }

        public List<TeamFoundationServer> Servers { get { return _registredServers; } }

        public event Action OnServersChange;

        public void RaiseServersChange()
        {
            if (OnServersChange != null)
            {
                OnServersChange();
            }
        }

        public string GetActiveWorkspace(ProjectCollection collection)
        {
            if (!_activeWorkspaces.ContainsKey(collection.Id))
                return string.Empty;
            return _activeWorkspaces[collection.Id];
        }

        public void SetActiveWorkspace(ProjectCollection collection, string workspaceName)
        {
            _activeWorkspaces[collection.Id] = workspaceName;
            StorePrefs();
        }

        public MergeToolInfo MergeToolInfo { get; set; }

        private CheckOutLockLevel checkOutLockLevel;

        public CheckOutLockLevel CheckOutLockLevel
        {
            get { return checkOutLockLevel; }
            set
            {
                checkOutLockLevel = value;
                StorePrefs();
            }
        }

        public void RefreshWorkingRepositories()
        {
            foreach(var system in VersionControlService.GetVersionControlSystems())
            {
                var tfsSystem = system as TFSClient;
                if (tfsSystem != null)
                {
                    tfsSystem.RefreshRepositories();
                }
            }
        }
    }
}