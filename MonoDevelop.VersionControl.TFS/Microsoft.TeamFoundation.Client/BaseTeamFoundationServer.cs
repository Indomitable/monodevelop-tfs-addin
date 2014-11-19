using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Client.Services;
using System.Xml.Linq;

namespace Microsoft.TeamFoundation.Client
{
    public abstract class BaseTeamFoundationServer: IEquatable<BaseTeamFoundationServer>, IComparable<BaseTeamFoundationServer>
    {
        protected BaseTeamFoundationServer(Uri uri, string name, string userName, string password, bool isPasswordSavedInXml)
        {
            this.Uri = uri;
            this.Name = name;
            this.UserName = userName;
            this.IsPasswordSavedInXml = isPasswordSavedInXml;
            this.Password = password;
        }

        public void LoadProjectConnections()
        {
            var projectCollectionsService = new ProjectCollectionService(this);
            this.ProjectCollections = projectCollectionsService.GetProjectCollections();
        }

        public abstract XElement ToLocalXml();

        public bool IsPasswordSavedInXml { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public List<ProjectCollection> ProjectCollections { get; set; }

        public string Name { get; protected set; }

        public Uri Uri { get; protected set; }

        public bool IsDebuMode { get; set;}

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

