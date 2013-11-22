//
// Node.cs
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
using Mono.Security.X509;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.CompilerServices;
using System.Web.Services.Protocols;
using Microsoft.TeamFoundation.WorkItemTracking.Client.Objects;

namespace Microsoft.TeamFoundation.WorkItemTracking.Client.Query
{
    enum NodeType
    {
        Undefined,
        //System.State
        Field,
        //=,<,>,<=,>=,<>
        Condition,
        //@project
        Parameter,
        //'Active'
        Constant,
        //In clause ('Project1', @project) Could contains Constants or Parameters.
        ArrayOfValues,
        //AND,OR
        Operator,
        //(
        OpenBracket,
        //)
        CloseBracket
    }

    class Node
    {
        public virtual NodeType NodeType { get { return NodeType.Undefined; } }
    }

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

        private ConstantNode CreateConstantFromParameter(ParameterNode parameterNode, ParameterContext context)
        {
            if (string.Equals(parameterNode.ParameterName, "project"))
            {
                ConstantNode node = new ConstantNode(context.ProjectId.ToString());
                node.DataType = ValueDataType.Number;
                return node;
            }
            else if (string.Equals(parameterNode.ParameterName, "me"))
            {
                ConstantNode node = new ConstantNode("'" + context.Me + "'");
                node.DataType = ValueDataType.String;
                return node;
            }
            else if (parameterNode.ParameterName.IndexOf("today", StringComparison.OrdinalIgnoreCase) > -1)
            {
                //TODO: Do date time calc
                ConstantNode node = new ConstantNode(DateTime.Now.ToString("s") + "Z");
                node.DataType = ValueDataType.DateTime;
                return node;
            }
            else
            {
                throw new Exception("Unknown parameter name");
            }
        }

        public void ConvertAllParameters(ParameterContext context)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].NodeType == NodeType.Condition)
                {
                    var condition = (ConditionNode)this[i];
                    if (condition.Right.NodeType == NodeType.Parameter)
                    {
                        var parameterNode = (ParameterNode)condition.Right;
                        condition.Right = CreateConstantFromParameter(parameterNode, context);
                    }
                }
            }
        }

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
                    }
                    if (field.Id == -105) //Iteration Path to Iteration Id
                    {
                        fieldNode.Field = fields[-104].ReferenceName;
                    }
                    if (field.Id == -1) //Authorized As to PersonId
                    {
                        fieldNode.Field = fields[-6].ReferenceName;
                    }
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

    class FieldNode : Node
    {
        public FieldNode(string field)
        {
            this.Field = field.Trim('[', ']');
        }

        public override NodeType NodeType { get { return NodeType.Field; } }

        public string Field { get; set; }

        public int FieldType { get; set; }

        public override string ToString()
        {
            return "[" + Field + "]";
        }
    }

    class ConditionNode : Node
    {
        public ConditionNode(string condition)
        {
            switch (condition)
            {
                case "=":
                    Condition = Condition.Equals;
                    break;
                case "<":
                    Condition = Condition.Less;
                    break;
                case "<=":
                    Condition = Condition.LessOrEquals;
                    break;
                case ">":
                    Condition = Condition.Greater;
                    break;
                case ">=":
                    Condition = Condition.GreaterOrEquals;
                    break;
                case "<>":
                    Condition = Condition.NotEquals;
                    break;
                default:
                    if (string.Equals(condition, "in", StringComparison.OrdinalIgnoreCase))
                        Condition = Condition.In;
                    else if (string.Equals(condition, "under", StringComparison.OrdinalIgnoreCase))
                        Condition = Condition.Under;
                    else
                        Condition = Condition.None;
                    break;
            }
        }

        public override NodeType NodeType { get { return NodeType.Condition; } }

        public Node Left { get; set; }

        public Condition Condition { get; set; }

        public Node Right { get; set; }

        public override string ToString()
        {
            string val;
            switch (Condition)
            {
                case Condition.Equals:
                    val = "=";
                    break;
                case Condition.Greater:
                    val = ">";
                    break;
                case Condition.GreaterOrEquals:
                    val = ">=";
                    break;
                case Condition.In:
                    val = "in";
                    break;
                case Condition.Less:
                    val = "<";
                    break;
                case Condition.LessOrEquals:
                    val = "<=";
                    break;
                case Condition.NotEquals:
                    val = "<>";
                    break;
                case Condition.Under:
                    val = "Under";
                    break;
                default:
                    val = "NONE";
                    break;
            }
            if (Left != null && Right != null)
                return Left + " " + val + " " + Right;
            return val;
        }
    }

    enum ValueDataType
    {
        String,
        Number,
        DateTime
    }

    class ConstantNode : Node
    {
        public ConstantNode(string value)
        {
            if (value.StartsWith("'", StringComparison.Ordinal))
            {
                Value = value.Trim('\'');
                DataType = ValueDataType.String;
            }
            else
            {
                if (value.IndexOf(".", StringComparison.Ordinal) > -1)
                {
                    Value = Convert.ToDouble(value);
                }
                else
                {
                    Value = Convert.ToInt64(value);
                }
                DataType = ValueDataType.Number;
            }
        }
        //        public ConstantNode(object value, ValueDataType type)
        //        {
        //            this.Value = value;
        //            this.DataType = type;
        //        }
        public ValueDataType DataType { get; set; }

        public override NodeType NodeType { get { return NodeType.Constant; } }

        public object Value { get; set; }

        public override string ToString()
        {
            if (DataType == ValueDataType.String)
                return "'" + Value + "'";
            return Convert.ToString(Value);
        }
    }

    class ParameterNode : Node
    {
        public ParameterNode(string parameter)
        {
            this.ParameterName = parameter.TrimStart('@');
        }

        public override NodeType NodeType { get { return NodeType.Parameter; } }

        public string ParameterName { get; set; }

        public override string ToString()
        {
            return "@" + ParameterName;
        }
    }

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

    class OperatorNode : Node
    {
        public OperatorNode(string word)
        {
            switch (word.ToLowerInvariant())
            {
                case "and":
                    Operator = Operator.And;
                    break;
                case "or":
                    Operator = Operator.Or;
                    break;
            }
        }

        public override NodeType NodeType { get { return NodeType.Operator; } }

        public Operator Operator { get; set; }

        public override string ToString()
        {
            return Operator.ToString();
        }
    }

    class OpenBracketNode : Node
    {
        public override NodeType NodeType { get { return NodeType.OpenBracket; } }

        public override string ToString()
        {
            return "(";
        }
    }

    class CloseBracketNode : Node
    {
        public override NodeType NodeType { get { return NodeType.CloseBracket; } }

        public override string ToString()
        {
            return ")";
        }
    }
}

