using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Vigilance.API;
using Vigilance.Enums;

namespace Vigilance
{

    public static class ServerGuard
    {
        public static bool IsEnabled => PluginManager.Config.GetBool("guard_enabled", false);
        public static List<string> EnabledModules
        {
            get
            {
                List<string> modules = new List<string>();
                foreach (string str in PluginManager.Config.GetStringList("guard_enabled_modules"))
                {
                    if (str.ToLower() == "vpn" || str.ToLower() == "vpnshield")
                        modules.Add("vpn");
                    if (str.ToLower() == "steam" || str.ToLower() == "steamshield")
                        modules.Add("steam");
                }
                return modules;
            }
        }

        public static void KickPlayer(Player player, int kickType)
        {
            if (kickType == 1)
            {
                player.Kick("This server does not allow new Steam accounts. You have to buy something on Steam before playing.");
                Log.Add("ServerGuard", $"Detected a new account [{player.Nick} ({player.ParsedUserId})]", LogType.Info);
            }

            if (kickType == 2)
            {
                player.Kick("This server does not allow non setup Steam accounts. You have to setup your Steam profile before playing.");
                Log.Add("ServerGuard", $"Detected a non-setup account [{player.Nick} ({player.ParsedUserId})]", LogType.Info);
            }

            if (kickType == 3)
            {
                player.Kick("This server does not allow VPN connections.");
                Log.Add("ServerGuard", $"Detected a possible VPN connection [{player.Nick} ({player.UserIdType}ID: {player.ParsedUserId} / IP: {player.IpAddress})", LogType.Info);
            }
        }

        public static bool SSLValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            bool isOk = true;
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                for (int i = 0; i < chain.ChainStatus.Length; i++)
                {
                    if (chain.ChainStatus[i].Status == X509ChainStatusFlags.RevocationStatusUnknown)
                        continue;
                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                    chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                    bool chainIsValid = chain.Build((X509Certificate2)certificate);
                    if (!chainIsValid)
                    {
                        isOk = false;
                        break;
                    }
                }
            }
            return isOk;
        }

        public static class VPNShield
        {
            public static bool IsEnabled => EnabledModules.Contains("vpn");
            public static string APIKey => PluginManager.Config.GetString("vpn_api_key");

            public static bool CheckIP(Player player)
            {
                if (!IsEnabled)
                    return false;
                if (APIKey.IsEmpty())
                    return false;
                string ipAddress = player.IpAddress.Replace("::ffff:", "");
                HttpWebResponse response = null;
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://v2.api.iphub.info/ip/" + ipAddress);
                    request.Headers.Add("x-key", APIKey);
                    request.Method = "GET";
                    response = (HttpWebResponse)request.GetResponse();
                    string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    JObject json = JObject.Parse(responseString);
                    int verificationLevel = json.Value<int>("block");
                    if (verificationLevel == 1)
                    {
                        KickPlayer(player, 3);
                        response.Close();
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Log.Add("ServerGuard", e);
                }
                return false;
            }
        }

        public static class SteamShield
        {

            public static bool IsEnabled => EnabledModules.Contains("steam");
            public static bool BlockNewAccounts => PluginManager.Config.GetBool("steam_block_new_accounts");
            public static bool BlockNonSetupAccounts => PluginManager.Config.GetBool("steam_block_non_setup_accounts");

            public static bool CheckAccount(Player player)
            {
                if (!IsEnabled)
                    return false;
                if (player.UserIdType == UserIdType.Discord)
                    return false;
                ServicePointManager.ServerCertificateValidationCallback = SSLValidation;
                HttpWebResponse response = null;
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://steamcommunity.com/profiles/" + player.ParsedUserId + "?xml=1");
                    request.Method = "GET";
                    response = (HttpWebResponse)request.GetResponse();
                    string xmlResponse = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    string[] foundStrings = xmlResponse.Split('\n').Where(w => w.Contains("isLimitedAccount")).ToArray();

                    if (foundStrings.Length == 0)
                    {
                        if (BlockNonSetupAccounts)
                        {
                            KickPlayer(player, 2);
                            return true;
                        }
                        else
                        {
                            Log.Add("ServerGuard", $"Failed while checking Steam profile [{player.Nick} ({player.ParsedUserId})]", LogType.Warn);
                            return false;
                        }
                    }
                    bool isLimitedAccount = foundStrings[0].Where(c => char.IsDigit(c)).ToArray()[0] != '0';
                    if (isLimitedAccount)
                    {
                        if (BlockNewAccounts)
                        {
                            KickPlayer(player, 1);
                            return true;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Add("ServerGuard", e);
                }
                return false;
            }
        }
    }
}
