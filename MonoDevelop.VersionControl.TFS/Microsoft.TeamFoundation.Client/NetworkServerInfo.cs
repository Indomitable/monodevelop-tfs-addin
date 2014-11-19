using System;

namespace Microsoft.TeamFoundation.Client
{
    public class NetworkServerInfo : BaseServerInfo
    {
        public NetworkServerInfo(string name, Uri uri)
            : base(name)
        {
            this.uri = uri;
        }

        private readonly Uri uri;
        public override Uri Uri { get { return uri; } }
    }
}

