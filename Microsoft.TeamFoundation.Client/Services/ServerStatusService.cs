//
// IService.cs
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
namespace Microsoft.TeamFoundation.Client.Services
{
    public class ServerStatusService : TFSCollectionService
    {
        class ServerStatusResolver : IServiceResolver
        {
            public string Id
            {
                get
                {
                    return "d395630a-d784-45b9-b8d1-f4b82042a8d0";
                }
            }

            public string ServiceType
            {
                get
                {
                    return "ServerStatus";
                }
            }
        }

        public override System.Xml.Linq.XNamespace MessageNs
        {
            get
            {
                return "http://schemas.microsoft.com/TeamFoundation/2005/06/Services/ServerStatus/03";
            }
        }

        public override IServiceResolver ServiceResolver
        {
            get
            {
                return new ServerStatusResolver();
            }
        }
    }
}
