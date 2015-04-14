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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Client.Enums;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.VersionControl.TFS.Core.Structure;
using MonoDevelop.VersionControl.TFS.Infrastructure;

namespace MonoDevelop.VersionControl.TFS
{
    sealed class TFSVersionControlService
    {
        private readonly List<TeamFoundationServer> _registredServers = new List<TeamFoundationServer>();
        /// <summary>
        /// Store default workspaces for each saved project collection. The Key - Project Collection Id, The Value - Workspace Name.
        /// </summary>
        private readonly Dictionary<Guid, string> _defaultWorkspaces = new Dictionary<Guid, string>();
        private static TFSVersionControlService instance;

        public TFSVersionControlService()
        {
            LoadPrefs();
        }

        public static TFSVersionControlService Instance { get { return instance ?? (instance = new TFSVersionControlService()); } }

        private string ConfigFile { get { return UserProfile.Current.ConfigDir.Combine("VersionControl.TFS.config"); } }

        public void StorePrefs()
        {
            using (var file = File.Create(ConfigFile))
            {
                XDocument doc = new XDocument();
                doc.Add(new XElement("TFSRoot"));
                doc.Root.Add(new XElement("Servers", _registredServers.Select(x => x.ToConfigXml())));
                doc.Root.Add(new XElement("Workspaces", _defaultWorkspaces.Select(a => new XElement("Workspace", new XAttribute("Id", a.Key), new XAttribute("Name", a.Value)))));
                if (this.MergeToolInfo != null)
                    doc.Root.Add(new XElement("MergeTool", new XAttribute("Command", this.MergeToolInfo.CommandName), new XAttribute("Arguments", this.MergeToolInfo.Arguments)));
                doc.Root.Add(new XElement("CheckOutLockLevel", (int)CheckOutLockLevel));
                doc.Root.Add(new XElement("DebugMode", IsDebugMode));
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
                var xmlConfig = File.ReadAllText(ConfigFile);
                XDocument doc = XDocument.Parse(xmlConfig);
                foreach (var serverElement in doc.Root.Element("Servers").Elements("Server"))
                {
                    var server = TeamFoundationServer.FromConfigXml(serverElement);
                    _registredServers.Add(server);
                }
                foreach (var workspace in doc.Root.Element("Workspaces").Elements("Workspace"))
                {
                    _defaultWorkspaces.Add(Guid.Parse(workspace.Attribute("Id").Value), workspace.Attribute("Name").Value);
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
                checkOutLockLevel = doc.Root.Element("CheckOutLockLevel") == null ? LockLevel.Unchanged : (LockLevel)Convert.ToInt32(doc.Root.Element("CheckOutLockLevel").Value);
                this.isDebugMode = doc.Root.Element("DebugMode") != null && Convert.ToBoolean(doc.Root.Element("DebugMode").Value);
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
            return _registredServers.Any(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
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
            if (!_defaultWorkspaces.ContainsKey(collection.Id))
                return string.Empty;
            return _defaultWorkspaces[collection.Id];
        }

        public void SetActiveWorkspace(ProjectCollection collection, string workspaceName)
        {
            if (!_defaultWorkspaces.ContainsKey(collection.Id))
                _defaultWorkspaces.Add(collection.Id, workspaceName);
            else
                _defaultWorkspaces[collection.Id] = workspaceName;

            StorePrefs();
        }

        public MergeToolInfo MergeToolInfo { get; set; }

        private LockLevel checkOutLockLevel;

        public LockLevel CheckOutLockLevel
        {
            get { return checkOutLockLevel; }
            set
            {
                checkOutLockLevel = value;
                StorePrefs();
            }
        }

        private bool isDebugMode;

        public bool IsDebugMode
        { 
            get
            {
                return isDebugMode;
            }
            set
            {
                isDebugMode = value;
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