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
using System.Linq;
using Microsoft.TeamFoundation.Client;
using System.Collections.Generic;

namespace Microsoft.TeamFoundation.Client
{
    public class CommonStructureService : TfsService
    {
        class CommonStructureServiceResolver : IServiceResolver
        {
            public string Id
            {
                get
                {
                    return "d9c3f8ff-8938-4193-919b-7588e81cb730";
                }
            }

            public string ServiceType
            {
                get
                {
                    return "CommonStructure";
                }
            }
        }

        public override System.Xml.Linq.XNamespace MessageNs
        {
            get
            {
                return "http://schemas.microsoft.com/TeamFoundation/2005/06/Services/Classification/03";
            }
        }

        public override IServiceResolver ServiceResolver
        {
            get
            {
                return new CommonStructureServiceResolver();
            }
        }

        public List<ProjectInfo> ListProjects()
        {
            SoapInvoker invoker = new SoapInvoker(this);
            invoker.CreateEnvelope("ListProjects", this.MessageNs);
            var resultEl = invoker.InvokeResult();
            return new List<ProjectInfo>(resultEl.Elements(this.MessageNs + "ProjectInfo").Select(x => ProjectInfo.FromXml(Collection, x)));
        }

        public List<ProjectInfo> ListAllProjects()
        {
            SoapInvoker invoker = new SoapInvoker(this);
            invoker.CreateEnvelope("ListAllProjects", this.MessageNs);
            var resultEl = invoker.InvokeResult();
            return new List<ProjectInfo>(resultEl.Elements(this.MessageNs + "ProjectInfo").Select(x => ProjectInfo.FromXml(Collection, x)));
        }
    }
}

