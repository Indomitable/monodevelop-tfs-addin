//
// ServerConfiguration.cs
//
// Author:
//       Ventsislav Mladenov <ventsislav.mladenov@gmail.com>
//
// Copyright (c) 2015 Ventsislav Mladenov License MIT/X11
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.TeamFoundation.VersionControl.Client;
using MonoDevelop.VersionControl.TFS.Core.Structure;
using MonoDevelop.VersionControl.TFS.Infrastructure;
using MonoDevelop.VersionControl.TFS.VersionControl;
using MonoDevelop.VersionControl.TFS.VersionControl.Models;
using MonoDevelop.VersionControl.TFS.VersionControl.Structure;
using Xunit;

namespace MonoDevelop.VersionControl.TFS.Tests.Core
{
    public class ServerFixture
    {
        internal TeamFoundationServer Server;
        internal const string WorkspaceName = "TEST_COMPUTER_WORKSPACE";
        internal readonly string MapFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TFS_ADDIN_TEST_Projects");
        internal readonly string RootFolder = "monotfstestproject";

        public ServerFixture()
        {
            DependencyInjection.Register(new TestServiceBuilder());

            const string xmlConfig = @"<Server Name=""CodePlex"" Uri=""https://tfs.codeplex.com/tfs"" UserName=""mono_tfs_plugin_cp"">
  <Auth>
    <Ntlm UserName=""mono_tfs_plugin_cp"" Password=""mono_tfs_plugin"" Domain=""snd"" />
  </Auth>
</Server>";

            Server = TeamFoundationServer.FromConfigXml(XElement.Parse(xmlConfig));
            Server.LoadStructure();
        }


        internal IWorkspace GetWorkspace(ProjectCollection collection)
        {
            var workspaceDatas = collection.GetLocalWorkspaces();
            var workspaceData = workspaceDatas.SingleOrDefault(wd => string.Equals(wd.Name, WorkspaceName, StringComparison.OrdinalIgnoreCase));
            if (workspaceData == null)
            {
                workspaceData = new WorkspaceData
                {
                    Computer = Environment.MachineName,
                    Comment = "Test Workspace",
                    Name = WorkspaceName,
                    Owner = Server.UserName,
                    IsLocal = false,
                    WorkingFolders = new List<WorkingFolder> { new WorkingFolder(RepositoryPath.RootPath, MapFolder) }
                };
                collection.CreateWorkspace(workspaceData);
            }
            var topFolder = GetWorkspaceTopFolder();
            if (!Directory.Exists(topFolder))
            {
                Directory.CreateDirectory(topFolder);
            }
            var workspace = DependencyInjection.GetWorkspace(workspaceData, collection);
            if (workspace.PendingChanges.Any())
            {
                var undoItems = workspace.PendingChanges.Select(pc => new ItemSpec(pc.LocalItem, RecursionType.Full));
                workspace.Undo(undoItems);
            }
            return workspace;
        }

        internal string GetWorkspaceTopFolder()
        {
            return Path.Combine(MapFolder, RootFolder);
        }
    }

    [CollectionDefinition("Server")]
    public class ServerCollection : ICollectionFixture<ServerFixture>
    {
        
    }
}

