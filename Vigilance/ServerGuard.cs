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
using Vigilance.Extensions;

namespace Vigilance
{

    public static class ServerGuard
    {
        private static List<string> _modules;
        public static bool IsEnabled => ConfigManager.IsServerGuardEnabled;
        public static List<string> EnabledModules
        {
            get
            {
                if (_modules == null)
                {
                    _modules = new List<string>();
                    foreach (string str in ConfigManager.GuardEnabledModules)
                    {
                        if (str.ToLower() == "vpn" || str.ToLower() == "vpnshield")
                            _modules.Add("vpn");
                        if (str.ToLower() == "steam" || str.ToLower() == "steamshield")
                            _modules.Add("steam");
                    }
                }
                return _modules;
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
            public static string APIKey => ConfigManager.VpnApiKey;

            public static bool CheckIP(Player player)
            {
                if (!ServerGuard.IsEnabled)
                    return false;
                if (!IsEnabled)
                    return false;
                if (player == null)
                    return false;
                if (APIKey.IsEmpty() || APIKey.ToLower() == "none")
                    return false;
                if (string.IsNullOrEmpty(player.UserId) || player.IpAddress == "localClient")
                    return false;
                string ipAddress = player.IpAddress.Replace("::ffff:", "");
                if (ConfigManager.IpWhitelist.Contains(ipAddress))
                    return false;
                if (ConfigManager.UserIdWhitelist.Contains(player.UserId))
                    return false;
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
                catch (WebException)
                {

                }
                catch (Exception e)
                {
                    if (!(e is WebException))
                        Log.Add("ServerGuard", e);
                }
                return false;
            }
        }

        public static class SteamShield
        {

            public static bool IsEnabled => EnabledModules.Contains("steam");
            public static bool BlockNewAccounts => ConfigManager.ShouldKickNewAccounts;
            public static bool BlockNonSetupAccounts => ConfigManager.ShouldKickNonSetupAccounts;

            public static bool CheckAccount(Player player)
            {
                if (!ServerGuard.IsEnabled)
                    return false;
                if (!IsEnabled)
                    return false;
                if (player == null)
                    return false;
                if (string.IsNullOrEmpty(player.UserId))
                    return false;
                if (!player.UserId.Contains("@steam") || !player.UserIdType.IsSteam() | player.UserIdType != UserIdType.Steam)
                    return false;
                if (ConfigManager.UserIdWhitelist.Contains(player.UserId))
                    return false;
                if (ConfigManager.IpWhitelist.Contains(player.IpAddress))
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
                            Log.Add(nameof(SteamShield.CheckAccount), $"Failed while checking Steam profile [{player.Nick} ({player.ParsedUserId})]", LogType.Warn);
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
                catch (WebException)
                {

                }
                catch (Exception e)
                {
                    if (!(e is WebException))
                        Log.Add("ServerGuard", e);
                }
                return false;
            }
        }
    }
}
