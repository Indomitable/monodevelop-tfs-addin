using System.Collections.Generic;
using MonoDevelop.Core;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using MonoDevelop.VersionControl.TFS.Infrastructure.Objects;
using System;

namespace MonoDevelop.VersionControl.TFS
{
    public class TFSVersionControlService
    {
        private readonly List<ServerEntry> _registredServers = new List<ServerEntry>();
        private static TFSVersionControlService _instance;

        public TFSVersionControlService()
        {
            LoadPrefs();
        }

        public static TFSVersionControlService Instance
        {
            get
            {
                return _instance ?? (_instance = new TFSVersionControlService());
            }
        }

        private string ConfigFile
        {
            get
            {
                return UserProfile.Current.ConfigDir.Combine("VersionControl.TFS.config");
            }
        }

        public void StorePrefs()
        {
            using (var file = File.Create(ConfigFile))
            {
                XDocument doc = new XDocument();
                doc.Add(new XElement("TFSRoot"));
                doc.Root.Add(new XElement("TFSServers", from server in _registredServers
                                                                    select new XElement("TFSServer", 
                                                                            new XAttribute("Name", server.Name),
                                                                            new XAttribute("Url", server.Url))));
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
                        _registredServers.Add(new ServerEntry
                        {
                            Name = serverElement.Attribute("Name").Value,
                            Url = new Uri(serverElement.Attribute("Url").Value)
                        });
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

        public List<ServerEntry> Servers
        {
            get
            {
                return _registredServers;
            }
        }

        public event Action OnServersChange;

        private void RaiseServersChange()
        {
            if (OnServersChange != null)
            {
                OnServersChange();
            }
        }
    }
}