//
// RPNToXml.cs
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
using System.Xml.Linq;
using System.Linq;

namespace Microsoft.TeamFoundation.WorkItemTracking.Client.Query
{
    class RPNToXml
    {
        readonly List<Node> nodes;

        public RPNToXml(List<Node> nodes)
        {
            this.nodes = nodes;
        }

        class NodeHierarchy
        {
            public NodeHierarchy(Node obj)
            {
                Children = new List<NodeHierarchy>();
                Value = obj;
            }

            public NodeHierarchy Parent { get; set; }

            public Node Value { get; set; }

            public List<NodeHierarchy> Children { get; set; }
        }

        NodeHierarchy CreateHierarchy()
        {
            if (nodes.Count == 0)
                return null;
            if (nodes[0].NodeType != NodeType.Operator)
                throw new Exception("Wrong RPN");
            var linear = nodes.Select(x => new NodeHierarchy(x)).ToList();
            var top = linear[0];
            for (int i = 1; i < linear.Count; i++)
            {
                if (linear[i].Value.NodeType == NodeType.Operator)
                {
                    bool foundParent = false;
                    for (int j = i; j > 0; j--)
                    {
                        if (linear[j].Value.NodeType == NodeType.Operator &&
                            linear[j - 1].Value.NodeType == NodeType.Operator)
                        {
                            linear[i].Parent = linear[j - 1];
                            linear[j - 1].Children.Add(linear[i]);
                            foundParent = true;
                            break;
                        }
                    }
                    if (!foundParent)
                    {
                        linear[i].Parent = linear[0];
                        linear[0].Children.Add(linear[i]);
                    }
                }
                else
                {
                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (linear[j].Value.NodeType == NodeType.Operator)
                        {
                            linear[i].Parent = linear[j];
                            linear[j].Children.Add(linear[i]);
                            break;
                        }
                    }
                }
            }
            return top;
        }

        internal XElement Process()
        {
            var hierarchy = CreateHierarchy();
            var top = CreateGroupElement((OperatorNode)hierarchy.Value);
            foreach (var child in hierarchy.Children)
            {
                ProcessChild(child, top);
            }
            return top;
        }

        void ProcessChild(NodeHierarchy element, XElement parent)
        {
            if (element.Value.NodeType == NodeType.Operator)
            {
                var @group = CreateGroupElement((OperatorNode)element.Value);
                parent.Add(@group);
                foreach (var child in element.Children)
                {
                    ProcessChild(child, @group);
                }
            }
            else
            {
                var expression = CreateExpressionElement((ConditionalNode)element.Value);
                parent.Add(expression);
            }
        }

        XElement CreateGroupElement(OperatorNode node)
        {
            var element = new XElement("Group", new XAttribute("GroupOperator", node.Operator.ToString().ToLowerInvariant()));
            return element; 
        }

        XElement CreateExpressionElement(ConditionalNode conditionalNode)
        {
            var column = ((FieldNode)conditionalNode.Left).Field;
            var value = string.Empty;
            if (conditionalNode.Right.NodeType == NodeType.Constant)
                value = Convert.ToString(((ConstantNode)conditionalNode.Right).Value);
            else //Should use parameter evaluator.
                value = ((ParameterNode)conditionalNode.Right).ParameterName;

            var element = new XElement("Expression", new XAttribute("Column", column), 
                              new XAttribute("FieldType", ""), 
                              new XAttribute("Operator", conditionalNode.Condition.ToString().ToLowerInvariant()));
            element.Add(new XElement("String", value));

            return element;
        }
    }
}

