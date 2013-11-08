//
// Microsoft.TeamFoundation.VersionControl.Client.DifferenceUtil
//
// Authors:
//	Joel Reed (joelwreed@gmail.com)
//
// Copyright (C) 2007 Joel Reed
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.TeamFoundation.VersionControl.Common;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
	internal class DiffItemUtil
	{
		public string[] Lines = new string[0];
		public string name;
		public int Length;

		public string Name
		{
			get {
				return name;
			}
		}

		public DiffItemUtil(char prefix, string name, string fileContents)
		{
			Length = fileContents.Length;

			string path = PrefixedHeaderPath(prefix, name);
			if (Length == 0)
				{
					this.name = "/dev/null";
					return;
				}

			// gnu patch doesn't want to see backslashes in filenames
			this.name = path.Replace('\\', '/');

			// if file ends with /n the split below generates one extra row we dont want
			int len = fileContents.Length;
			if (fileContents.EndsWith("\n")) len -= 1;

			string x = fileContents.Substring(0, len);
			Lines = x.Split('\n');
		}

		private string PrefixedHeaderPath(char c, string path)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(c);
			if (path[0] != Path.DirectorySeparatorChar) sb.Append(Path.DirectorySeparatorChar);
 			sb.Append(path);

			return sb.ToString();
		}
	}

	internal class Hunk
	{
		static readonly int CONTEXT = 3;

		private DiffItem item;
		private int ctx1Start = 0;
		private int ctx2Start = 0;
		private int ctx1End = 0;
		private int ctx2End = 0;
		public int ctxLineCnt;

		public DiffItem Item { get { return item; } }

		public Hunk(DiffItem item, int prevDist, int nextDist, 
								int maxA, int maxB)
		{
			this.item = item;

			ctx1Start = Math.Max(item.StartA - prevDist, 0);
			int proposedEnd = Math.Min(item.StartA, ctx1Start + prevDist);
			ctx1End = Math.Min(proposedEnd, maxA);

			if (nextDist >= CONTEXT)
				{
					ctx2End = Math.Min(item.StartB + item.insertedB + nextDist, maxB);
					int proposedStart = Math.Max(item.StartB + item.insertedB, ctx2End - nextDist);
					//Console.WriteLine("proposedStart {0}, nextDist {1}", proposedStart, nextDist);
					ctx2Start = Math.Min(proposedStart, ctx2End);
				}

			ctxLineCnt = (ctx1End - ctx1Start) + (ctx2End - ctx2Start);
			//Console.WriteLine(String.Format("ctx1Start={0} ctx1End={1} ctx2Start={2} ctx2End={3} ctxLineCnt={4}\nmaxB={5} prevDist={6} nextDist={7}", 
			//																ctx1Start, ctx1End, ctx2Start, ctx2End, ctxLineCnt, maxB, prevDist, nextDist));
		}

		public int LinesA { get { return ctxLineCnt + item.deletedA; } }
		public int LinesB { get { return ctxLineCnt + item.insertedB; } }

		public string ToString(string[] a, string[] b)
		{
			StringBuilder sb = new StringBuilder();
			for (int i = ctx1Start; i < ctx1End; i++)
				sb.Append(" " + a[i] + "\n");

			for (int i = item.StartA; i < item.StartA + item.deletedA; i++)
				sb.Append("-" + a[i] + "\n");

			for (int i = item.StartB; i < item.StartB + item.insertedB; i++)
				sb.Append("+" + b[i] + "\n");
			
			for (int i = ctx2Start; i < ctx2End; i++)
				sb.Append(" " + b[i] + "\n");

			return sb.ToString();
		}
	}

}