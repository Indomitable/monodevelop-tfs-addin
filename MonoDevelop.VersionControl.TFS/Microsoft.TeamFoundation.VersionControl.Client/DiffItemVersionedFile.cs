//
// Microsoft.TeamFoundation.VersionControl.Client.DiffItemVersionedFile
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
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Web.Services;
using Microsoft.TeamFoundation.VersionControl.Client.Objects;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
    public sealed class DiffItemVersionedFile : IDiffItem
    {
        private string label;
        private Item item;

        public DiffItemVersionedFile(Item item)
        {
            this.item = item;
            this.label = item.ServerItem;
        }

        public DiffItemVersionedFile(int itemId, int changeset, string displayPath)
        {
            //this.item = versionControl.GetItem(itemId, changeset);
            this.label = displayPath;
        }

        public string GetFile()
        {
            throw new NotImplementedException();
            // this is a quite inefficient implementation, FIXME 
//            string tname = Path.GetTempFileName();
//            item.DownloadFile(tname);
//
//            string file;
//            using (StreamReader sr = new StreamReader(tname))
//            {
//                file = sr.ReadToEnd();
//            }
//			
//            File.Delete(tname);
//            return file;
        }

        public bool IsTemporary
        { 
            get { return true; }
        }

        public string Label
        { 
            get { return label; }
            set { label = value; }
        }

        public int GetEncoding()
        {
            return item.Encoding;
        }
    }
}
