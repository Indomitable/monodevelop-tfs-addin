//
// ParameterManager.cs
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
using Microsoft.TeamFoundation.WorkItemTracking.Client.Query.Where;

namespace Microsoft.TeamFoundation.WorkItemTracking.Client.Query
{
    public class ParameterManager
    {
        readonly WorkItemContext context;

        public ParameterManager(WorkItemContext context)
        {
            this.context = context;
        }

        private ConstantNode CreateConstantFromParameter(ParameterNode parameterNode)
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

        internal void EvalParameters(NodeList list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].NodeType == NodeType.Condition)
                {
                    var condition = (ConditionNode)list[i];
                    if (condition.Right.NodeType == NodeType.Parameter)
                    {
                        var parameterNode = (ParameterNode)condition.Right;
                        condition.Right = CreateConstantFromParameter(parameterNode);
                    }
                }
            }
        }
    }
}

