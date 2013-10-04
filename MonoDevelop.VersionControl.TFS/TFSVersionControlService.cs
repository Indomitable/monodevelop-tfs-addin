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
using MonoDevelop.VersionControl.TFS.Infrastructure.Objects;
using System;
using GLib;

namespace MonoDevelop.VersionControl.TFS
{
    public class TFSVersionControlService
    {
        private readonly List<ServerEntry> _registredServers = new List<ServerEntry>();
        private readonly Dictionary<ServerEntry, string> _activeWorkspaces = new Dictionary<ServerEntry, string>();
        private static TFSVersionControlService _instance;

        public TFSVersionControlService()
        {
            LoadPrefs();
        }

        public static TFSVersionControlService Instance { get { return _instance ?? (_instance = new TFSVersionControlService()); } }

        private string ConfigFile { get { return UserProfile.Current.ConfigDir.Combine("VersionControl.TFS.config"); } }

        public void StorePrefs()
        {
            using (var file = File.Create(ConfigFile))
            {
                XDocument doc = new XDocument();
                doc.Add(new XElement("TFSRoot"));
                doc.Root.Add(new XElement("TFSServers", from server in _registredServers
                                                                    select new XElement("TFSServer", 
                                                                            new XAttribute("Name", server.Name),
                                                                            new XAttribute("Url", server.Url),
                                                                            new XAttribute("Workspace", _activeWorkspaces[server]))));
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
                    foreach (var serverElement in doc.Root.Element("TFSServers").Elements("TFSServer"))
                    {
                        var serverEntry = new ServerEntry
                        {
                            Name = serverElement.Attribute("Name").Value,
                            Url = new Uri(serverElement.Attribute("Url").Value)
                        };
                        _registredServers.Add(serverEntry);
                        _activeWorkspaces.Add(serverEntry, serverElement.Attribute("Workspace") == null ? string.Empty : serverElement.Attribute("Workspace").Value);
                    }
                    file.Close();
                }
            }
            catch
            {
                return;
            }
            if (_registredServers.Any())
            {
                RaiseServersChange();
            }
        }

        public void AddServer(string name, Uri url)
        {
            if (HasServer(name))
                return;
            _registredServers.Add(new ServerEntry { Name = name, Url = url });
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

        public ServerEntry GetServer(string name)
        {
            return _registredServers.SingleOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public bool HasServer(string name)
        {
            return _registredServers.Any(x => string.Equals(x.Name, name, System.StringComparison.OrdinalIgnoreCase));
        }

        public List<ServerEntry> Servers { get { return _registredServers; } }

        public event Action OnServersChange;

        private void RaiseServersChange()
        {
            if (OnServersChange != null)
            {
                OnServersChange();
            }
        }

        public string GetActiveWorkspace(ServerEntry server)
        {
            return _activeWorkspaces[server];
        }

        public void SetActiveWorkspace(ServerEntry server, string workspaceName)
        {
            _activeWorkspaces[server] = workspaceName;
            StorePrefs();
        }
    }
}