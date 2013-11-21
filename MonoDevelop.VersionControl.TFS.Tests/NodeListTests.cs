//
// NodeListTests.cs
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
using Microsoft.TeamFoundation.WorkItemTracking.Client.Query;

namespace MonoDevelop.VersionControl.TFS.Tests
{
    [TestFixtureAttribute]
    public class NodeListTests
    {
        [Test]
        public void RemoveUnUsedBrackets1()
        {
            string val = "where (([a] = 2))";
            var parser = new LexalParser(val);
            var nodes = parser.Process();
            nodes.Optimize();
            Console.WriteLine(nodes);
            Assert.AreEqual("[a] = 2", nodes.ToString());
        }

        [Test]
        public void RemoveUnUsedBrackets2()
        {
            string val = "where (([a] = 2) and [b] = @p)";
            var parser = new LexalParser(val);
            var nodes = parser.Process();
            nodes.Optimize();
            Console.WriteLine(nodes);
            Assert.AreEqual("( [a] = 2 ) And [b] = @p", nodes.ToString());
        }

        [Test]
        public void RemoveUnUsedBrackets3()
        {
            string val = "where ([a] = 2) and ([b] = @p)";
            var parser = new LexalParser(val);
            var nodes = parser.Process();
            nodes.Optimize();
            Console.WriteLine(nodes);
            Assert.AreEqual("( [a] = 2 ) And ( [b] = @p )", nodes.ToString());
        }

        [Test]
        public void RemoveUnUsedBrackets4()
        {
            string val = "where (([a] = 2) and (([b] = @p)))";
            var parser = new LexalParser(val);
            var nodes = parser.Process();
            nodes.Optimize();
            Console.WriteLine(nodes);
            Assert.AreEqual("( [a] = 2 ) And ( ( [b] = @p ) )", nodes.ToString());
        }

        [Test]
        public void GetSubList1()
        {
            string val = "where ([a] = 2) and ([b] = @p)";
            var parser = new LexalParser(val);
            var nodes = parser.Process();
            var list = nodes.GetSubList(0);
            Console.WriteLine(list);
            Assert.AreEqual("[a] = 2", list.ToString());
        }

        [Test]
        public void GetSubList2()
        {
            string val = "where (([a] = 2) and [b] = @p)";
            var parser = new LexalParser(val);
            var nodes = parser.Process();
            var list = nodes.GetSubList(0);
            Console.WriteLine(list);
            Assert.AreEqual("( [a] = 2 ) And [b] = @p", list.ToString());
        }
        //        [Test]
        //        public void CreateHelperGroups1()
        //        {
        //            string val = "where [a] = 2 and [b] = 3 and [c] = 4 or [d] = 5";
        //            var parser = new LexalParser(val);
        //            var nodes = parser.Process();
        //            nodes.Optimize();
        //            nodes.CreateHelperGroups();
        //            Console.WriteLine(nodes);
        //            Assert.AreEqual("( [a] = 2 And [b] = 3 And [c] = 4 ) Or [d] = 5", nodes.ToString());
        //        }
        //
        //        [Test]
        //        public void CreateHelperGroups2()
        //        {
        //            string val = "where [f] = 2 and [a] = 2 or [s] = 1 or [b] = 3 or [x] = 1 and [c] = 4 or [d] = 5";
        //            var parser = new LexalParser(val);
        //            var nodes = parser.Process();
        //            nodes.Optimize();
        //            nodes.CreateHelperGroups();
        //            Console.WriteLine(nodes);
        //            Assert.AreEqual("( [f] = 2 And [a] = 2 ) Or ( [s] = 1 Or [b] = 3 Or [x] = 1 ) And ( [c] = 4 Or [d] = 5 )", nodes.ToString());
        //        }
        //
        //        [Test]
        //        public void CreateHelperGroups3()
        //        {
        //            string val = "where ([f] = 2 and [a] = 2 or [s] = 1) and [b] = 3";
        //            var parser = new LexalParser(val);
        //            var nodes = parser.Process();
        //            nodes.Optimize();
        //            nodes.CreateHelperGroups();
        //            Console.WriteLine(nodes);
        //            Assert.AreEqual("( ( [f] = 2 And [a] = 2 ) Or [s] = 1 ) And [b] = 3", nodes.ToString());
        //        }
        //
        //        [Test]
        //        public void CreateHelperGroups4()
        //        {
        //            string val = "where ([f] = 2 and [a] = 2 or [s] = 1) and ([b] = 3 or [x] = 1)";
        //            var parser = new LexalParser(val);
        //            var nodes = parser.Process();
        //            nodes.Optimize();
        //            nodes.CreateHelperGroups();
        //            Console.WriteLine(nodes);
        //            Assert.AreEqual("( ( [f] = 2 And [a] = 2 ) Or [s] = 1 ) And ( [b] = 3 Or [x] = 1 )", nodes.ToString());
        //        }
        //
        //        [Test]
        //        public void CreateHelperGroups5()
        //        {
        //            string val = "where ([f] = 2 and [a] = 2 or [s] = 1) and ([b] = 3 or [x] = 1 and [s] = 3)";
        //            var parser = new LexalParser(val);
        //            var nodes = parser.Process();
        //            nodes.Optimize();
        //            nodes.CreateHelperGroups();
        //            Console.WriteLine(nodes);
        //            Assert.AreEqual("( ( [f] = 2 And [a] = 2 ) Or [s] = 1 ) And ( ( [b] = 3 Or [x] = 1 ) And [s] = 3  )", nodes.ToString());
        //        }
        [Test]
        public void ExtractOperatorForward1()
        {
            string val = "where [a] = 2 and [b] = 3 and [c] = 4 or [d] = 5";
            var parser = new LexalParser(val);
            var nodes = parser.Process();
            nodes.Optimize();
            nodes.ExtractOperatorForward();
            Console.WriteLine(nodes);
            Assert.AreEqual("Or ( And [a] = 2 [b] = 3 [c] = 4 ) [d] = 5", nodes.ToString());
        }

        [Test]
        public void ExtractOperatorForward2()
        {
            string val = "where ([f] = 2 and [a] = 2 or [s] = 1) and [b] = 3";
            var parser = new LexalParser(val);
            var nodes = parser.Process();
            nodes.Optimize();
            nodes.ExtractOperatorForward();
            Console.WriteLine(nodes);
            Assert.AreEqual("And ( Or ( And [f] = 2 [a] = 2 ) [s] = 1 ) [b] = 3", nodes.ToString());
        }

        [Test]
        public void ExtractOperatorForward3()
        {
            string val = "where ([f] = 2 and [a] = 2 or [s] = 1) and ([b] = 3 or [x] = 1 and [s] = 3)";
            var parser = new LexalParser(val);
            var nodes = parser.Process();
            nodes.Optimize();
            nodes.ExtractOperatorForward();
            Console.WriteLine(nodes);
            Assert.AreEqual("And ( Or ( And [f] = 2 [a] = 2 ) [s] = 1 ) ( And ( Or [b] = 3 [x] = 1 ) [s] = 3 )", nodes.ToString());
        }

        [Test]
        public void ExtractOperatorForward4()
        {
            string val = "where (([f] = 2 and [a] = 2) or [s] = 1) and ([b] = 3 or ([x] = 1 and [s] = 3))";
            var parser = new LexalParser(val);
            var nodes = parser.Process();
            nodes.Optimize();
            nodes.ExtractOperatorForward();
            Console.WriteLine(nodes);
            Assert.AreEqual("And ( Or ( And [f] = 2 [a] = 2 ) [s] = 1 ) ( Or [b] = 3 ( And [x] = 1 [s] = 3 ) )", nodes.ToString());
        }

        [Test]
        public void ExtractOperatorForward5()
        {
            string val = "where ([f] = 2 and [a] = 2) or ([b] = 3 and [x] = 1)";
            var parser = new LexalParser(val);
            var nodes = parser.Process();
            nodes.Optimize();
            nodes.ExtractOperatorForward();
            Console.WriteLine(nodes);
            Assert.AreEqual("Or ( And [f] = 2 [a] = 2 ) ( And [b] = 3 [x] = 1 )", nodes.ToString());
        }

        [Test]
        public void ExtractOperatorForward6()
        {
            string val = "where [f] = 2";
            var parser = new LexalParser(val);
            var nodes = parser.Process();
            nodes.Optimize();
            nodes.ExtractOperatorForward();
            Console.WriteLine(nodes);
            Assert.AreEqual("[f] = 2", nodes.ToString());
        }

        [Test]
        public void NodesToXml1()
        {
            string val = "where [f] = 2";
            var parser = new LexalParser(val);
            var nodes = parser.Process();
            nodes.Optimize();
            nodes.ExtractOperatorForward();
            var xmlTransformer = new NodesToXml(nodes);
            var output = xmlTransformer.WriteXml();
            Console.WriteLine(output);
            Assert.AreEqual(@"<?xml version=""1.0"" encoding=""utf-16""?>
<Expression Column=""f"" FieldType="""" Operator=""equals"">
  <Number>2</Number>
</Expression>", output);
        }

        [Test]
        public void NodesToXml2()
        {
            string val = "where ([f] = 2 and [a] = 2) or ([b] = 3 and [x] = 1)";
            var parser = new LexalParser(val);
            var nodes = parser.Process();
            nodes.Optimize();
            nodes.ExtractOperatorForward();
            var xmlTransformer = new NodesToXml(nodes);
            var output = xmlTransformer.WriteXml();
            Console.WriteLine(output);
            Assert.AreEqual(@"<?xml version=""1.0"" encoding=""utf-16""?>
<Group GroupOperator=""Or"">
  <Group GroupOperator=""And"">
    <Expression Column=""f"" FieldType="""" Operator=""equals"">
      <Number>2</Number>
    </Expression>
    <Expression Column=""a"" FieldType="""" Operator=""equals"">
      <Number>2</Number>
    </Expression>
  </Group>
  <Group GroupOperator=""And"">
    <Expression Column=""b"" FieldType="""" Operator=""equals"">
      <Number>3</Number>
    </Expression>
    <Expression Column=""x"" FieldType="""" Operator=""equals"">
      <Number>1</Number>
    </Expression>
  </Group>
</Group>", output);
        }

        [Test]
        public void NodesToXml3()
        {
            string val = "where (([f] = 2 and [a] = 2) or [s] = 1) and ([b] = 3 or ([x] = 1 and [s] = 3))";
            var parser = new LexalParser(val);
            var nodes = parser.Process();
            nodes.Optimize();
            nodes.ExtractOperatorForward();
            var xmlTransformer = new NodesToXml(nodes);
            var output = xmlTransformer.WriteXml();
            Console.WriteLine(output);
        }
    }
}

