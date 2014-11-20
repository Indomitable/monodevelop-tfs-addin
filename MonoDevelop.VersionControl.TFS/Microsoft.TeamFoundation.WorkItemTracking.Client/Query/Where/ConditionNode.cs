//
// ConditionNode.cs
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

namespace Microsoft.TeamFoundation.WorkItemTracking.Client.Query.Where
{
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
                    else if (string.Equals(condition, "IN GROUP", StringComparison.OrdinalIgnoreCase))
                        Condition = Condition.InGroup;
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
                case Condition.InGroup:
                    val = "IN GROUP";
                    break;
                default:
                    val = "NONE";
                    break;
            }
            if (Left != null && Right != null)
                return Left + " " + val + " " + Right;
            return val;
        }

        public string ToOperator()
        {

//            public static String EQUALS = "equals";
//            public static String NOT_EQUALS = "notEquals";
//            public static String GREATER = "greater";
//            public static String LESS = "less";
//            public static String EQUALS_GREATER = "equalsGreater";
//            public static String EQUALS_LESS = "equalsLess";
//            public static String CONTAINS = "contains";
//            public static String NOT_CONTAINS = "notContains";
//            public static String EVER = "ever";
//            public static String NOT_EVER = "notEver";
//            public static String UNDER = "under";
//            public static String NOT_UNDER = "notUnder";
            switch (Condition)
            {
                case Condition.Equals:
                    return "equals";
                case Condition.Greater:
                    return "greater";
                case Condition.GreaterOrEquals:
                    return "equalsGreater";
                case Condition.In:
                    return "in";
                case Condition.Less:
                    return "less";
                case Condition.LessOrEquals:
                    return "equalsLess";
                case Condition.NotEquals:
                    return "notEquals";
                case Condition.Under:
                    return "under";
                case Condition.InGroup:
                    return "equals";
                default:
                    return string.Empty;
            }
        }
    }
}
