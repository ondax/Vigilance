using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Vigilance.API;
using Vigilance.API.Enums;

namespace Vigilance
{

    public static class ServerGuard
    {
        public static bool IsEnabled => ConfigManager.GetBool("guard_enabled");
        public static List<Module> EnabledModules => GetEnabledModules();

        private static List<Module> GetEnabledModules()
        {
            List<Module> modules = new List<Module>();
            foreach (string str in ConfigManager.GetStringList("guard_enabled_modules"))
            {
                if (str.ToLower() == "vpn" || str.ToLower() == "vpnshield")
                    modules.Add(Module.VPNShield);
                if (str.ToLower() == "steam" || str.ToLower() == "steamshield")
                    modules.Add(Module.SteamShield);
            }
            return modules;
        }

        public static void KickPlayer(Player player, KickType kickType)
        {
            if (kickType == KickType.NewAccount)
            {
                player.Kick("This server does not allow new Steam accounts, you have to buy something on Steam before playing.");
                Log.Info("ServerGuard", $"Kicked {player.Nick} for {kickType}");
            }

            if (kickType == KickType.NonSetupAccount)
            {
                player.Kick("This server does not allow non setup Steam accounts, you have to setup your Steam profile before playing.");
                Log.Info("ServerGuard", $"Kicked {player.Nick} for {kickType}");
            }

            if (kickType == KickType.DetectedVPN)
            {
                player.Kick("This server does not allow VPN connections.");
                Log.Info("ServerGuard", $"Kicked {player.Nick} for {kickType}");
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
                    {
                        continue;
                    }
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
            public static bool IsEnabled => EnabledModules.Contains(Module.VPNShield);

            public static string APIKey => ConfigManager.GetString("vpn_api_key");

            public static bool CheckIP(Player player)
            {
                if (!IsEnabled)
                    return false;
				string ipAddress = player.IpAdress.Replace("::ffff:", "");
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
                        KickPlayer(player, KickType.DetectedVPN);
						response.Close();
						return true;
					}
				}
				catch (WebException e)
				{
					Log.Error("ServerGuard", e);
				}
				return false;
			}
        }

        public static class SteamShield
        {

            public static bool IsEnabled => EnabledModules.Contains(Module.SteamShield);

            public static bool BlockNewAccounts => ConfigManager.GetBool("steam_block_new_accounts");
            public static bool BlockNonSetupAccounts => ConfigManager.GetBool("steam_block_non_setup_accounts");

            public static bool CheckAccount(Player player)
            {
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
                            KickPlayer(player, KickType.NonSetupAccount);
                            return true;
                        }
                        else
                        {
                            Log.Error("ServerGuard", "Steam account check failed. Their profile did not have the required information.");
                            return false;
                        }
                    }
                    bool isLimitedAccount = foundStrings[0].Where(c => char.IsDigit(c)).ToArray()[0] != '0';
                    if (isLimitedAccount)
                    {
                        if (BlockNewAccounts)
                        {
                            KickPlayer(player, KickType.NewAccount);
                            return true;
                        }
                    }
                }
                catch (WebException e)
                {
                    Log.Error("ServerGuard", e);
                }
                return false;
            }
        }
    }
}
