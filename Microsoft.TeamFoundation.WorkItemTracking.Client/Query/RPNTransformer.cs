//
// RPNTransformer.cs
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
    class RPNTransformer
    {
        private readonly Queue<QueueValue> _queue = new Queue<QueueValue>();
        private readonly Stack<QueueValue> _rpn = new Stack<QueueValue>();

        public RPNTransformer(List<Node> nodes)
        {
            foreach (var node in nodes)
            {
                OperandType type = OperandType.EndOfQueue;
                if (node.NodeType == NodeType.Operator)
                {
                    if (((OperatorNode)node).Operator == Operator.And)
                    {
                        type = OperandType.AND;
                    }
                    else
                    {
                        type = OperandType.OR;
                    }
                }
                if (node.NodeType == NodeType.StartGroup)
                    type = OperandType.OpenBracket;
                if (node.NodeType == NodeType.EndGroup)
                    type = OperandType.CloseBracket;
                if (node.NodeType == NodeType.Condition)
                    type = OperandType.Operand;

                _queue.Enqueue(new QueueValue(type, node));
            }
            _queue.Enqueue(new QueueValue(OperandType.EndOfQueue, null));
        }

        enum OperandType
        {
            Operand,
            AND,
            OR,
            OpenBracket,
            CloseBracket,
            EndOfQueue,
            EndOfStack
        }

        bool IsOp(OperandType type)
        {
            return type == OperandType.AND || type == OperandType.OR;
        }

        int Prec(OperandType type)
        {
            switch (type)
            {
                case OperandType.AND:
                    return 1;
                case OperandType.OR:
                    return 1;
                case OperandType.OpenBracket:
                    return 0;
                case OperandType.CloseBracket:
                    return 2;
                case OperandType.EndOfQueue:
                    return 3;
                case OperandType.EndOfStack:
                    return 0;
            }
            return -1;
        }

        class QueueValue
        {
            //
            // Fields
            //
            public OperandType Type;
            public Node Node;
            //
            // Constructors
            //
            public QueueValue(OperandType type, Node node)
            {
                this.Type = type;
                this.Node = node;
            }
        }

        public List<Node> ConvertToRPN()
        {
            QueueValue qV = null;
            bool flag = true;
            Stack<QueueValue> stack = new Stack<QueueValue>();
            stack.Push(new QueueValue(OperandType.EndOfStack, null));
            if (this._queue.Count > 0)
            {
                while (flag ? ((qV = this._queue.Dequeue()).Type != OperandType.EndOfQueue) : (qV.Type != OperandType.EndOfQueue))
                {
                    flag = true;
                    if (qV.Type == OperandType.Operand)
                    {
                        this._rpn.Push(qV);
                    }
                    else
                    {
                        if (IsOp(qV.Type))
                        {
                            if (Prec(stack.Peek().Type) < Prec(qV.Type))
                            {
                                stack.Push(qV);
                                continue;
                            }
                            if (Prec(stack.Peek().Type) >= Prec(qV.Type))
                            {
                                this._rpn.Push(stack.Pop());
                                flag = false;
                                continue;
                            }
                        }
                        if (qV.Type == OperandType.OpenBracket)
                        {
                            stack.Push(qV);
                        }
                        else
                        {
                            if (qV.Type == OperandType.CloseBracket)
                            {
                                while (stack.Peek().Type != OperandType.OpenBracket)
                                {
                                    this._rpn.Push(stack.Pop());
                                    if (stack.Peek().Type == OperandType.EndOfStack)
                                    {
                                        throw new Exception();
                                    }
                                }
                                if (stack.Peek().Type == OperandType.OpenBracket)
                                {
                                    stack.Pop();
                                }
                            }
                        }
                    }
                }
                while (stack.Peek().Type != OperandType.EndOfStack)
                {
                    this._rpn.Push(stack.Pop());
                }
            }

            List<Node> output = new List<Node>();
            while (_rpn.Count > 0)
            {
                var node = _rpn.Pop().Node;
                if (node != null)
                    output.Add(node);
            }
            return output;
        }
    }
}

