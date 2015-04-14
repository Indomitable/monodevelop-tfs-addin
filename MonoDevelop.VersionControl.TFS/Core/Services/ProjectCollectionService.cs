//
// TeamFoundationProjectCollection.cs
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
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.XPath;
using MonoDevelop.VersionControl.TFS.Core.Structure;
using MonoDevelop.VersionControl.TFS.Helpers;

namespace MonoDevelop.VersionControl.TFS.Core.Services
{
    internal sealed class ProjectCollectionService : TFSService
    {

        #region implemented abstract members of TFSService

        public override XNamespace MessageNs
        {
            get
            {
                return TFSServiceMessage.ServiceNs;
            }
        }

        #endregion

        private const string servicePath = "/TeamFoundation/Administration/v3.0/CatalogService.asmx";
        private readonly string projectCollectionsTypeId = "26338d9e-d437-44aa-91f2-55880a328b54";

        internal ProjectCollectionService(Uri baseUri)
            : base(baseUri, servicePath)
        {

        }

        private IEnumerable<XElement> GetXmlCollections()
        {
            var invoker = GetSoapInvoker();
            //var serviceNs = 
            var message = invoker.CreateEnvelope("QueryResourcesByType");
            message.AddElement(new XElement("resourceTypes", new XElement("guid", projectCollectionsTypeId)));
            var resultElement = invoker.InvokeResult();
            return resultElement.XPathSelectElements(
                string.Format("./msg:CatalogResources/msg:CatalogResource[@ResourceTypeIdentifier='{0}']", projectCollectionsTypeId), 
                this.NsResolver
            );
        }

        public List<ProjectCollection> GetProjectCollections(TeamFoundationServer server)
        {
            var collection = new List<ProjectCollection>();
            foreach (var catalogResource in GetXmlCollections())
            {
                collection.Add(ProjectCollection.FromServerXml(catalogResource, server));
            }
            return collection;

        }
    }
}

