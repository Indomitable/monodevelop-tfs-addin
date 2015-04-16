// MessageTests.cs
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

using System;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;
using MonoDevelop.VersionControl.TFS.VersionControl.Helpers;
using Xunit;

namespace MonoDevelop.VersionControl.TFS.Tests
{
    public class MessageTests
    {
        [Fact]
        public void CreateHeader()
        {
            XNamespace xsiNs = "http://www.w3.org/2001/XMLSchema-instance";
            XNamespace xsdNs = "http://www.w3.org/2001/XMLSchema";
            XNamespace soapNs = "http://schemas.xmlsoap.org/soap/envelope/";

            XDocument doc = new XDocument(new XDeclaration("1.0", "utf-8", "no"));
            XElement el = new XElement(soapNs + "Envelope", 
                              new XAttribute(XNamespace.Xmlns + "xsi", xsiNs),
                              new XAttribute(XNamespace.Xmlns + "xsd", xsdNs),
                              new XAttribute(XNamespace.Xmlns + "soap", soapNs));

            doc.Add(el);
            doc.Save(Console.Out);
            Assert.Equal("<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" />", el.ToString());
        }

        [Fact]
        public void CreateBody()
        {
            XNamespace xsiNs = XmlSchema.InstanceNamespace;
            XNamespace xsdNs = XmlSchema.Namespace;
            XNamespace soapNs = "http://schemas.xmlsoap.org/soap/envelope/";

            var document = new XDocument(new XDeclaration("1.0", "utf-8", "no"));
            XElement messageEl = new XElement(soapNs + "Envelope", 
                                     new XAttribute(XNamespace.Xmlns + "xsi", xsiNs),
                                     new XAttribute(XNamespace.Xmlns + "xsd", xsdNs),
                                     new XAttribute(XNamespace.Xmlns + "soap", soapNs));

            var soapbody = new XElement(soapNs + "Body");
            XNamespace messageNamespace = "http://schemas.microsoft.com/TeamFoundation/2005/06/VersionControl/ClientServices/03";
            var body = new XElement(messageNamespace + "QueryWorkspaces");
            soapbody.Add(body);
            messageEl.Add(soapbody);
            document.Add(messageEl);
            document.Save(Console.Out);
        }

        [Fact]
        public void ReadResult()
        {
            XElement el = XElement.Parse(@"<GetResult xmlns=""http://schemas.microsoft.com/TeamFoundation/2005/06/VersionControl/ClientServices/03"">
  <ArrayOfGetOperation>
    <GetOperation type=""Folder"" itemid=""43082"" tlocal=""U:\home\vmladenov\Projects\work\AT\Admintool\App_Themes"" titem=""$/Phoenix Admintool/Admintool/App_Themes"" sitem=""/"" sver=""13036"" vrevto=""13036"" nmscnflct=""2"" enc=""-3"" />
    <GetOperation type=""Folder"" itemid=""43080"" tlocal=""U:\home\vmladenov\Projects\work\AT\Admintool\App_Themes\MainTheme"" titem=""$/Phoenix Admintool/Admintool/App_Themes/MainTheme"" sitem=""/"" sver=""13036"" vrevto=""13036"" nmscnflct=""2"" enc=""-3"" />
    <GetOperation type=""File"" itemid=""42360"" tlocal=""U:\home\vmladenov\Projects\work\AT\Admintool\App_Themes\MainTheme\MainSkin.skin"" titem=""$/Phoenix Admintool/Admintool/App_Themes/MainTheme/MainSkin.skin"" sitem=""/"" sver=""20429"" vrevto=""20429"" nmscnflct=""2"" durl=""type=rsa&amp;sfid=167041,0,0,0,0,0,0,0,0,0,0,0,0,0,0&amp;ts=635165749160645552&amp;s=NBccSDtoWnXYm8piUNlUTg4n9yw3ey4406dMhnQUq3bd5mtZGo6Zw%2BO4i7MdQQ71%2BhWDEPf6lpmDQJ5myP63mY0L7w2yySyMtmT%2BLNoLevPotzqbfB4qCXAPvwZSfnrez454%2FoqebSR%2BLDEGXmS4lv4My8qMnDimO0MhbnMHuhZP0KgSs%2FFBFsQtmUmz1sMYxTxtiyLMTwBio487rLA7W6doiFmSDOFhVifn%2BfSylxcINlmavg6LYHLUz1WExwmejIT6rVXNhKxChJ%2FDz0m0zP7eDLX5juqRNw8D%2B0J3ud2JOc480aWHeV5qyBE6GR9B%2Fx%2FTsEDnUt9P8Z%2FLm2UhJQ%3D%3D&amp;fid=167041&amp;iid=ab308402-c25c-4b96-843c-bd0a3801ce1f&amp;cp=/tfs/DefaultCollection/"" enc=""65001"">
      <HashValue>f8goKztyCFn79jd3lxiaPg==</HashValue>
    </GetOperation>
  </ArrayOfGetOperation>
</GetResult>");
            Assert.Equal(3, el.XPathSelectElements("//msg:ArrayOfGetOperation/msg:GetOperation", XmlNamespaces.NsResolver).Count());
        }
    }
}

