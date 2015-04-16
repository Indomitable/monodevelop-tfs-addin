// PendingSet.cs
// 
// Author:
//       Ventsislav Mladenov
// 
// The MIT License (MIT)
// 
// Copyright (c) 2013-2015 Ventsislav Mladenov
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

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using MonoDevelop.VersionControl.TFS.Helpers;

namespace MonoDevelop.VersionControl.TFS.VersionControl.Models
{
    //<s:complexType name="PendingSet">
    //    <s:sequence>
    //        <s:element minOccurs="0" maxOccurs="1" name="PendingChanges" type="tns:ArrayOfPendingChange"/>
    //    </s:sequence>
    //    <s:attribute name="computer" type="s:string"/>
    //    <s:attribute name="owner" type="s:string"/>
    //    <s:attribute name="ownerdisp" type="s:string"/>
    //    <s:attribute name="owneruniq" type="s:string"/>
    //    <s:attribute name="ownership" type="s:int" use="required"/>
    //    <s:attribute name="name" type="s:string"/>
    //    <s:attribute name="type" type="tns:PendingSetType" use="required"/>
    //    <s:attribute name="signature" type="s1:guid" use="required"/>
    //</s:complexType>
    internal sealed class PendingSet
    {
        private PendingSet()
        {
            PendingChanges = new List<PendingChange>();
        }

        public static PendingSet FromXml(XElement element)
        {
            PendingSet pSet = new PendingSet();
            pSet.Computer = element.GetAttributeValue("computer");
            pSet.Owner = element.GetAttributeValue("owner");
            pSet.PendingChanges.AddRange(element.Descendants(element.Name.Namespace + "PendingChange").Select(PendingChange.FromXml));
            return pSet;
        }

        public List<PendingChange> PendingChanges { get; private set; }

        public string Computer { get; private set; }

        public string Owner { get; private set; }
    }
}
