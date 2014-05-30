//
// Microsoft.TeamFoundation.VersionControl.Client.VersionSpec
//
// Authors:
//	Joel Reed (joelwreed@gmail.com)
//  Ventsislav Mladenov (ventsislav.mladenov@gmail.com)
//
// Copyright (C) 2013 Joel Reed, Ventsislav Mladenov
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Microsoft.TeamFoundation.VersionControl.Client.Objects
{
    public abstract class VersionSpec
    {
        protected static readonly XNamespace XsiNs = XmlSchema.InstanceNamespace;
        static readonly LatestVersionSpec latest = new LatestVersionSpec();

        internal abstract XElement ToXml(XName element);

        public static VersionSpec ParseSingleSpec(string versionSpec, string user)
        {
            if (string.IsNullOrEmpty(versionSpec))
                throw new VersionControlException("Invalid version specification");

            char prefix = Char.ToUpper(versionSpec[0]);
            if (prefix == 'T')
                return Latest;
            else if (prefix == 'C')
                return new ChangesetVersionSpec(versionSpec.Substring(1));
            else if (prefix == 'D')
                return new DateVersionSpec(DateTime.Parse(versionSpec.Substring(1)));
            else if (prefix == 'L')
                return new LabelVersionSpec(versionSpec.Substring(1));
            else if (prefix == 'W')
                return new WorkspaceVersionSpec(versionSpec.Substring(1), user);

            return null;
        }

        public abstract string DisplayString { get; }

        public static VersionSpec Latest { get { return latest; } }

        public override string ToString()
        {
            return string.Format("VersionSpec: {0}", DisplayString);
        }
    }
}
