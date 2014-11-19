using System;

namespace Microsoft.TeamFoundation.Client
{
    public class VisualStudioServerInfo : BaseServerInfo
    {
        private readonly string url;

        public VisualStudioServerInfo(string name, string url, string tfsUser)
            : base(name)
        {
            this.url = url;
            this.TFSUserName = tfsUser;
        }

        public string TFSUserName { get; set; }

        public override Uri Uri
        {
            get
            {
                return new Uri(this.url);
            }
        }
    }
}

