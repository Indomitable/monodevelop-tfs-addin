//
// TableDictionaryExtractor.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace MonoDevelop.VersionControl.TFS.WorkItemTracking.Metadata
{
    sealed class TableDictionaryExtractor : BaseTableExtractor
    {
        public TableDictionaryExtractor(XElement response, string tableName)
            : base(response, tableName)
        {
        }

        private Dictionary<int, Tuple<string,string>> BuildMapping(XElement[] columns)
        {
            var result = new Dictionary<int, Tuple<string,string>>();
            for (int i = 0; i < columns.Length; i++)
            {
                var column = columns[i];
                var columnName = column.Element(ns + "n").Value;
                var columnType = column.Element(ns + "t").Value;
                result.Add(i, new Tuple<string, string>(columnName, columnType));
            }
            return result;
        }

        public List<Dictionary<string, object>> Extract()
        {
            var result = new List<Dictionary<string, object>>();
            var table = GetTable();
            if (table == null)
                return result;
            var columns = GetColumns(table);
            var mapping = BuildMapping(columns);
            var rows = table.Element(ns + "rows").Elements(ns + "r");
            foreach (var row in rows)
            {
                var fields = row.Elements(ns + "f").ToArray();
                var obj = new Dictionary<string, object>();
                var indexer = 0;
                for (int i = 0; i < fields.Length; i++)
                {
                    var field = fields[i];
                    if (field.Attribute("k") != null)
                        indexer = Convert.ToInt32(field.Attribute("k").Value);
                    var map = mapping[indexer];
                    obj[map.Item1] = DataTypeConverter.Convert(field.Value, map.Item2);
                    indexer++;
                }
                result.Add(obj);
            }
            return result;
        }
    }
}
