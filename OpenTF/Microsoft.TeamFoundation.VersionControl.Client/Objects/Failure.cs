//
// Microsoft.TeamFoundation.VersionControl.Client.Failure
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
using System.Text;
using System.Xml.Linq;
using Microsoft.TeamFoundation.VersionControl.Client.Helpers;
using Microsoft.TeamFoundation.VersionControl.Client.Enums;

namespace Microsoft.TeamFoundation.VersionControl.Client.Objects
{
    //    <s:complexType name="Failure">
    //        <s:sequence>
    //            <s:element minOccurs="0" maxOccurs="1" name="Warnings" type="tns:ArrayOfWarning"/>
    //            <s:element minOccurs="0" maxOccurs="1" name="Message" type="s:string"/>
    //        </s:sequence>
    //        <s:attribute default="None" name="req" type="tns:RequestType"/>
    //        <s:attribute name="code" type="s:string"/>
    //        <s:attribute default="Error" name="sev" type="tns:SeverityType"/>
    //        <s:attribute name="computer" type="s:string"/>
    //        <s:attribute name="ident" type="s:string"/>
    //        <s:attribute name="local" type="s:string"/>
    //        <s:attribute name="res" type="s:string"/>
    //        <s:attribute name="item" type="s:string"/>
    //        <s:attribute default="0" name="itemid" type="s:int"/>
    //        <s:attribute name="ws" type="s:string"/>
    //        <s:attribute name="owner" type="s:string"/>
    //    </s:complexType>
    //    <s:complexType name="ArrayOfWarning">
    //        <s:sequence>
    //            <s:element minOccurs="0" maxOccurs="unbounded" name="Warning" nillable="true" type="tns:Warning"/>
    //        </s:sequence>
    //    </s:complexType>
    //    <s:complexType name="Warning">
    //        <s:attribute default="ResourcePendingChangeWarning" name="wrn" type="tns:WarningType"/>
    //        <s:attribute default="0" name="chgEx" type="s:int"/>
    //        <s:attribute default="None" name="chg" type="tns:ChangeType"/>
    //        <s:attribute name="user" type="s:string"/>
    //        <s:attribute name="userdisp" type="s:string"/>
    //        <s:attribute name="cpp" type="s:string"/>
    //        <s:attribute name="ws" type="s:string"/>
    //    </s:complexType>
    //        <s:simpleType name="WarningType">
    //        <s:restriction base="s:string">
    //            <s:enumeration value="Invalid"/>
    //            <s:enumeration value="ResourcePendingChangeWarning"/>
    //            <s:enumeration value="NamespacePendingChangeWarning"/>
    //            <s:enumeration value="StaleVersionWarning"/>
    //        </s:restriction>
    //    </s:simpleType>
    public sealed class Failure
    {
        //<Failure req="None or Add or Branch or Encoding or Edit or Delete or Lock or Rename or Undelete or Property" code="string" sev="Error or Warning" computer="string" ident="string" local="string" res="string" item="string" itemid="int" ws="string" owner="string">
        //    <Warnings>
        //        <Warning xsi:nil="true" />
        //        <Warning xsi:nil="true" />
        //    </Warnings>
        //    <Message>string</Message>
        //</Failure>
        internal static Failure FromXml(XElement element)
        {
            Failure failure = new Failure();
            failure.RequestType = EnumHelper.ParseRequestType(element.GetAttribute("req"));
            if (!string.IsNullOrEmpty(element.GetAttribute("sev")))
            {
                failure.SeverityType = EnumHelper.ParseSeverityType(element.GetAttribute("sev"));
            }
            failure.Code = element.GetAttribute("code");
            failure.ComputerName = element.GetAttribute("computer");
            failure.IdentityName = element.GetAttribute("ident");
            failure.LocalItem = element.GetAttribute("local");
            failure.ServerItem = element.GetAttribute("item");
            failure.ItemId = GeneralHelper.XmlAttributeToInt(element.GetAttribute("itemid"));
            if (element.Element(element.Name.Namespace + "Message") != null)
                failure.Message = element.Element(element.Name.Namespace + "Message").Value;
            return failure;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Failure instance ");
            sb.Append(GetHashCode());

            sb.Append("\n	 Message: ");
            sb.Append(Message);

            sb.Append("\n	 Local Item: ");
            sb.Append(LocalItem);

            sb.Append("\n	 Request Type: ");
            sb.Append(RequestType);

            return sb.ToString();
        }

        public RequestType RequestType { get; private set; }

        public SeverityType SeverityType { get; private set; }

        public string Code { get; private set; }

        public FailureException Exception
        {
            get
            {
                FailureException exception;
                if (!Enum.TryParse<FailureException>(Code, true, out exception))
                    exception = FailureException.Other;
                return exception;
            }
        }

        public string ComputerName { get; private set; }

        public string IdentityName { get; private set; }

        public string LocalItem { get; private set; }

        public string Message { get; private set; }

        public string ServerItem { get; private set; }

        public int ItemId { get; private set; }

        public string ResourceName { get; private set; }

        public string WorkspaceOwner { get; private set; }

        public string WorkspaceName { get; private set; }
    }
}

