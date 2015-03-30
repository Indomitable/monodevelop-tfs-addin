using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services.Protocols;
using System.Xml.Linq;
using MonoDevelop.VersionControl.TFS.Configuration;
using MonoDevelop.VersionControl.TFS.Core.ServerAuthorization;
using MonoDevelop.VersionControl.TFS.Helpers;
using MonoDevelop.VersionControl.TFS.Core.Services;

namespace MonoDevelop.VersionControl.TFS.Core.Structure
{
    sealed class BaseTeamFoundationServer: IEquatable<BaseTeamFoundationServer>, IComparable<BaseTeamFoundationServer>
    {
        BaseTeamFoundationServer()
        {
            ProjectCollections = new List<ProjectCollection>();
        }

        public ServerConfig FetchServerStructure()
        {
            var projectCollectionsService = new ProjectCollectionService(Uri) {Server = this};
            var projectCollectionConfigs = projectCollectionsService.GetProjectCollections();
            this.config.ProjectCollections.Clear();
            foreach (var projectCollectionConfig in projectCollectionConfigs)
            {
                projectCollectionConfig.Server = config;
                projectCollectionConfig.Projects.Clear();
                var projectCollection = new ProjectCollection(projectCollectionConfig, this);
                projectCollectionConfig.Projects.AddRange(projectCollection.FetchProjects());
            }
            this.config.ProjectCollections.AddRange(projectCollectionConfigs);
            return this.config;
        }

        public string Name { get; private set; }

        public Uri Uri { get; private set; }

        public IServerAuthorization Authorization { get; private set; }

        public List<ProjectCollection> ProjectCollections { get; set; }

        public XElement ToConfigXml()
        {
            var element = new XElement("Server",
                                new XAttribute("Name", this.Name),
                                new XAttribute("Uri", this.Uri));
            element.Add(new XElement("Auth", Authorization.ToConfigXml()));
            element.Add(new XElement("ProjectCollections", this.ProjectCollections.Select(pc => pc.ToConfigXml())));
            return element;
        }

        public static BaseTeamFoundationServer FromConfigXml(XElement element)
        {
            if (!string.Equals(element.Name.LocalName, "Server", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Invalid xml element");

            var server = new BaseTeamFoundationServer();
            server.Name = element.Attribute("Name").Value;
            server.Uri = new Uri(element.Attribute("Uri").Value);

            var authElement = (XElement)element.Element("Auth").FirstNode;
            server.Authorization = ServerAuthorizationFactory.GetServerAuthorization(authElement, server.Uri);

            server.ProjectCollections.AddRange(element.GetDescendants("ProjectCollection").Select(ProjectCollection.FromConfigXml));
            server.ProjectCollections.ForEach(pc => pc.Server = server);

            return server;
        }
        
        #region Equal

        #region IComparable<TeamFoundationServer> Members

        public int CompareTo(BaseTeamFoundationServer other)
        {
            return string.Compare(Uri.ToString(), other.Uri.ToString(), StringComparison.Ordinal);
        }

        #endregion

        #region IEquatable<TeamFoundationServer> Members

        public bool Equals(BaseTeamFoundationServer other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return other.Uri == Uri;
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            var cast = obj as BaseTeamFoundationServer;
            if (cast == null)
                return false;
            return Equals(cast);
        }

        public override int GetHashCode()
        {
            return Uri.GetHashCode();
        }

        public static bool operator ==(BaseTeamFoundationServer left, BaseTeamFoundationServer right)
        {
            return ReferenceEquals(null, left) ? ReferenceEquals(null, right) : left.Equals(right);
        }

        public static bool operator !=(BaseTeamFoundationServer left, BaseTeamFoundationServer right)
        {
            return !(left == right);
        }

        #endregion Equal
    }
}

