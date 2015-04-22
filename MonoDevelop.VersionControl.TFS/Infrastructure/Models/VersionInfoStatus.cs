﻿// VersionInfoStatus.cs
// 
// Author:
//       Ventsislav Mladenov
// 
// The MIT License (MIT)
// 
// Copyright (c) 2013-2015 Ventsislav Mladenov
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

using MonoDevelop.VersionControl.TFS.VersionControl.Infrastructure;

namespace MonoDevelop.VersionControl.TFS.Infrastructure.Models
{
    internal sealed class VersionInfoStatus
    {
        public RepositoryPath RemotePath { get; set; }
        public VersionStatus LocalStatus { get; set; }
        public TFSRevision LocalRevision { get; set; }
        public VersionStatus RemoteStatus { get; set; }
        public TFSRevision RemoteRevision { get; set; }

        public bool IsUnversioned
        {
            get { return LocalStatus == VersionStatus.Unversioned; }
        }
        
        public static VersionInfoStatus Unversioned
        {
            get
            {
                return new VersionInfoStatus
                {
                    LocalStatus = VersionStatus.Unversioned,
                };
            }
        }


        public static VersionInfoStatus Versioned(RepositoryPath remotePath)
        {
            return new VersionInfoStatus
            {
                RemotePath = remotePath,
                LocalStatus = VersionStatus.Versioned,
            };
        }
    }
}