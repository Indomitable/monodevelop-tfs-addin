using System;
using System.Xml.Schema;
using NUnit.Framework;
using System.Xml.Linq;
using System.IO;
using System.Xml;
using System.Text;

namespace MonoDevelop.VersionControl.TFS.Tests.OpenTF
{
    [TestFixture]
    public class MessageTests
    {
        [Test]
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
            Assert.AreEqual("<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" />", el.ToString());

        }

        [Test]
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
    }
}

