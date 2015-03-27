//
// LexalParserTests.cs
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
using Microsoft.TeamFoundation.WorkItemTracking.Client.Query;
using System.Xml.Linq;
using Microsoft.TeamFoundation.WorkItemTracking.Client.Query.Where;
using Xunit;

namespace Tests
{
    public class LexalParserTests
    {
        [Fact]
        public void Parse1()
        {
            XElement el = XElement.Parse(@"<f>SELECT [System.Id], [System.WorkItemType], [System.AssignedTo], [System.CreatedBy], [Microsoft.VSTS.Common.Priority], [System.Title], [System.Description] 
FROM WorkItems 
WHERE 
    [System.TeamProject] = @project 
AND [System.State] &lt;&gt; 'Closed' 
AND [Microsoft.VSTS.Common.Issue] = 'Yes' 
ORDER BY [Microsoft.VSTS.Common.Priority], [System.Id]</f>");
            var parser = new LexalParser(el.Value);
            var nodes = parser.ProcessWherePart();

            Assert.True(nodes[0].NodeType == NodeType.Field);
            Assert.True(string.Equals(((FieldNode)nodes[0]).Field, "System.TeamProject"));

            Assert.True(nodes[1].NodeType == NodeType.Condition);
            Assert.True(((ConditionNode)nodes[1]).Condition == Condition.Equals);

            Assert.True(nodes[2].NodeType == NodeType.Parameter);
            Assert.True(string.Equals(((ParameterNode)nodes[2]).ParameterName, "project"));

            Assert.True(nodes[3].NodeType == NodeType.Operator);
            Assert.True(((OperatorNode)nodes[3]).Operator == Operator.And);

            Assert.True(nodes[4].NodeType == NodeType.Field);
            Assert.True(nodes[5].NodeType == NodeType.Condition);
            Assert.True(nodes[6].NodeType == NodeType.Constant);
            Assert.True(nodes[7].NodeType == NodeType.Operator);
            Assert.True(nodes[8].NodeType == NodeType.Field);
            Assert.True(nodes[9].NodeType == NodeType.Condition);
            Assert.True(nodes[10].NodeType == NodeType.Constant);
        }

        [Fact]
        public void Parse2()
        {
            XElement el = XElement.Parse(@"<f>select [System.Id], [Phoenix.DueDate], [Phoenix.MagicDueDate], [System.WorkItemType], [System.State], [System.Title], [System.IterationPath] 
from WorkItems 
where ([System.State] = 'New' 
    or [System.State] = 'Active') 
    and ([System.AssignedTo] = 'Ventsislav Mladenov' or [System.TeamProject] = @project) 
order by [System.Id]</f>");
            var parser = new LexalParser(el.Value);
            var nodes = parser.ProcessWherePart();
            Assert.True(nodes[0].NodeType == NodeType.OpenBracket);

            Assert.True(nodes[1].NodeType == NodeType.Field);
            Assert.True(nodes[2].NodeType == NodeType.Condition);
            Assert.True(nodes[3].NodeType == NodeType.Constant);
            Assert.True(nodes[4].NodeType == NodeType.Operator);
            Assert.True(nodes[5].NodeType == NodeType.Field);
            Assert.True(nodes[6].NodeType == NodeType.Condition);
            Assert.True(nodes[7].NodeType == NodeType.Constant);
            Assert.True(nodes[8].NodeType == NodeType.CloseBracket);

            Assert.True(nodes[9].NodeType == NodeType.Operator);

            Assert.True(nodes[10].NodeType == NodeType.OpenBracket);
            Assert.True(nodes[11].NodeType == NodeType.Field);
            Assert.True(nodes[12].NodeType == NodeType.Condition);
            Assert.True(nodes[13].NodeType == NodeType.Constant);
            Assert.True(nodes[14].NodeType == NodeType.Operator);
            Assert.True(nodes[15].NodeType == NodeType.Field);
            Assert.True(nodes[16].NodeType == NodeType.Condition);
            Assert.True(nodes[17].NodeType == NodeType.Parameter);
            Assert.True(nodes[18].NodeType == NodeType.CloseBracket);
        }

        [Fact]
        public void SelectParse1()
        {
            XElement el = XElement.Parse(@"<f>select [System.Id], [Phoenix.DueDate], [Phoenix.MagicDueDate], [System.WorkItemType], [System.State], [System.Title], [System.IterationPath] 
from WorkItems 
where ([System.State] = 'New' 
    or [System.State] = 'Active') 
    and ([System.AssignedTo] = 'Ventsislav Mladenov' or [System.TeamProject] = @project) 
order by [System.Id]</f>");
            var parser = new LexalParser(el.Value);
            var nodes = parser.ProcessSelect();
            Console.WriteLine(nodes);
            Assert.Equal("[System.Id], [Phoenix.DueDate], [Phoenix.MagicDueDate], [System.WorkItemType], [System.State], [System.Title], [System.IterationPath]", nodes.ToString());
        }

        [Fact]
        public void OrderByParse1()
        {
            XElement el = XElement.Parse(@"<f>select [System.Id], [Phoenix.DueDate], [Phoenix.MagicDueDate], [System.WorkItemType], [System.State], [System.Title], [System.IterationPath] 
from WorkItems 
where ([System.State] = 'New' 
    or [System.State] = 'Active') 
    and ([System.AssignedTo] = 'Ventsislav Mladenov' or [System.TeamProject] = @project) 
order by [System.Id]</f>");
            var parser = new LexalParser(el.Value);
            var nodes = parser.ProcessOrderBy();
            Console.WriteLine(nodes);
            Assert.Equal("[System.Id] Asc", nodes.ToString());
        }

        [Fact]
        public void OrderByParse2()
        {
            XElement el = XElement.Parse(@"<f>SELECT [System.Id], [System.WorkItemType], [System.AssignedTo], [System.CreatedBy], [Microsoft.VSTS.Common.Priority], [System.Title], [System.Description] 
FROM WorkItems 
WHERE 
    [System.TeamProject] = @project 
AND [System.State] &lt;&gt; 'Closed' 
AND [Microsoft.VSTS.Common.Issue] = 'Yes' 
ORDER BY [Microsoft.VSTS.Common.Priority], [System.Id] desc</f>");
            var parser = new LexalParser(el.Value);
            var nodes = parser.ProcessOrderBy();
            Console.WriteLine(nodes);
            Assert.Equal("[Microsoft.VSTS.Common.Priority] Asc, [System.Id] Desc", nodes.ToString());
        }
    }
}

