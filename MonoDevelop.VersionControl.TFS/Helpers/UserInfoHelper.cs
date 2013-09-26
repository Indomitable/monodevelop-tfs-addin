using System;
using System.Net;

namespace MonoDevelop.VersionControl.TFS.Helpers
{
    public static class UserInfoHelper
    {
        public static NetworkCredential ExtractCredentials(Uri serverUrl)
        {
            if (string.IsNullOrEmpty(serverUrl.UserInfo))
            {
                throw new ArgumentException("No User Info!", "serverUrl");
            }
            string[] urlCredentials = serverUrl.UserInfo.Split(':');
            if (urlCredentials.Length != 2)
            {
                throw new ArgumentException("User Info Missformat!", "serverUrl");
            }
            string userAndDomain = urlCredentials[0];
            string password = urlCredentials[1];

            string[] userParts = userAndDomain.Split('@');
            NetworkCredential credentials = new NetworkCredential();

            if (userParts.Length == 2)
            {
                credentials.UserName = userParts[0];
                credentials.Domain = userParts[1];
            }
            else
            {
                credentials.UserName = userAndDomain;
                credentials.Domain = string.Empty;
            }
            credentials.Password = password;
            return credentials;
        }
    }
}

