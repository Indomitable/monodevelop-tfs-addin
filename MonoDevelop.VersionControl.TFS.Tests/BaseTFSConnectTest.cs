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
            //Should Add sertificates: http://www.mono-project.com/FAQ:_Security
            //sudo mozroots --import --ask-remove --machine
            return new TeamFoundationServer(new Uri("https://tfs.codeplex.com/tfs/"), "codeplex", "snd", "mono_tfs_plugin_cp", "mono_tfs_plugin", false);
        }
    }
}

