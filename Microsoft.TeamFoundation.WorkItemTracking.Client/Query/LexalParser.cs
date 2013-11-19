//
// Parser.cs
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

namespace Microsoft.TeamFoundation.WorkItemTracking.Client.Query
{
    public class LexalParser
    {
        private enum CursorState
        {
            None,
            FieldName,
            Condition,
            StrValue,
            IntValue,
            Parameter,
            Operator,
            OpenBracket,
            CloseBracket,
        }

        readonly string whereClause;
        string word = string.Empty;
        CursorState currentState = CursorState.None;
        CursorState prevState = CursorState.None;
        readonly List<Node> nodes = new List<Node>();

        public LexalParser(string query)
        {
            this.whereClause = query.Substring(query.IndexOf("where", StringComparison.OrdinalIgnoreCase) + 5).Trim();
            if (this.whereClause.IndexOf("order by", StringComparison.OrdinalIgnoreCase) > -1)
            {
                this.whereClause = this.whereClause.Substring(0, this.whereClause.IndexOf("order by", StringComparison.OrdinalIgnoreCase));
            }
        }

        void SetState(CursorState state)
        {
            if (state != currentState)
            {
                if (currentState == CursorState.FieldName)
                {
                    nodes.Add(new FieldNode(word));
                }
                if (currentState == CursorState.Condition)
                {
                    nodes.Add(new ConditionalNode(word));
                }
                if (currentState == CursorState.Parameter)
                {
                    nodes.Add(new ParameterNode(word));
                }
                if (currentState == CursorState.IntValue || currentState == CursorState.StrValue)
                {
                    nodes.Add(new ConstantNode(word));
                }
                if (currentState == CursorState.OpenBracket)
                {
                    nodes.Add(new OpenBracketNode());
                }
                if (currentState == CursorState.CloseBracket)
                {
                    nodes.Add(new CloseBracketNode());
                }
                if (currentState == CursorState.Operator)
                {
                    nodes.Add(new OperatorNode(word));
                }
                prevState = currentState;
                currentState = state;
                word = string.Empty;
            }
        }

        private bool InState(params CursorState[] states)
        {
            return states.Any(s => s == currentState);
        }

        private CursorState[] SpaceAllowed()
        {
            return new [] { CursorState.StrValue };
        }

        private CursorState[] NoSpaceAllowed()
        {
            return new [] { CursorState.FieldName, CursorState.Condition, CursorState.IntValue, CursorState.Parameter, CursorState.OpenBracket, CursorState.CloseBracket };
        }

        internal List<Node> Process()
        {
            const int stringDelimeter = '\'';
            const int parameterStart = '@';
            const int fieldStart = '[';
            const int fieldEnd = ']';
            const int openBracket = '(';
            const int closedBracket = ')';
            for (int i = 0; i < whereClause.Length; i++)
            {
                char currentChar = whereClause[i];

                if (char.IsWhiteSpace(currentChar) && (string.Equals(word, "and", StringComparison.OrdinalIgnoreCase) || string.Equals(word, "or", StringComparison.OrdinalIgnoreCase)))
                {
                    string copyWord = word;
                    SetState(CursorState.Operator);
                    word = copyWord;
                    SetState(CursorState.None);
                    continue;
                }

                if (char.IsWhiteSpace(currentChar) && (InState(NoSpaceAllowed()) || currentState == CursorState.None))
                {
                    SetState(CursorState.None);
                    continue;
                }

                if (!char.IsWhiteSpace(currentChar))
                {
                    if (currentChar == fieldStart && currentState == CursorState.None)
                        SetState(CursorState.FieldName);

                    if (currentChar == fieldEnd && currentState == CursorState.FieldName)
                    {
                        SetState(CursorState.None);
                        continue;
                    }

                    if (currentState == CursorState.None && prevState == CursorState.FieldName)
                        SetState(CursorState.Condition);

                    if (currentState == CursorState.StrValue && currentChar == stringDelimeter)
                    {
                        SetState(CursorState.None);
                        continue;
                    }

                    if (currentState == CursorState.None && prevState == CursorState.Condition)
                    {
                        if (currentChar == parameterStart)
                            SetState(CursorState.Parameter);
                        else if (currentChar == stringDelimeter)
                            SetState(CursorState.StrValue);
                        else
                            SetState(CursorState.IntValue);
                    }

                    if (currentChar == openBracket && currentState != CursorState.StrValue)
                    {
                        SetState(CursorState.OpenBracket);
                        SetState(CursorState.None);
                        continue;
                    }
                    if (currentChar == closedBracket && currentState != CursorState.StrValue)
                    {
                        SetState(CursorState.CloseBracket);
                        SetState(CursorState.None);
                        continue;
                    }
                }
                word += currentChar;
            }

            return nodes;
        }
    }
}

