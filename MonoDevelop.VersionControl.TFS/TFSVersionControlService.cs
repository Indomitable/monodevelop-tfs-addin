using System.Collections.Generic;
using MonoDevelop.Core;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace MonoDevelop.VersionControl.TFS
{
    public class TFSVersionControlService
    {
        private readonly Dictionary<string, string> _registredServers = new Dictionary<string, string>();
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
                                                                            new XAttribute("Name", server.Key),
                                                                            new XAttribute("Url", server.Value))));
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
                        _registredServers.Add(serverElement.Attribute("Name").Value, serverElement.Attribute("Url").Value);
                    }
                    file.Close();
                }
            }
            catch
            {
                return;
            }
        }

        public void AddServer(string name, string url)
        {
            if (_registredServers.ContainsKey(name))
                return;
            _registredServers.Add(name, url);
            StorePrefs();
        }

        public void RemoveServer(string name)
        {
            if (!_registredServers.ContainsKey(name))
                return;
            _registredServers.Remove(name);
            StorePrefs();
        }

        public bool HasServer(string name)
        {
            return _registredServers.ContainsKey(name);
        }

        public Dictionary<string, string> Servers
        {
            get
            {
                return _registredServers;
            }
        }
    }
}

