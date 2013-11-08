//
// LocationService.cs
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
using System.Xml.XPath;

namespace Microsoft.TeamFoundation.Client.Services
{
    public class LocationService : TFSService
    {

        #region implemented abstract members of TFSService

        public override System.Xml.Linq.XNamespace MessageNs
        {
            get
            {
                return TeamFoundationServerServiceMessage.ServiceNs;
            }
        }

        public override Uri Url
        {
            get
            {
                return collection.LocationServiceUrl;
            }
        }

        #endregion

        readonly ProjectCollection collection;

        public LocationService(ProjectCollection collection)
        {
            this.Server = collection.Server;
            this.collection = collection;
        }

        internal T LoadService<T>()
            where T: TFSCollectionService
        {
            SoapInvoker invoker = new SoapInvoker(this);
            invoker.CreateEnvelope("QueryServices");
            var resultEl = invoker.InvokeResult();
            T service = Activator.CreateInstance<T>();
            var serviceEl = resultEl.XPathSelectElement(string.Format("./msg:ServiceDefinitions/msg:ServiceDefinition[@identifier='{0}']", service.ServiceResolver.Id), 
                                TeamFoundationServerServiceMessage.NsResolver);
            if (serviceEl == null)
                throw new Exception("Service not found");
            service.Server = this.collection.Server;
            service.Collection = this.collection;
            service.RelativeUrl = serviceEl.Attribute("relativePath").Value;
            return service;
        }
    }
}

