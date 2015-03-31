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
using System.Linq;
using System.Collections.Generic;
using MonoDevelop.VersionControl.TFS.Core.Services.Resolvers;
using MonoDevelop.VersionControl.TFS.Core.Structure;
using MonoDevelop.VersionControl.TFS.Helpers;

namespace MonoDevelop.VersionControl.TFS.Core.Services
{
    [ServiceResolver(typeof(CommonStructureServiceResolver))]
    internal sealed class CommonStructureService : TFSService
    {
        internal CommonStructureService(Uri baseUri, string servicePath)
            : base(baseUri, servicePath)
        {
            
        }

        public override System.Xml.Linq.XNamespace MessageNs
        {
            get
            {
                return "http://schemas.microsoft.com/TeamFoundation/2005/06/Services/Classification/03";
            }
        }

        public List<ProjectInfo> ListAllProjects(ProjectCollection collection)
        {
            SoapInvoker invoker = new SoapInvoker(this);
            invoker.CreateEnvelope("ListAllProjects");
            var resultEl = invoker.InvokeResult();
            return new List<ProjectInfo>(resultEl.GetElements("ProjectInfo").Select(e => ProjectInfo.FromServerXml(e, collection)).OrderBy(p => p.Name));
        }
    }
}

