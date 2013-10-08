using System;
using Microsoft.TeamFoundation.Client;
using System.Net;
using System.Runtime.ConstrainedExecution;

namespace MonoDevelop.VersionControl.TFS.Tests
{
    public class BaseTFSConnectTest
    {
        protected TeamFoundationServer GetServer()
        {
            var credentials = new NetworkCredential { Domain = "snd", UserName = "mono_tfs_plugin_cp", Password = "mono_tfs_plugin" };
            //Should Add sertificates: http://www.mono-project.com/FAQ:_Security
            return new TeamFoundationServer(new Uri("https://tfs.codeplex.com/tfs/"), "codeplex", credentials);
        }
    }
}

