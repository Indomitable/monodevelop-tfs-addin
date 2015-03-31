//
// ConfigTests.cs
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
using System.Web.Services.Protocols;
using Xunit;
using System.Xml.Linq;
using MonoDevelop.VersionControl.TFS.Core;
using MonoDevelop.VersionControl.TFS.Core.Structure;

namespace Tests
{
    public class ConfigTests
    {
//        private TeamFoundationServer CreateServerConfig()
//        {
//            return new ServerConfig(ServerType.OnPremise, "TestServer", new Uri("http://tfs.company.com:8080/tfs"), "MyUser", "TestDomain", "MyUser", "MyPassword");
//        }
//
//        private ProjectCollection CreateProjectCollectionConfig()
//        {
//            var collection = new ProjectCollection();
//            collection.Id = Guid.Parse("a4c1be27-e9c4-470d-9adb-fdb3bbb72ccb");
//            collection.Name = "ProjectCollection";
//            collection.LocationServicePath = "/ProjectCollection/Services/v3.0/LocationService.asmx";
//            return collection;
//        }
//
//
//        [Fact]
//        public void ServerToXml()
//        {
//            var server = CreateServerConfig();
//            var xml = server.ToConfigXml();
//            Assert.Equal("<Server Name=\"TestServer\" Type=\"0\" " +
//                "Url=\"http://tfs.company.com:8080/tfs\" Domain=\"TestDomain\" " +
//                "UserName=\"MyUser\" AuthUserName=\"\" Password=\"MyPassword\" />", 
//                xml.ToString());
//        }
//
//        [Fact]
//        public void ConfigToXml()
//        {
//            const string config = "<Server Name=\"TestServer\" Type=\"1\" " +
//                "Url=\"http://tfs.visualstudio.com/tfs\" Domain=\"TestDomain\" " +
//                "UserName=\"MyUser\" AuthUserName=\"TestAuthUser\" />";
//            var server = TeamFoundationServer.FromConfigXml(XElement.Parse(config));
//            Assert.Equal("TestServer", server.Name);
//            Assert.Equal("http://tfs.visualstudio.com/tfs", server.Url.AbsoluteUri);
//            Assert.Equal("TestDomain", server.Domain);
//            Assert.Equal("MyUser", server.UserName);
//            Assert.Null(server.Password);
//            Assert.Equal(false, server.HasPassword);
//            Assert.Equal("TestAuthUser", server.AuthUserName);
//        }
//
//        [Fact]
//        public void ProjectCollectionToXml()
//        {
//            var collection = CreateProjectCollectionConfig();
//            Assert.Equal("<ProjectCollection Id=\"a4c1be27-e9c4-470d-9adb-fdb3bbb72ccb\" " +
//                "Name=\"ProjectCollection\" " +
//                "Url=\"http://tfs.company.local:8080/tfs/ProjectCollection\" " +
//                "LocationServiceUrl=\"http://tfs.company.local:8080/tfs/ProjectCollection/Services/v3.0/LocationService.asmx\" />", 
//                collection.ToConfigXml().ToString());
//        }
//
//        [Fact]
//        public void ProjectCollectionFromXml()
//        {
//            const string xml = "<ProjectCollection Id=\"a4c1be27-e9c4-470d-9adb-fdb3bbb72ccb\" " + 
//                "Name=\"ProjectCollection\" " + 
//                "Url=\"http://tfs.company.local:8080/tfs/ProjectCollection\" " + 
//                "LocationServiceUrl=\"http://tfs.company.local:8080/tfs/ProjectCollection/Services/v3.0/LocationService.asmx\">" + 
//                "<Project Name=\"TestProject1\" Status=\"WellFormed\" Uri=\"vstfs:///Classification/TeamProject/232f5dac-5e7e-4c4c-98ca-1109c5e93273\" />" + 
//                "<Project Name=\"TestProject2\" Status=\"WellFormed\" Uri=\"vstfs:///Classification/TeamProject/d561562e-0d7b-4e26-bbc0-c881781ca347\" />" + 
//                "</ProjectCollection>";
//            var collection = ProjectCollectionConfig.FromConfigXml(XElement.Parse(xml));
//            Assert.Equal(2, collection.Projects.Count);
//            Assert.Equal("ProjectCollection", collection.Name);
//            Assert.Equal("TestProject1", collection.Projects[0].Name);
//            Assert.Equal(ProjectState.WellFormed, collection.Projects[0].State);
//            Assert.Equal("vstfs:///Classification/TeamProject/d561562e-0d7b-4e26-bbc0-c881781ca347", collection.Projects[1].Url.AbsoluteUri);
//        }
    }
}

