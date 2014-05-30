//
// FileOperationsHelper.cs
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
using System.IO;
using MonoDevelop.Core;
using Microsoft.TeamFoundation.VersionControl.Client.Enums;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
    public static class FileHelper
    {
        #region File Operations

        public static bool FileMove(string source, string destination)
        {
            if (!File.Exists(source) || File.Exists(destination))
                return false;
            try
            {
                File.Move(source, destination);
                FileService.NotifyFileRemoved(source);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool FileMove(string source, string destination, bool overrideDetination)
        {
            if (!File.Exists(source))
                return false;
            if (!overrideDetination && File.Exists(destination))
                return false;
            try
            {
                bool flag = false;
                if (overrideDetination && File.Exists(destination))
                {
                    File.Delete(destination);
                    flag = true;
                }
                File.Move(source, destination);
                if (flag)
                    FileService.NotifyFileChanged(destination);
                FileService.NotifyFileRemoved(source);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool FileCopy(string source, string destination)
        {
            try
            {
                if (File.Exists(source) && !File.Exists(destination))
                {
                    File.Copy(source, destination, true);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static bool FileDelete(string source)
        {
            try
            {
                if (File.Exists(source))
                {
                    File.Delete(source);
                    FileService.NotifyFileRemoved(source);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Folder Operations

        public static bool FolderMove(string source, string destination)
        {
            if (!Directory.Exists(source) || Directory.Exists(destination))
                return false;
            try
            {
                Directory.Move(source, destination);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool FolderMove(string source, string destination, bool overrideDetination)
        {
            if (!Directory.Exists(source))
                return false;
            if (!overrideDetination && Directory.Exists(destination))
                return false;
            try
            {
                if (overrideDetination && Directory.Exists(destination))
                    Directory.Delete(destination, true);
                Directory.Move(source, destination);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool FolderDelete(string source)
        {
            try
            {
                if (Directory.Exists(source))
                {
                    Directory.Delete(source, true);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        public static bool Delete(ItemType itemType, string source)
        {
            if (itemType == ItemType.File)
                return FileHelper.FileDelete(source);
            else
                return FileHelper.FolderDelete(source);
        }

        public static bool HasFile(string path)
        {
            return File.Exists(path);
        }

        public static bool HasFolder(string path)
        {
            return Directory.Exists(path);
        }

        public static bool Exists(string path)
        {
            return File.Exists(path) || Directory.Exists(path);
        }
    }
}

