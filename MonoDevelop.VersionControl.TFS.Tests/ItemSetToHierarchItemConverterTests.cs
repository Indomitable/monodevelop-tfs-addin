using NUnit.Framework;
using System;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using MonoDevelop.VersionControl.TFS.Helpers;
using System.Net;
using System.Diagnostics;

namespace MonoDevelop.VersionControl.TFS.Tests
{
    [TestFixture()]
    public class ItemSetToHierarchItemConverterTests
    {
        [Test()]
        public void TestCase()
        {
            using (var tfsServer = new TeamFoundationServer("http://localhost:8080/tfs", new NetworkCredential() { UserName = "", Password = "" }))
            {
                tfsServer.Authenticate();
                var versionControl = (VersionControlServer)tfsServer.GetService(typeof(VersionControlServer));
                var itemSet = versionControl.GetItems(new ItemSpec(VersionControlPath.RootFolder, RecursionType.Full), VersionSpec.Latest, DeletedState.NonDeleted, ItemType.Folder, false);
                Stopwatch watch = new Stopwatch();
                watch.Start();
                HierarchyItem topItem = ItemSetToHierarchItemConverter.Convert(itemSet.Items);
                watch.Stop();
                Console.WriteLine(watch.ElapsedMilliseconds);
                Assert.AreEqual(32, topItem.Children.Count);
            }
        }
    }
}

