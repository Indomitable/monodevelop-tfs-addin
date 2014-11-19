using System;
using System.Net;

namespace Microsoft.TeamFoundation.Client
{
    public interface INetworkServer
    {
        NetworkCredential Credentials { get; }
    }
}

