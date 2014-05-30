//
// ArrayOfValues.cs
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

using Microsoft.TeamFoundation.WorkItemTracking.Client.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.TeamFoundation.WorkItemTracking.Client.Query.Where
{
    class ArrayOfValues : Node
    {
        public ArrayOfValues(string word)
        {
            Values = new List<Node>();
            if (string.IsNullOrEmpty(word))
                return;
            var arrayOfWords = word.Split(new []{ ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var w in arrayOfWords)
            {
                var trimmedWord = w.Trim();
                if (trimmedWord.StartsWith("@", StringComparison.Ordinal))
                    Values.Add(new ParameterNode(trimmedWord));
                else
                    Values.Add(new ConstantNode(trimmedWord));
            }
        }

        public override NodeType NodeType { get { return NodeType.ArrayOfValues; } }

        public List<Node> Values { get; set; }

        public override string ToString()
        {
            return "(" + string.Join(", ", Values.Select(n => n.ToString())) + ")";
        }
    }
}
