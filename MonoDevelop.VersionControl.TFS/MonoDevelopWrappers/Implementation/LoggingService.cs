// LoggingService.cs
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

using System;
using System.IO;
using System.Text;
using MonoDevelop.Core.Logging;
using MonoDevelop.VersionControl.TFS.Infrastructure.Settings;

namespace MonoDevelop.VersionControl.TFS.MonoDevelopWrappers.Implementation
{
    sealed class LoggingService : ILoggingService
    {
        private readonly IConfigurationService _configurationService;
        readonly static object locker = new object();
        private Configuration _configuration;

        public LoggingService(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
            _configuration = _configurationService.Load();
        }

        public void LogToDebug(string message)
        {
            if (_configuration.IsDebugMode)
            {
                lock (locker)
                {
                    var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "TFS.VersionControl.Debug.log");
                    File.AppendAllText(path, message, Encoding.UTF8);
                }
            }
        }

        public void LogToInfo(string message)
        {
            MonoDevelop.Core.LoggingService.Log(LogLevel.Info, message);
        }
    }
}
