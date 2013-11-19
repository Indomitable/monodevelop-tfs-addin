//
// NodeOptimizator.cs
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
using System;
using System.Collections.Generic;

namespace Microsoft.TeamFoundation.WorkItemTracking.Client.Query
{
    public static class NodeManager
    {
        /// <summary>
        /// Nodes are ordered field condition constant/parameter, optimize to one node condition with left and right set to field and constant/parameter
        /// </summary>
        /// <param name="input">Nodes.</param>
        internal static List<Node> Optimize(List<Node> input)
        {
            List<Node> output = new List<Node>();
            for (int i = 0; i < input.Count; i++)
            {
                var node = input[i];
                if (node.NodeType != NodeType.Condition &&
                    node.NodeType != NodeType.Field &&
                    node.NodeType != NodeType.Constant &&
                    node.NodeType != NodeType.Parameter)
                {
                    output.Add(node);
                }
                else if (node.NodeType == NodeType.Condition)
                {
                    var condition = (ConditionalNode)node;
                    condition.Left = input[i - 1];
                    condition.Right = input[i + 1];
                    output.Add(condition);
                }
            }
            return output;
        }
    }
}

