//
// NodeList.cs
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
using System.Linq;
using Microsoft.TeamFoundation.WorkItemTracking.Client.Objects;
using Microsoft.TeamFoundation.WorkItemTracking.Client.Metadata;

namespace Microsoft.TeamFoundation.WorkItemTracking.Client.Query.Where
{
    class NodeList : List<Node>
    {
        public void RemoveFieldsAndValues()
        {
            var output = new List<Node>();
            for (int i = 0; i < this.Count; i++)
            {
                var node = this[i];
                if (node.NodeType != NodeType.Condition &&
                    node.NodeType != NodeType.Field &&
                    node.NodeType != NodeType.Constant &&
                    node.NodeType != NodeType.Parameter &&
                    node.NodeType != NodeType.ArrayOfValues)
                {
                    output.Add(node);
                }
                else if (node.NodeType == NodeType.Condition)
                {
                    var condition = (ConditionNode)node;
                    condition.Left = this[i - 1];
                    condition.Right = this[i + 1];
                    output.Add(condition);
                }
            }
            this.Clear();
            this.AddRange(output);
        }

        public void ConvertInToOrs()
        {
            var output = new List<Node>();
            for (int i = 0; i < this.Count; i++)
            {
                var node = this[i];
                if (node.NodeType == NodeType.Condition)
                {
                    var condition = (ConditionNode)node;
                    if (condition.Condition == Condition.In)
                    {
                        var array = (ArrayOfValues)condition.Right;
                        output.Add(new OpenBracketNode());
                        foreach (var value in array.Values)
                        {
                            var cNew = new ConditionNode("=");
                            cNew.Left = condition.Left;
                            cNew.Right = value;
                            output.Add(cNew);

                            output.Add(new OperatorNode("or"));
                        }
                        output.RemoveAt(output.Count - 1); //Remove last or
                        output.Add(new CloseBracketNode());
                    }
                    else
                        output.Add(node);
                }
                else
                    output.Add(node);
            }
            this.Clear();
            this.AddRange(output);
        }

        public void Optimize()
        {
            RemoveFieldsAndValues();
            ConvertInToOrs();
            RemoveUnUsedBrackets();
        }

        private int FindMatchingCloseBracket(int openBracketIndex)
        {
            int bracketCount = 0;
            for (int i = openBracketIndex; i < this.Count; i++)
            {
                var node = this[i];
                if (node.NodeType == NodeType.OpenBracket)
                    bracketCount++;
                if (node.NodeType == NodeType.CloseBracket)
                    bracketCount--;
                if (bracketCount == 0)
                {
                    return i;
                }
            }
            throw new Exception("Could not find Close Bracket");
        }

        public NodeList GetSubList(int openBracketIndex)
        {
            if (this[openBracketIndex].NodeType != NodeType.OpenBracket)
                throw new Exception("Not an open bracket");
            var subList = new NodeList();
            var closeBracketIndex = FindMatchingCloseBracket(openBracketIndex);
            //Do not add open and closed brackets.
            for (int i = openBracketIndex + 1; i < closeBracketIndex; i++)
            {
                subList.Add(this[i]);
            }
            //Remove other inner brackets.
            subList.RemoveUnUsedBrackets();
            return subList;
        }

        public void RemoveUnUsedBrackets()
        {
            //Remove unused brackets (([a] = 2)) == [a] = 2
            if (this[0].NodeType != NodeType.OpenBracket || this[this.Count - 1].NodeType != NodeType.CloseBracket)
                return;
            int brackCnt = 0;
            for (int i = 1; i < this.Count - 1; i++)
            {
                var node = this[i];
                if (node.NodeType == NodeType.OpenBracket)
                    brackCnt++;
                if (node.NodeType == NodeType.CloseBracket)
                    brackCnt--;
                if (brackCnt < 0) //Every open bracket should have close bracket , ([a] = 2) and ([b] = @p) 
                    return; 
            }
            if (brackCnt == 0)
            {
                this.RemoveAt(this.Count - 1); //Remove last
                this.RemoveAt(0); //Remove first.
            }
            RemoveUnUsedBrackets();
        }

        public void ExtractOperatorForward()
        {
            var newNodes = ExtractOperatorForward(0, this.Count);
            this.Clear();
            this.AddRange(newNodes);
        }

        private NodeList ExtractOperatorForward(int from, int to)
        {
            NodeList list = new NodeList();
            Operator? currentOperator = null;
            for (int i = from; i < to; i++)
            {
                var node = this[i];
                if (node.NodeType == NodeType.OpenBracket)
                {
                    var closeBracket = FindMatchingCloseBracket(i);
                    var newList = ExtractOperatorForward(i + 1, closeBracket);
                    i = closeBracket;
                    list.Add(new OpenBracketNode());
                    list.AddRange(newList);
                    list.Add(new CloseBracketNode());
                    continue;
                }
                if (node.NodeType == NodeType.Operator)
                {
                    var operatorNode = ((OperatorNode)node);
                    if (!currentOperator.HasValue)
                    {
                        currentOperator = operatorNode.Operator;
                        list.Insert(0, operatorNode);
                    }
                    else
                    {
                        if (currentOperator == operatorNode.Operator)
                            continue;
                        list.Add(operatorNode);
                    }
                }
                else
                {
                    list.Add(node);
                }
            }
            //Analyze list if last is only one operator and condition, push operator forward and mark previous as group.
            if (list.Count > 1)
            {
                if (list[list.Count - 1].NodeType == NodeType.Condition && list[list.Count - 2].NodeType == NodeType.Operator)
                {
                    var op = list[list.Count - 2];
                    list.RemoveAt(list.Count - 2);
                    list.Insert(0, op);
                    list.Insert(1, new OpenBracketNode());
                    list.Insert(list.Count - 1, new CloseBracketNode());
                }
            }
            return list;
        }

        /// <summary>
        /// Projects and Iterations are passed by Id not by Name.
        /// </summary>
        /// <param name="fields">Fields.</param>
        public void FixFields(FieldList fields)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].NodeType == NodeType.Condition)
                {
                    var condition = (ConditionNode)this[i];
                    var fieldNode = (FieldNode)condition.Left;
                    var field = fields[fieldNode.Field];

                    if (field.Id == -42 || field.Id == -12 || field.Id == -7) //Project/Area to AreaId
                    {
                        fieldNode.Field = fields[-2].ReferenceName;
                        condition.Condition = Condition.Under;
                        //Fix Project Name to Project Id
                        if (condition.Right.NodeType == NodeType.Constant)
                        {
                            var rightNode = (ConstantNode)condition.Right;
                            if (rightNode.DataType == ValueDataType.String)
                            {
                                var project = CachedMetaData.Instance.Projects.FirstOrDefault(pr => string.Equals(pr.Name, Convert.ToString(rightNode.Value), StringComparison.OrdinalIgnoreCase));
                                if (project != null)
                                {
                                    rightNode.DataType = ValueDataType.Number;
                                    rightNode.Value = project.Id;
                                }
                            }
                        }
                        continue;
                    }
                    if (field.Id == -105) //Iteration Path to Iteration Id
                    {
                        fieldNode.Field = fields[-104].ReferenceName;
                        condition.Condition = Condition.Under;
                        //Fix Iteration Name to Iteration Id
                        if (condition.Right.NodeType == NodeType.Constant)
                        {
                            var rightNode = (ConstantNode)condition.Right;
                            if (rightNode.DataType == ValueDataType.String)
                            {
                                var iteration = CachedMetaData.Instance.Iterations.LocateIteration(Convert.ToString(rightNode.Value));
                                if (iteration != null)
                                {
                                    rightNode.DataType = ValueDataType.Number;
                                    rightNode.Value = iteration.Id;
                                }
                            }
                        }
                        continue;
                    }
//                    if (field.Id == -1) //Authorized As to PersonId
//                    {
//                        fieldNode.Field = fields[-6].ReferenceName;
//                    }
                }
            }
        }

        public void FillFieldTypes(FieldList fields)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].NodeType == NodeType.Condition)
                {
                    var condition = (ConditionNode)this[i];
                    var fieldNode = (FieldNode)condition.Left;
                    var field = fields[fieldNode.Field];
                    fieldNode.FieldType = field.Type;
                }
            }
        }

        public override string ToString()
        {
            return string.Join(" ", this.Select(n => n.ToString()));
        }
    }
}
