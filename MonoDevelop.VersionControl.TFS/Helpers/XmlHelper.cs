//
// XmlHelper.cs
//
// Author:
//       Ventsislav Mladenov <ventsislav.mladenov@gmail.com>
//
// Copyright (c) 2015 Ventsislav Mladenov License MIT/X11
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
using System.Xml.Linq;
using System.Collections.Generic;

namespace MonoDevelop.VersionControl.TFS.Helpers
{
    static class XmlHelper
    {
        #region Attributes

        public static string GetAttributeValue(this XElement element, string attributeName)
        {
            if (!element.HasAttributes ||
                element.Attribute(attributeName) == null ||
                string.IsNullOrEmpty(element.Attribute(attributeName).Value))
                return string.Empty;
            return element.Attribute(attributeName).Value;
        }

        public static bool GetBooleanAttribute(this XElement element, string attributeName)
        {
            var value = element.GetAttributeValue(attributeName);
            if (string.IsNullOrWhiteSpace(value))
                return false;
            return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        }

        public static int GetIntAttribute(this XElement element, string attributeName)
        {
            var value = element.GetAttributeValue(attributeName);
            if (string.IsNullOrWhiteSpace(value))
                return 0;
            return Convert.ToInt32(value);
        }

        public static DateTime GetDateAttribute(this XElement element, string attributeName)
        {
            var value = element.GetAttributeValue(attributeName);
            if (string.IsNullOrWhiteSpace(value))
                return DateTime.MinValue;
            return DateTime.Parse(value);
        }

        public static byte[] GetByteArrayAttribute(this XElement element, string attributeName)
        {
            var value = element.GetAttributeValue(attributeName);
            if (string.IsNullOrWhiteSpace(value))
                return new byte[0];
            return Convert.FromBase64String(value);
        }

        #endregion

        #region Element Selection

        public static XElement GetElement(this XElement element, string elementName)
        {
            return element.Element(element.Name.Namespace + elementName);
        }

        public static IEnumerable<XElement> GetElements(this XElement element, string elementName)
        {
            return element.Elements(element.Name.Namespace + elementName);
        }

        public static IEnumerable<XElement> GetDescendants(this XElement element, string elementName)
        {
            return element.Descendants(element.Name.Namespace + elementName);
        }

        #endregion

        #region Element Creation

        public static XElement CreateElement(this XNamespace nameSpace, string elementName, object content)
        {
            return new XElement(nameSpace.GetName(elementName), content);
        }

        public static XElement CreateElement(this XNamespace nameSpace, string elementName, params object[] contents)
        {
            return new XElement(nameSpace.GetName(elementName), contents);
        }

        public static void AddElement(this XElement element, string elementName, object content)
        {
            element.Add(element.Name.Namespace.CreateElement(elementName, content));
        }

        public static void AddElement(this XElement element, string elementName, params object[] contents)
        {
            element.Add(element.Name.Namespace.CreateElement(elementName, contents));
        }

        public static void AddElement(this XElement element, string elementName, IEnumerable<XElement> elements)
        {
            var parentElement = new XElement(element.Name.Namespace.GetName(elementName));
            parentElement.AddElements(elements);
            element.Add(parentElement);
        }

        public static void AddElement(this XElement parentElement, XElement childElement)
        {
            foreach (var subElement in childElement.DescendantsAndSelf())
            {
                subElement.Name = parentElement.Name.Namespace.GetName(subElement.Name.LocalName);
            }
            parentElement.Add(childElement);
        }

        public static void AddElements(this XElement parentElement, IEnumerable<XElement> childElements)
        {
            foreach (var childElement in childElements)
            {
                parentElement.AddElement(childElement);
            }
        }

        #endregion

    }
}

