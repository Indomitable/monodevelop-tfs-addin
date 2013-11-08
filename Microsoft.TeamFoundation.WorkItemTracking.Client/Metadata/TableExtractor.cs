//
// MetaDataExtractor.cs
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
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.Security.Policy;
using System.Reflection;
using System;
using System.Globalization;

namespace Microsoft.TeamFoundation.WorkItemTracking.Client.Metadata
{
    public class TableExtractor<T>
        where T: class
    {
        readonly XNamespace ns = "http://schemas.microsoft.com/TeamFoundation/2005/06/WorkItemTracking/ClientServices/03";
        readonly XElement response;
        readonly string tableName;

        public TableExtractor(XElement response, string tableName)
        {
            this.tableName = tableName;
            this.response = response;
        }

        private XElement GetTable()
        {
            var tables = response.Descendants(ns + "table");
            return tables.FirstOrDefault(t => string.Equals(t.Attribute("name").Value, tableName, System.StringComparison.OrdinalIgnoreCase));
        }

        private XElement[] GetColumns(XElement table)
        {
            return table.Element(ns + "columns").Elements(ns + "c").ToArray();
        }

        private string GetFieldName(PropertyInfo prop)
        {
            var attributes = prop.GetCustomAttributes(typeof(TableFieldNameAttribute), true);
            if (attributes.Length > 0)
                return ((TableFieldNameAttribute)attributes[0]).FieldName;
            else
                return string.Empty;
        }

        private Dictionary<int, Tuple<PropertyInfo, string>> BuildMapping(XElement table, XElement[] columns)
        {
            var result = new Dictionary<int, Tuple<PropertyInfo, string>>();
            var properties = typeof(T).GetProperties();
            foreach (var prop in properties)
            {
                string fieldName = GetFieldName(prop);
                if (string.IsNullOrEmpty(fieldName))
                    continue;
                for (int i = 0; i < columns.Length; i++)
                {
                    var column = columns[i];
                    if (string.Equals(column.Element(ns + "n").Value, fieldName, StringComparison.OrdinalIgnoreCase))
                    {
                        result.Add(i, new Tuple<PropertyInfo, string>(prop, column.Element(ns + "t").Value));
                        break;
                    }
                }
            }
            return result;
        }

        private object ConvertVal(string value, string type)
        {
            switch (type)
            {
                case "System.Int32":
                    return Convert.ToInt32(value);
                case "System.Boolean":
                    return string.Equals(value.ToLowerInvariant(), "true");
                case "System.Guid":
                    return Guid.ParseExact(value, "N");
                case "System.DateTime":
                    return DateTime.Parse(value);
                default:
                    return value;
            }
        }

        public List<T> Extract()
        {
            var result = new List<T>();
            var table = GetTable();
            if (table == null)
                return result;
            var columns = GetColumns(table);
            var mapping = BuildMapping(table, columns);
            var rows = table.Element(ns + "rows").Elements(ns + "r");
            foreach (var row in rows)
            {
                var fields = row.Elements(ns + "f").ToArray();
                T obj = Activator.CreateInstance<T>();
                var indexer = 0;
                for (int i = 0; i < columns.Length; i++)
                {
                    var field = fields[indexer];
                    if (field.Attribute("k") != null)
                        i = Convert.ToInt32(field.Attribute("k").Value);
                    Tuple<PropertyInfo, string> map;
                    if (!mapping.TryGetValue(i, out map))
                    {
                        indexer++;
                        continue;
                    }
                    map.Item1.SetValue(obj, ConvertVal(field.Value, map.Item2), new object[0]);
                    indexer++;
                }
                result.Add(obj);
            }
            return result;
        }
    }
}

