//
// CredentialsManager.cs
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
using System.Net;
using MonoDevelop.Core;

#if DBus
using DBus;
#endif
using System.Collections.Generic;

namespace MonoDevelop.VersionControl.TFS.Helpers
{
    public static class CredentialsManager
    {
        const string applicationId = "MonoDevelop.VersionControl.TFS.Addin";
        const string folderName = "VersionControl.TFS";
        #if DBus
        public static bool IsRunningKDE { get { return !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("KDE_SESSION_VERSION")); } }

        [Interface("org.kde.KWallet")]
        private interface IKWallet
        {
            string localWallet();

            string networkWallet();

            bool isEnabled();

            bool isOpen(string wallet);

            int open(string wallet, long wId, string appid);

            int writeEntry(int handle, string folder, string key, byte[] value, string appid);

            int writePassword(int handle, string folder, string key, string value, string appid);

            bool hasEntry(int handle, string folder, string key, string appid);

            int writeMap(int handle, string folder, string key, Dictionary<string, string> value, string appid);

            string readPassword(int handle, string folder, string key, string appid);

            int close(int handle, bool force, string appid);
        }

        private static bool SaveToKWallet(Uri url, string password)
        {
            try
            {
                var wallet = Bus.Session.GetObject<IKWallet>("org.kde.kwalletd", new ObjectPath("/modules/kwalletd"));
                if (wallet == null)
                    return false;
                var walletName = wallet.networkWallet();

                int handle = -1;
                try
                {
                    handle = wallet.open(walletName, 0, applicationId);
                    wallet.writePassword(handle, folderName, url.ToString(), password, applicationId);
                    return true;
                    //wallet.writeMap(handle, folderName, url.ToString(), new Dictionary<string, string> { { userName, password } }, password, applicationId);
                }
                finally
                {
                    if (handle != -1 && wallet.isOpen(walletName))
                        wallet.close(handle, false, applicationId);
                }
            }
            catch
            {
                return false;
            }
        }

        private static string LoadFromKWallet(Uri url)
        {
            var wallet = Bus.Session.GetObject<IKWallet>("org.kde.kwalletd", new ObjectPath("/modules/kwalletd"));
            if (wallet == null)
                return null;
            var walletName = wallet.networkWallet();

            int handle = -1;
            try
            {
                handle = wallet.open(walletName, 0, applicationId);
                if (!wallet.hasEntry(handle, folderName, url.ToString(), applicationId))
                    throw new Exception("No Password!\nRegister the server again.");
                return wallet.readPassword(handle, folderName, url.ToString(), applicationId);
            }
            finally
            {
                if (handle != -1 && wallet.isOpen(walletName))
                    wallet.close(handle, false, applicationId);
            }
        }
        #else
		public static bool IsRunningKDE { get { return false; } }
        #endif
        /// <summary>
        /// Stores the credential.
        /// </summary>
        /// <returns><c>true</c>, if credential was stored, <c>false</c> otherwise save password in Uri (unsecure)</returns>
        /// <param name="url">URL.</param>
        /// <param name="password">Password.</param>
        public static void StoreCredential(Uri url, string password)
        {
            //Use Password Service for Mac and Windows
            if (Platform.IsMac || Platform.IsWindows)
            {
                PasswordService.AddWebPassword(url, password);
            }
            else //If Linux
            {
#if DBus
                if (IsRunningKDE)
                {
                    SaveToKWallet(url, password);//Use KDE Wallet should write code for Gnome Keyring.
                }
#endif
            }
        }

        public static string GetPassword(Uri url)
        {
            try
            {
                if (Platform.IsMac || Platform.IsWindows)
                {
                    return PasswordService.GetWebPassword(url);
                }
                else //If Linux
                {
#if DBus
                    if (IsRunningKDE)
                    {
                        return LoadFromKWallet(url);
                    }
#endif
                }
            }
            catch
            {
                return null;
            }
            return null;
        }
    }
}

