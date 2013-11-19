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

namespace Microsoft.TeamFoundation.WorkItemTracking.Client.Query
{
    //    public class Node
    //    {
    //        public string FieldName { get; set; }
    //
    //        public Condition Condition { get; set; }
    //
    //        public object Value { get; set; }
    //    }
    //
    //    public class ParemeteredNode
    //    {
    //        public string Parameter { get; set; }
    //    }
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
        //AND,OR
        Operator,
        //(
        StartGroup,
        //)
        EndGroup
    }

    class Node
    {
        public virtual NodeType NodeType { get { return NodeType.Undefined; } }
    }

    class FieldNode : Node
    {
        public FieldNode(string field)
        {
            this.Field = field.Trim('[', ']');
        }

        public override NodeType NodeType { get { return NodeType.Field; } }

        public string Field { get; set; }
    }

    class ConditionalNode : Node
    {
        public ConditionalNode(string condition)
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
                    Condition = Condition.None;
                    break;
            }
        }

        public override NodeType NodeType { get { return NodeType.Condition; } }

        public Node Left { get; set; }

        public Condition Condition { get; set; }

        public Node Right { get; set; }
    }

    class ConstantNode : Node
    {
        public ConstantNode(string value)
        {
            if (value.StartsWith("'", StringComparison.Ordinal))
            {
                Value = value.Trim('\'');
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
            }
        }

        public override NodeType NodeType { get { return NodeType.Constant; } }

        public object Value { get; set; }
    }

    class ParameterNode : Node
    {
        public ParameterNode(string parameter)
        {
            this.ParameterName = parameter.TrimStart('@');
        }

        public override NodeType NodeType { get { return NodeType.Parameter; } }

        public string ParameterName { get; set; }
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
    }

    class OpenBracketNode : Node
    {
        public override NodeType NodeType { get { return NodeType.StartGroup; } }
    }

    class CloseBracketNode : Node
    {
        public override NodeType NodeType { get { return NodeType.EndGroup; } }
    }
    //    class NodeGroup
    //    {
    //        public List<NodeGroup> Nodes { get; set; }
    //    }
    //
    //    class NodeOperatorGroup
    //    {
    //        public OperatorNode Operator { get; set; }
    //
    //        public List<ConditionalNode> Nodes { get; set; }
    //    }
}

