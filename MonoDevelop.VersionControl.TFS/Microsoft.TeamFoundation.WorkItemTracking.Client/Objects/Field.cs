//
// Field.cs
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
using Microsoft.TeamFoundation.WorkItemTracking.Client.Metadata;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.TeamFoundation.WorkItemTracking.Client.Objects
{
    public enum FieldType
    {
        None,
        String,
        Integer,
        DateTime,
        PlainText,
        Html,
        TreePath,
        History,
        Double,
        Guid,
        Bool
    }

    public class Field
    {
        [TableFieldName("FldID")]
        public int Id { get; set; }

        [TableFieldName("Type")]
        public int Type { get; set; }

        [TableFieldName("ParentFldID")]
        public int ParentId { get; set; }

        [TableFieldName("Name")]
        public string Name { get; set; }

        [TableFieldName("ReferenceName")]
        public string ReferenceName { get; set; }

        public bool IsLongField
        {
            get
            {
                return (this.Type & 0xF0) == 64;
            }
        }

        public FieldType FieldType
        {
            get
            {     
                switch (Type)
                {
                    case 16:
                    case 24:
                    case 160:
                    case 528:
                    case 784:
                        return FieldType.String;
                    case 272:
                        return FieldType.TreePath;
                    case 32:
                    case 288:
                        return FieldType.Integer;
                    case 240:
                        return FieldType.Double;
                    case 48:
                        return FieldType.DateTime;
                    case 64:
                        return FieldType.PlainText;
                    case 320:
                        return FieldType.History;
                    case 576:
                        return FieldType.Html;
                    case 208:
                        return FieldType.Guid;
                    case 224:
                        return FieldType.Bool;
                    default:
                        return FieldType.String;
                }
            }
        }
    }

    public class FieldList : List<Field>
    {
        public FieldList(IEnumerable<Field> fields)
            : base(fields)
        {
            
        }

        public new Field this [int id]
        {
            get
            {
                return this.Find(x => x.Id == id);
            }
        }

        public Field this [string name]
        {
            get
            {
                return this.Find(x => string.Equals(x.ReferenceName, name, System.StringComparison.OrdinalIgnoreCase));
            }
        }

        public FieldList GetFieldsByNames(List<string> names)
        {
            return new FieldList(names.Select(n => this[n]));
        }
    }
}

