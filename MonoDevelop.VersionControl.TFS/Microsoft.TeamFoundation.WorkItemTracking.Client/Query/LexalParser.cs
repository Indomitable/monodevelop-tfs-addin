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
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.WorkItemTracking.Client.Query.Where;

namespace Microsoft.TeamFoundation.WorkItemTracking.Client.Query
{
    public class LexalParser
    {
        private enum CursorState
        {
            None,
            FieldName,
            Condition,
            Constant,
            Parameter,
            ArrayOfValues,
            Operator,
            OpenBracket,
            CloseBracket,
        }

        const char StringDelimeter = '\'';
        const char ParameterStart = '@';
        const char FieldStart = '[';
        const char FieldEnd = ']';
        const char OpenBracket = '(';
        const char CloseBracket = ')';
        const char NullChar = '\0';
        readonly static char[] Brackets = { OpenBracket, CloseBracket };
        readonly static char[] NumberChars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.' };
        readonly string whereClause = string.Empty;
        readonly string selectClause = string.Empty;
        readonly string orderByClause = string.Empty;
        CursorState currentState = CursorState.None;
        readonly NodeList nodes = new NodeList();
        const string SelectKeyWord = "select";
        const string WhereKeyWord = "where";
        const string OrderByKeyWord = "order by";
        const string FromWord = "from";

        public LexalParser(string query)
        {
            int whereIndex = query.IndexOf(WhereKeyWord, StringComparison.OrdinalIgnoreCase);
            int selectIndex = query.IndexOf(SelectKeyWord, StringComparison.OrdinalIgnoreCase);
            int orderByIndex = query.LastIndexOf(OrderByKeyWord, StringComparison.OrdinalIgnoreCase);
            int fromIndex = query.IndexOf(FromWord, StringComparison.OrdinalIgnoreCase);

            if (selectIndex > -1) //Should have but for tests.
                this.selectClause = query.Substring(selectIndex + SelectKeyWord.Length, fromIndex - selectIndex - SelectKeyWord.Length);
            else
                this.selectClause = string.Empty;

            if (orderByIndex > -1)
            {
                this.whereClause = query.Substring(whereIndex + WhereKeyWord.Length, orderByIndex - whereIndex - WhereKeyWord.Length);
                this.orderByClause = query.Substring(orderByIndex + OrderByKeyWord.Length);
            }
            else
            {
                this.whereClause = query.Substring(whereIndex + WhereKeyWord.Length);
            }

            this.selectClause = this.selectClause.Trim();
            this.whereClause = this.whereClause.Trim();
            this.orderByClause = this.orderByClause.Trim();

        }

        private Node SetState(CursorState state, string word)
        {
            Node node = null;
            if (state != currentState)
            {
                if (state == CursorState.FieldName)
                {
                    node = new FieldNode(word); 
                }
                if (state == CursorState.Condition)
                {
                    node = new ConditionNode(word);
                }
                if (state == CursorState.Parameter)
                {
                    node = new ParameterNode(word);
                }
                if (state == CursorState.Constant)
                {
                    node = new ConstantNode(word);
                }
                if (state == CursorState.ArrayOfValues)
                {
                    node = new ArrayOfValues(word);
                }
                if (state == CursorState.OpenBracket)
                {
                    node = new OpenBracketNode();
                }
                if (state == CursorState.CloseBracket)
                {
                    node = new CloseBracketNode();
                }
                if (state == CursorState.Operator)
                {
                    node = new OperatorNode(word);
                }
                if (node != null)
                    nodes.Add(node);
                currentState = state;
            }
            return node;
        }

        private bool InState(params CursorState[] states)
        {
            return states.Any(s => s == currentState);
        }

        private bool IsWordOperator(string word)
        {
            return string.Equals(word, "and", StringComparison.OrdinalIgnoreCase) || string.Equals(word, "or", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsNextWordOperator(int i)
        {
            var word = GetNextWord(i);
            return IsWordOperator(word.Item1);
        }

        private string ReadOperator(ref int i)
        {
            StringBuilder builder = new StringBuilder();
            for (int j = i; j < whereClause.Length; j++)
            {
                i = j;
                //After operator should be only empty space or open bracet "and(..", " and " 
                if ((char.IsWhiteSpace(whereClause[j]) || whereClause[i] == OpenBracket) && builder.ToString().Trim().Length > 0)
                {
                    break;
                }
                builder.Append(whereClause[j]);
            }
            var @operator = builder.ToString().Trim();
            if (whereClause[i] == OpenBracket)
                i--;
            return @operator;
        }

        private void MoveToSymbol(ref int i, int symbol)
        {
            for (int j = i; j < whereClause.Length; j++)
            {
                i = j;
                if (whereClause[j] == symbol)
                    break;
            }
        }

        private void ReadOpenBracket(ref int i)
        {
            MoveToSymbol(ref i, OpenBracket);
        }

        private void ReadCloseBracket(ref int i)
        {
            MoveToSymbol(ref i, CloseBracket);
        }

        private char MoveToNextNonWhiteSpace(ref int i)
        {
            for (int j = i; j < whereClause.Length; j++)
            {
                if (char.IsWhiteSpace(whereClause[j]))
                    continue;
                else
                {
                    i = j;
                    return whereClause[j];
                }
            }
            return NullChar;
        }

        private bool IsNextSymbol(int i, char symbol)
        {
            var nextSymbol = MoveToNextNonWhiteSpace(ref i);
            return nextSymbol == symbol;
        }

        private string ReadParameter(ref int i)
        {
            StringBuilder builder = new StringBuilder();
            if (whereClause[i] != ParameterStart)
                throw new Exception("Invalid Parameter");
            for (int j = i; j < whereClause.Length; j++)
            {
                i = j;
                if ((char.IsWhiteSpace(whereClause[j]) && IsNextWordOperator(j)) || IsNextSymbol(j, CloseBracket))
                {
                    break;
                }
                builder.Append(whereClause[j]);
            }
            return builder.ToString();
        }

        private string ReadStringConstant(ref int i)
        {
            StringBuilder builder = new StringBuilder();
            for (int j = i + 1; j < whereClause.Length; j++)
            {
                builder.Append(whereClause[j]);
                i = j;
                if (whereClause[j] == StringDelimeter)
                    break;
            }
            return StringDelimeter + builder.ToString();
        }

        private string ReadNumberConstant(ref int i)
        {
            StringBuilder builder = new StringBuilder();
            for (int j = i; j < whereClause.Length; j++)
            {
                i = j;
                if (char.IsWhiteSpace(whereClause[j]) && builder.ToString().Trim().Length == 0)
                {
                    continue;
                }
                if (!NumberChars.Contains(whereClause[j]))
                {
                    break;
                }
                builder.Append(whereClause[j]);
            }
            return builder.ToString();
//            var word = GetNextWord(i);
//            i = word.Item2;
//            return word.Item1;
        }

        private string ReadConstant(ref int i)
        {
            if (whereClause[i] == StringDelimeter)
                return ReadStringConstant(ref i);
            else
                return ReadNumberConstant(ref i);
        }

        private string ReadArrayOfValues(ref int i)
        {
            var ch = MoveToNextNonWhiteSpace(ref i);
            if (ch != OpenBracket)
                throw new Exception("Invalid Array Of Values");
            var copyI = i;
            MoveToSymbol(ref i, CloseBracket);
            return whereClause.Substring(copyI, i - copyI + 1).Trim(Brackets);
        }

        private string ReadField(ref int i)
        {
            StringBuilder builder = new StringBuilder();
            if (whereClause[i] != FieldStart)
                throw new Exception("Invalid Field");
            for (int j = i; j < whereClause.Length; j++)
            {
                i = j;
                builder.Append(whereClause[j]);
                if (whereClause[j] == FieldEnd)
                {
                    break;
                }
            }
            return builder.ToString();
        }

        private Tuple<string, int> GetNextWord(int i)
        {
            StringBuilder builder = new StringBuilder();
            var k = i;
            for (int j = i; j < whereClause.Length; j++)
            {
                builder.Append(whereClause[j]);
                k = j;
                if ((char.IsWhiteSpace(whereClause[j]) || Brackets.Contains(whereClause[j])) && builder.ToString().Trim().Length > 0)
                {
                    break;
                }
            }
            return new Tuple<string, int>(builder.ToString().Trim().Trim(Brackets), k + 1);
        }

        private string ReadCondition(ref int i)
        {
            var word = GetNextWord(i);
            if (string.Equals(word.Item1, "IN", StringComparison.OrdinalIgnoreCase))
            {
                //Check for condition IN Group
                var nextWord = GetNextWord(word.Item2);
                if (string.Equals(nextWord.Item1, "GROUP", StringComparison.OrdinalIgnoreCase))
                {
                    i = nextWord.Item2;
                    return word.Item1 + " " + nextWord.Item1;
                }
                else
                {
                    i = word.Item2;
                    return word.Item1;
                }
            }

            if (string.Equals(word.Item1, "under", StringComparison.OrdinalIgnoreCase))
            {
                i = word.Item2;
                return word.Item1;
            }

            char[] conditionChars = { '=', '>', '<' };
            StringBuilder builder = new StringBuilder();
            for (int j = i; j < whereClause.Length; j++)
            {
                i = j;
                if (char.IsWhiteSpace(whereClause[j]) && builder.ToString().Trim().Length == 0)
                    continue;
                if (!conditionChars.Contains(whereClause[j]))
                {
                    break;
                }
                builder.Append(whereClause[j]);
            }
            return builder.ToString().Trim();
        }

        private string ReadValue(ref int i)
        {
            var startChar = MoveToNextNonWhiteSpace(ref i);
            string result = string.Empty;
            if (startChar == ParameterStart)
                result = ReadParameter(ref i);
            else if (startChar == StringDelimeter)
                result = ReadStringConstant(ref i);
            else
                result = ReadNumberConstant(ref i);
            if (whereClause[i] == CloseBracket) //If After Value is Close bracket with no space. 
                i--;
            return result;
        }

        internal NodeList ProcessWherePart()
        {
            for (int i = 0; i < whereClause.Length; i++)
            {
                char currentChar = whereClause[i];

                if (char.IsWhiteSpace(currentChar) && currentState == CursorState.None)
                {
                    SetState(CursorState.None, string.Empty);
                    continue;
                }

                if (currentChar == FieldStart && currentState == CursorState.None)
                {
                    //Read Field
                    var field = ReadField(ref i);
                    SetState(CursorState.FieldName, field);
                    i++; //Skip FieldEnd.
                    //Read Condition
                    var condition = ReadCondition(ref i);
                    var conditionNode = SetState(CursorState.Condition, condition);
                    //Read Value
                    switch (((ConditionNode)conditionNode).Condition)
                    {
                        case Condition.In:
                            var avalue = ReadArrayOfValues(ref i);
                            SetState(CursorState.ArrayOfValues, avalue);
                            break;
                        default:
                            var value = ReadValue(ref i);
                            if (value[0] == ParameterStart)
                                SetState(CursorState.Parameter, value);
                            else
                                SetState(CursorState.Constant, value);
                            break;
                    }
                    //Continue
                    SetState(CursorState.None, string.Empty);
                    continue;
                }

                if (currentState == CursorState.None && IsNextWordOperator(i))
                {
                    var @operator = ReadOperator(ref i);
                    SetState(CursorState.Operator, @operator);
                    SetState(CursorState.None, string.Empty);
                    continue;
                }

                if (currentState == CursorState.None && IsNextSymbol(i, OpenBracket))
                {
                    ReadOpenBracket(ref i);
                    SetState(CursorState.OpenBracket, null);
                    SetState(CursorState.None, string.Empty);
                    continue;
                }

                if (currentState == CursorState.None && IsNextSymbol(i, CloseBracket))
                {
                    ReadCloseBracket(ref i);
                    SetState(CursorState.CloseBracket, null);
                    SetState(CursorState.None, string.Empty);
                    continue;
                }
            }

            AnalyzeNodes();
            return nodes;
        }

        internal SelectNodeList ProcessSelect()
        {
            var list = new SelectNodeList(); 
            if (!string.IsNullOrWhiteSpace(selectClause))
            {
                var columns = selectClause.Split(new [] { "," }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in columns)
                {
                    list.Add(new SelectNode(item));
                }
            }
            return list;
        }

        internal OrderByList ProcessOrderBy()
        {
            var list = new OrderByList();
            if (!string.IsNullOrWhiteSpace(orderByClause))
            {
                var orderByItems = orderByClause.Split(new [] { "," }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in orderByItems)
                { 
                    var tmpItem = item.Trim();
                    var parts = tmpItem.Split(new [] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 1)
                        list.Add(new OrderByNode(parts[0], Direction.Asc));
                    if (parts.Length == 2)
                    {
                        var direction = string.Equals(parts[1], "desc", StringComparison.OrdinalIgnoreCase) ? Direction.Desc : Direction.Asc;
                        list.Add(new OrderByNode(parts[0], direction));
                    }
                }
            }
            return list;
        }

        private void AnalyzeNodes()
        {
            var openBracketsCount = nodes.Count(x => x.NodeType == NodeType.OpenBracket);
            var closeBracketsCount = nodes.Count(x => x.NodeType == NodeType.CloseBracket);
            var exception = new Exception("Could not parse where clause correctly");
            if (openBracketsCount != closeBracketsCount)
                throw exception;
            for (int i = 0; i < nodes.Count; i++)
            {
                //The order should be field condition value operator field condition value ....
                if (nodes[i].NodeType == NodeType.Field)
                {
                    if (nodes[i + 1].NodeType != NodeType.Condition ||
                        !(nodes[i + 2].NodeType == NodeType.Constant || nodes[i + 2].NodeType == NodeType.Parameter || nodes[i + 2].NodeType == NodeType.ArrayOfValues))
                    {
                        throw exception;
                    }
                }
                //Next node should be operator or end, skip close brackets
                if (nodes[i].NodeType == NodeType.Constant || nodes[i].NodeType == NodeType.Parameter || nodes[i].NodeType == NodeType.ArrayOfValues)
                {
                    bool found = true;
                    for (int j = i + 1; j < nodes.Count; j++)
                    {
                        if (nodes[j].NodeType == NodeType.CloseBracket)
                            continue;
                        if (nodes[j].NodeType == NodeType.Operator)
                            break;
                        if (nodes[j].NodeType == NodeType.Field ||
                            nodes[j].NodeType == NodeType.Condition ||
                            nodes[j].NodeType == NodeType.Parameter ||
                            nodes[j].NodeType == NodeType.Constant ||
                            nodes[j].NodeType == NodeType.ArrayOfValues ||
                            nodes[j].NodeType == NodeType.OpenBracket)
                        {
                            found = false;
                            break;
                        }
                    }
                    if (!found)
                        throw exception;
                }
            }
        }
    }
}

