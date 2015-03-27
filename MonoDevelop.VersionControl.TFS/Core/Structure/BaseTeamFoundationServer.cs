using System;
using System.Collections.Generic;
using MonoDevelop.VersionControl.TFS.Configuration;
using MonoDevelop.VersionControl.TFS.Helpers;
using MonoDevelop.VersionControl.TFS.Core.Services;

namespace MonoDevelop.VersionControl.TFS.Core.Structure
{
    abstract class BaseTeamFoundationServer: IEquatable<BaseTeamFoundationServer>, IComparable<BaseTeamFoundationServer>
    {
        protected ServerConfig Config;
        private readonly string password;

        protected BaseTeamFoundationServer(ServerConfig config)
        {
            this.Config = config;
            password = config.HasPassword ? config.Password : CredentialsManager.GetPassword(config.Url);
        }

        public ServerConfig FetchServerStructure()
        {
            var projectCollectionsService = new ProjectCollectionService(Uri);
            var projectCollectionConfigs = projectCollectionsService.GetProjectCollections();
            this.Config.ProjectCollections.Clear();
            foreach (var projectCollectionConfig in projectCollectionConfigs)
            {
                projectCollectionConfig.Projects.Clear();
                var projectCollection = new ProjectCollection(projectCollectionConfig, this);
                projectCollectionConfig.Projects.AddRange(projectCollection.FetchProjects());
            }
            this.Config.ProjectCollections.AddRange(projectCollectionConfigs);
            return this.Config;
        }

        public bool IsPasswordSavedInXml { get { return this.Config.HasPassword; } }

        public abstract string UserName { get; }

        public string Password { get { return this.password; } }

        public string Name { get { return this.Config.Name; } }

        public Uri Uri { get { return this.Config.Url; } }

        public List<ProjectCollection> ProjectCollections { get; set; }

        #region Equal

        #region IComparable<BaseTeamFoundationServer> Members

        public int CompareTo(BaseTeamFoundationServer other)
        {
            return string.Compare(Uri.ToString(), other.Uri.ToString(), StringComparison.Ordinal);
        }

        #endregion

        #region IEquatable<BaseTeamFoundationServer> Members

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

