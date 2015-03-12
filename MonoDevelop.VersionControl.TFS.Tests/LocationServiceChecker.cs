//
// LocationServiceChecker.cs
//
// Author:
//       Ventsislav Mladenov <vmladenov.mladenov@gmail.com>
//
// Copyright (c) 2013 Ventsislav Mladenov
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
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.Net;
using Microsoft.TeamFoundation.Client;
using Xunit;

namespace MonoDevelop.VersionControl.TFS.Tests
{
    public class LocationServiceChecker
    {
        private static XNamespace ns = "http://microsoft.com/webservices/";

        [Fact]
        public void GetLocationServises()
        {
            var service = new ServiceChecker();
            var request = service.CreateRequest("/TeamFoundation/Administration/v3.0/LocationService.asmx");
            XElement message;
            var requestDoc = service.CreateEnvelope("QueryServices", ns, out message);
            requestDoc.Save(request.GetRequestStream());
            Console.WriteLine(requestDoc);
            using (var response = service.GetResponse(request))
            {
                var responseDoc = XDocument.Load(response.GetResponseStream());
                Console.WriteLine(responseDoc);
            }
        }
        //        [Test]
        //        public void GetLocationServisesInvoker()
        //        {
        //            var credentials = new NetworkCredential { Domain = "snd", UserName = "mono_tfs_plugin_cp", Password = "mono_tfs_plugin" };
        //            var soapInvoker = new SoapInvoker("https://tfs.codeplex.com/tfs/", "/TeamFoundation/Administration/v3.0/LocationService.asmx", credentials);
        //            soapInvoker.CreateEnvelope("QueryServices", ns);
        //            Console.WriteLine(soapInvoker.InvokeResult());
        //        }
        [Fact]
        public void GetProjectLocationServises()
        {
            var service = new ServiceChecker();
            var request = service.CreateRequest("/TFS19/Services/v3.0/LocationService.asmx");
            XElement message;
            var requestDoc = service.CreateEnvelope("QueryServices", ns, out message);
            requestDoc.Save(request.GetRequestStream());
            Console.WriteLine(requestDoc);
            using (var response = service.GetResponse(request))
            {
                var responseDoc = XDocument.Load(response.GetResponseStream());
                Console.WriteLine(responseDoc);
            }
        }

        [Fact]
        public void Connect()
        {
            var service = new ServiceChecker();
            var request = service.CreateRequest("/TeamFoundation/Administration/v3.0/LocationService.asmx");
            XElement message;
            var requestDoc = service.CreateEnvelope("Connect", ns, out message);
            requestDoc.Save(request.GetRequestStream());
            Console.WriteLine(requestDoc);
            using (var response = service.GetResponse(request))
            {
                var responseDoc = XDocument.Load(response.GetResponseStream());
                Console.WriteLine(responseDoc);
            }
        }

        [Fact]
        public void GetDefaultCollectionId()
        {
            var service = new ServiceChecker();
            var request = service.CreateRequest("/TeamFoundation/Administration/v3.0/TeamProjectCollectionService.asmx");
            XElement message;
            var requestDoc = service.CreateEnvelope("GetDefaultCollectionId", ns, out message);
            requestDoc.Save(request.GetRequestStream());
            Console.WriteLine(requestDoc);
            using (var response = service.GetResponse(request))
            {
                var responseDoc = XDocument.Load(response.GetResponseStream());
                Console.WriteLine(responseDoc);
            }
        }

        [Fact]
        public void GetCollectionProperties()
        {
            var service = new ServiceChecker();
            var request = service.CreateRequest("/TeamFoundation/Administration/v3.0/TeamProjectCollectionService.asmx");
            XElement message;
            var requestDoc = service.CreateEnvelope("GetCollectionProperties", ns, out message);
            message.Add(new XElement(ns + "ids", new XElement(ns + "guid", Guid.Empty)));
            requestDoc.Save(request.GetRequestStream());
            Console.WriteLine(requestDoc);
            using (var response = service.GetResponse(request))
            {
                var responseDoc = XDocument.Load(response.GetResponseStream());
                Console.WriteLine(responseDoc);
            }
        }

        [Fact]
        public void QueryNodes()
        {
            var service = new ServiceChecker();
            var request = service.CreateRequest("/TeamFoundation/Administration/v3.0/CatalogService.asmx");
            XElement message;
            var requestDoc = service.CreateEnvelope("QueryNodes", ns, out message);
            requestDoc.Save(request.GetRequestStream());
            Console.WriteLine(requestDoc);
            using (var response = service.GetResponse(request))
            {
                var responseDoc = XDocument.Load(response.GetResponseStream());
                Console.WriteLine(responseDoc);
            }
        }

        [Fact]
        public void QueryResourceTypes()
        {
            var service = new ServiceChecker();
            var request = service.CreateRequest("/TeamFoundation/Administration/v3.0/CatalogService.asmx");
            XElement message;
            var requestDoc = service.CreateEnvelope("QueryResourceTypes", ns, out message);
            requestDoc.Save(request.GetRequestStream());
            Console.WriteLine(requestDoc);
            using (var response = service.GetResponse(request))
            {
                var responseDoc = XDocument.Load(response.GetResponseStream());
                Console.WriteLine(responseDoc);
            }
        }

        [Fact]
        public void QueryResourcesByType()
        {
            var service = new ServiceChecker();
            var request = service.CreateRequest("/TeamFoundation/Administration/v3.0/CatalogService.asmx");
            XElement message;
            var requestDoc = service.CreateEnvelope("QueryResourcesByType", ns, out message);
            message.Add(new XElement(ns + "resourceTypes", new XElement(ns + "guid", "26338d9e-d437-44aa-91f2-55880a328b54")));
            requestDoc.Save(request.GetRequestStream());
            Console.WriteLine(requestDoc);
            using (var response = service.GetResponse(request))
            {
                var responseDoc = XDocument.Load(response.GetResponseStream());
                Console.WriteLine(responseDoc);
            }
        }

        [Fact]
        public void QueryHistory()
        {
            var service = new ServiceChecker();
            var request = service.CreateRequest("/TeamFoundation/Administration/v3.0/CatalogService.asmx");
            XElement message;
            var requestDoc = service.CreateEnvelope("QueryResourcesByType", ns, out message);
            message.Add(new XElement(ns + "resourceTypes", new XElement(ns + "guid", "26338d9e-d437-44aa-91f2-55880a328b54")));
            requestDoc.Save(request.GetRequestStream());
            Console.WriteLine(requestDoc);
            using (var response = service.GetResponse(request))
            {
                var responseDoc = XDocument.Load(response.GetResponseStream());
                Console.WriteLine(responseDoc);
            }
        }
    }
}

