using System;
using System.Net;
using MonoDevelop.Core;
using System.Security.Policy;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using DBus;
using System.Collections.Generic;

namespace MonoDevelop.VersionControl.TFS.Helpers
{
    public static class CredentialsManager
    {
        const string applicationId = "MonoDevelop.VersionControl.TFS.Addin";
        const string folderName = "VersionControl.TFS";

        public static bool IsRunningKDE
        {
            get
            {
                return !string.IsNullOrWhiteSpace(System.Environment.GetEnvironmentVariable("KDE_SESSION_VERSION"));
            }
        }

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
            try
            {
                var wallet = Bus.Session.GetObject<IKWallet>("org.kde.kwalletd", new ObjectPath("/modules/kwalletd"));
                if (wallet == null)
                    return null;
                var walletName = wallet.networkWallet();

                int handle = -1;
                try
                {
                    handle = wallet.open(walletName, 0, applicationId);
                    return wallet.readPassword(handle, folderName, url.ToString(), applicationId);
                }
                finally
                {
                    if (handle != -1 && wallet.isOpen(walletName))
                        wallet.close(handle, false, applicationId);
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Stores the credential.
        /// </summary>
        /// <returns><c>true</c>, if credential was stored, <c>false</c> otherwise save password in Uri (unsecure)</returns>
        /// <param name="url">URL.</param>
        /// <param name="password">Password.</param>
        public static bool StoreCredential(Uri url, string password)
        {
            //Use Password Service for Mac and Windows
            if (Platform.IsMac || Platform.IsWindows)
            {
                PasswordService.AddWebPassword(url, password);
            }
            else //If Linux
            {
                if (!IsRunningKDE || !SaveToKWallet(url, password)) //Use KDE Wallet should write code for Gnome Keyring.
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Loads the credential.
        /// </summary>
        /// <returns>The credential.</returns>
        /// <param name="url">URL.</param>
        public static string LoadCredential(Uri url)
        {
            if (Platform.IsMac || Platform.IsWindows)
            {
                return PasswordService.GetWebPassword(url);
            }
            else //If Linux
            {
                if (IsRunningKDE)
                {
                    string password = LoadFromKWallet(url);
                    return password ?? new UriBuilder(url).Password;
                }
                return new UriBuilder(url).Password;
            }
        }
    }
}

