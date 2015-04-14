using System.Xml.Linq;

namespace MonoDevelop.VersionControl.TFS.Core
{
    internal interface ISoapInvoker
    {
        XElement CreateEnvelope(string methodName);
        SoapEnvelope CreateEnvelope(string methodName, string headerName);
        XElement MethodResultExtractor(XElement responseElement);
        XElement InvokeResult();
        XElement InvokeResponse();
    }
}