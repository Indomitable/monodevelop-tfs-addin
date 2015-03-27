using System;
using System.Xml.Schema;
using Microsoft.TeamFoundation.VersionControl.Client;
using System.Xml.Linq;
using System.IO;
using System.Xml;
using System.Text;
using System.Xml.XPath;
using System.Linq;
using Xunit;

namespace Tests.OpenTF
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

