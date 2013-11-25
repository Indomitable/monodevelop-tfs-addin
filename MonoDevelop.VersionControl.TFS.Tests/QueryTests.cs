//
// QueryTests.cs
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
using NUnit.Framework;
using System.Xml.Linq;
using Microsoft.TeamFoundation.WorkItemTracking.Client.Query;
using System.Collections.Generic;
using Microsoft.TeamFoundation.WorkItemTracking.Client.Query.Where;

namespace MonoDevelop.VersionControl.TFS.Tests
{
    [TestFixture]
    public class QueryTests
    {
        [Test]
        public void Optimize1()
        {
            XElement el = XElement.Parse(@"<f>SELECT [System.Id], [System.WorkItemType], [Microsoft.VSTS.Common.Rank], [System.Title], [System.State], [System.AssignedTo], [Microsoft.VSTS.Common.RoughOrderOfMagnitude], [Microsoft.VSTS.Common.ExitCriteria], [System.Description] 
FROM WorkItems 
WHERE [System.TeamProject] = @project 
  AND [System.WorkItemType] = 'Scenario' 
  AND [System.State] = 'Active' 
ORDER BY [Microsoft.VSTS.Common.Rank], [System.State], [System.Id]</f>");
            var parser = new LexalParser(el.Value);
            var nodes = parser.ProcessWherePart();
            nodes.Optimize();
            Assert.IsTrue(nodes[0].NodeType == NodeType.Condition);
            Assert.IsTrue(((ConditionNode)nodes[0]).Right.NodeType == NodeType.Parameter);

            Assert.IsTrue(nodes[1].NodeType == NodeType.Operator);

            Assert.IsTrue(nodes[2].NodeType == NodeType.Condition);
            Assert.IsTrue(((ConditionNode)nodes[2]).Right.NodeType == NodeType.Constant);

            Assert.IsTrue(nodes[3].NodeType == NodeType.Operator);

            Assert.IsTrue(nodes[4].NodeType == NodeType.Condition);
            Assert.IsTrue(((ConditionNode)nodes[4]).Right.NodeType == NodeType.Constant);
        }

        [Test]
        public void RPN1()
        {
            XElement el = XElement.Parse(@"<f>select [System.Id], [Phoenix.DueDate], [Phoenix.MagicDueDate], [System.WorkItemType], [System.State], [System.Title], [System.IterationPath] 
from WorkItems 
where ([System.State] = 'New' 
    or [System.State] = 'Active') 
    and ([System.AssignedTo] = 'Ventsislav Mladenov' or [System.TeamProject] = @project) 
order by [System.Id]</f>");
            var parser = new LexalParser(el.Value);
            var nodes = parser.ProcessWherePart();
            nodes.Optimize();
            nodes.ExtractOperatorForward();
            var xmlTransformer = new NodesToXml(nodes);
            var result = xmlTransformer.WriteXml();
            Console.WriteLine(result);
            Assert.AreEqual("<Group GroupOperator=\"And\">\r\n  <Group GroupOperator=\"Or\">\r\n    <Expression Column=\"System.State\" FieldType=\"\" Operator=\"equals\">\r\n      <String>New</String>\r\n    </Expression>\r\n    <Expression Column=\"System.State\" FieldType=\"\" Operator=\"equals\">\r\n      <String>Active</String>\r\n    </Expression>\r\n  </Group>\r\n  <Group GroupOperator=\"Or\">\r\n    <Expression Column=\"System.AssignedTo\" FieldType=\"\" Operator=\"equals\">\r\n      <String>Ventsislav Mladenov</String>\r\n    </Expression>\r\n    <Expression Column=\"System.TeamProject\" FieldType=\"\" Operator=\"equals\">\r\n      <String>project</String>\r\n    </Expression>\r\n  </Group>\r\n</Group>", result);
        }

        [Test]
        public void RPN2()
        {
            string val = "where [Microsoft.VSTS.Common.ResolvedBy] = 'Ventsislav Mladenov' or [Microsoft.VSTS.Common.ClosedBy] = 'Ventsislav Mladenov'";
            var parser = new LexalParser(val);
            var nodes = parser.ProcessWherePart();
            nodes.Optimize();
            nodes.ExtractOperatorForward();
            var xmlTransformer = new NodesToXml(nodes);
            var result = xmlTransformer.WriteXml();
            Console.WriteLine(result);
        }

        [Test]
        public void RPN3()
        {
            string val = "where [Microsoft.VSTS.Common.ResolvedBy] = 'Ventsislav Mladenov' and [Microsoft.VSTS.CMMI.CalledBy] = 'Ventsislav Mladenov' or [Microsoft.VSTS.Common.ClosedBy] = 'Ventsislav Mladenov'";
            var parser = new LexalParser(val);
            var nodes = parser.ProcessWherePart();
            nodes.Optimize();
            nodes.ExtractOperatorForward();
            var xmlTransformer = new NodesToXml(nodes);
            var result = xmlTransformer.WriteXml();
            Console.WriteLine(result);
        }

        private void AssertNodesTypes(List<Node> nodes, params NodeType[] types)
        {
            for (int i = 0; i < types.Length; i++)
            {
                if (nodes[i].NodeType != types[i])
                    throw new AssertionException("Invalid Node Type");
            }
        }

        [Test]
        public void ParseTest1()
        {
            string value = "select * from a where ([a]=10  and [c]<30)and([d] <> 3 or [e] >@s )";
            var parser = new LexalParser(value);
            var nodes = parser.ProcessWherePart(); //or [h] in(10,20)
            AssertNodesTypes(nodes, NodeType.OpenBracket, NodeType.Field, NodeType.Condition, NodeType.Constant, NodeType.Operator,
                NodeType.Field, NodeType.Condition, NodeType.Constant, NodeType.CloseBracket, NodeType.Operator,
                NodeType.OpenBracket, NodeType.Field, NodeType.Condition, NodeType.Constant, NodeType.Operator, 
                NodeType.Field, NodeType.Condition, NodeType.Parameter);
        }

        [Test]
        public void ParseTest2()
        {
            string value = " where ( ([a] = '3' or [b]<@c) and [d]> 40 )";
            var parser = new LexalParser(value);
            var nodes = parser.ProcessWherePart(); //or [h] in(10,20)
            AssertNodesTypes(nodes, NodeType.OpenBracket, NodeType.OpenBracket, NodeType.Field, NodeType.Condition, NodeType.Constant,
                NodeType.Operator, NodeType.Field, NodeType.Condition, NodeType.Parameter, NodeType.CloseBracket, NodeType.Operator, 
                NodeType.Field, NodeType.Condition, NodeType.Constant, NodeType.CloseBracket);
        }

        [Test]
        public void ParseTest3()
        {
            string value = " where ([a] <> '')";
            var parser = new LexalParser(value);
            var nodes = parser.ProcessWherePart(); 
            AssertNodesTypes(nodes, NodeType.OpenBracket, NodeType.Field, NodeType.Condition, NodeType.Constant, NodeType.CloseBracket);
        }

        [Test]
        public void ParseTest4()
        {
            string value = " where [a] = @me";
            var parser = new LexalParser(value);
            var nodes = parser.ProcessWherePart(); 
            AssertNodesTypes(nodes, NodeType.Field, NodeType.Condition, NodeType.Parameter);
        }

        [Test]
        public void RPN4()
        {
            XElement el = XElement.Parse(@"<f>select [System.Id], [System.WorkItemType], [System.Title], [System.AssignedTo], [System.State] 
from WorkItems 
where [System.TeamProject] = @project 
and [System.WorkItemType] &lt;&gt; '' 
and ([System.ChangedDate] &lt; @today - 30 
  or [System.CreatedDate] &gt; @today - 7) 
and [Microsoft.VSTS.Common.ResolvedBy] = @me 
and [System.WorkItemType] in ('Bug', 'Task') 
and [System.State] &lt;&gt; '' 
order by [System.Id]</f>");

            var parser = new LexalParser(el.Value);
            var nodes = parser.ProcessWherePart();
            nodes.Optimize();
            nodes.ExtractOperatorForward();
            var xmlTransformer = new NodesToXml(nodes);
            var result = xmlTransformer.WriteXml();
            Console.WriteLine(result);
        }
    }
}