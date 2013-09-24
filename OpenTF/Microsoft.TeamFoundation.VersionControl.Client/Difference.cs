//
// Microsoft.TeamFoundation.VersionControl.Client.Difference
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
	public static class Difference
	{
		internal static readonly int CONTEXT = 3;

		internal static void WriteHunkSet(StreamWriter stream, 
																			string[] a, string[] b, List<Hunk> hunkSet)
		{
			if (hunkSet.Count == 0) return;

			Hunk hunk1 = hunkSet[0];
			DiffItem item1 = hunk1.Item;

			int ctxStartA = Math.Max(item1.StartA - CONTEXT, -1);
			int ctxStartB = Math.Max(item1.StartB - CONTEXT, -1);

			int linesA = 0;
			int linesB = 0;

			foreach (Hunk hunk in hunkSet)
				{
					linesA += hunk.LinesA;
					linesB += hunk.LinesB;
				}

			string header = String.Format("@@ -{0},{1} +",
																		ctxStartA+1, linesA);

			header += String.Format("{0},", ctxStartB+1);
			header += String.Format("{0} @@", linesB);

			//			header += String.Format("{0},{1},{2},{3}", 
			//												item1.StartA, item1.deletedA, item1.StartB, item1.insertedB);

			stream.WriteLine(header);

			foreach (Hunk hunk in hunkSet)
				{
					stream.Write(hunk.ToString(a, b));
				}
		}

		internal static void WriteUnified(StreamWriter stream, 
																			string[] a, string[] b, DiffItem[] items)
		{
			List<Hunk> hunkSet = new List<Hunk>();
			for (int x = 0; x < items.Length; x++)
				{
					DiffItem item = items[x];

					int prevDist = CONTEXT;
					if (x > 0) prevDist = Math.Min(CONTEXT, item.StartA - (items[x-1].StartA + items[x-1].deletedA));

					int nextDist = CONTEXT;
					if (x < items.Length - 1) nextDist = Math.Min(CONTEXT, items[x+1].StartA - item.StartA - 1);

					Hunk hunk = new Hunk(item, prevDist, nextDist, a.Length, b.Length);
					hunkSet.Add(hunk);

					if (nextDist == CONTEXT)
						{
							WriteHunkSet(stream, a, b, hunkSet);
							hunkSet.Clear();
						}
				}

			WriteHunkSet(stream, a, b, hunkSet);
		}

		internal static void WriteNewFile(StreamWriter stream, 
																			string[] b)
		{
			DiffItem item = new DiffItem();
			item.StartA = 0; item.StartB = 0; item.deletedA = 0; item.insertedB = b.Length;

			stream.WriteLine(String.Format("@@ -0,0 +1,{0} @@", b.Length));
			Hunk hunk = new Hunk(item, 0, 0, 0, b.Length);
			stream.Write(hunk.ToString(new string[0], b));
		}

		internal static void WriteHeader(DiffItemUtil aItem, DiffItemUtil bItem,
																		 DiffOptions diffOpts)
		{
			StreamWriter stream = diffOpts.StreamWriter;
			stream.Write("diff --tfs " + aItem.Name + " ");
			if (!String.IsNullOrEmpty(diffOpts.SourceLabel))
				stream.Write(diffOpts.SourceLabel + " ");

			stream.Write(bItem.Name);
			if (!String.IsNullOrEmpty(diffOpts.TargetLabel))
				stream.Write("@" + diffOpts.TargetLabel);

			stream.WriteLine();
			
			// the trailing tabs below help diffutils grok filenames with spaces
			stream.WriteLine("--- " + aItem.Name + "\t");
			stream.WriteLine("+++ " + bItem.Name + "\t");
		}

		public static void DiffFiles (VersionControlServer versionControl,
																	IDiffItem source, IDiffItem target,
																	DiffOptions diffOpts, string fileNameForHeader,
																	bool wait)
		{
			DiffItemUtil aItem = new DiffItemUtil('a', fileNameForHeader, source.GetFile());
			DiffItemUtil bItem = new DiffItemUtil('b', fileNameForHeader, target.GetFile());
			StreamWriter stream = diffOpts.StreamWriter;

			// short circuit for binary file comparisions
			if (source.GetEncoding() == RepositoryConstants.EncodingBinary && target.GetEncoding() == RepositoryConstants.EncodingBinary)
				{
					stream.WriteLine("Binary files {0} and {1} differ", aItem.Name, bItem.Name);
					return;
				}

			WriteHeader(aItem, bItem, diffOpts);

			// short circuit new files
			if (aItem.Length == 0)
				{
					WriteNewFile(stream, bItem.Lines);
					return;
				}

			Hashtable hashtable = new Hashtable(aItem.Length + bItem.Length);
			bool ignoreWhiteSpace = (diffOpts.Flags & DiffOptionFlags.IgnoreWhiteSpace) ==  DiffOptionFlags.IgnoreWhiteSpace;

			DiffItem[] items = DiffUtil.DiffText(hashtable, aItem.Lines, bItem.Lines, 
																					 ignoreWhiteSpace, ignoreWhiteSpace, false);

			WriteUnified(stream, aItem.Lines, bItem.Lines, items);
		}
	}
}
