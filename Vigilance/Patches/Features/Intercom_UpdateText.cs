using Harmony;
using UnityEngine;
using Vigilance.API;
using System;

namespace Vigilance.Patches.Features
{
	[HarmonyPatch(typeof(Intercom), nameof(Intercom.UpdateText))]
	public static class Intercom_UpdateText
	{
		public static bool Prefix(Intercom __instance)
		{
			try
			{
				if (!string.IsNullOrEmpty(__instance.CustomContent))
				{
					__instance.IntercomState = Intercom.State.Custom;
					__instance.Network_intercomText = __instance.CustomContent;
				}

				else if (__instance.Muted)
				{
					Map.Intercom.SetContent(__instance, Intercom.State.Muted, ConfigManager.Intercom_Muted);
				}

				else if (Intercom.AdminSpeaking)
				{
					Map.Intercom.SetContent(__instance, Intercom.State.AdminSpeaking, ConfigManager.Intercom_Admin);
				}

				else if (__instance.remainingCooldown > 0f)
				{
					int num = Mathf.CeilToInt(__instance.remainingCooldown);
					__instance.NetworkIntercomTime = (ushort)((num >= 0) ? ((ushort)num) : 0);
					Map.Intercom.SetContent(__instance, Intercom.State.Restarting, ConfigManager.Intercom_Restart);
				}

				else if (__instance.Networkspeaker != null)
				{
					if (__instance.bypassSpeaking)
					{
						Map.Intercom.SetContent(__instance, Intercom.State.TransmittingBypass, ConfigManager.Intercom_Bypass);
					}
					else
					{
						int num2 = Mathf.CeilToInt(__instance.speechRemainingTime);
						__instance.NetworkIntercomTime = (ushort)((num2 >= 0) ? ((ushort)num2) : 0);
						Map.Intercom.SetContent(__instance, Intercom.State.Transmitting, ConfigManager.Intercom_Transmit);
					}
				}
				else
				{
					Map.Intercom.SetContent(__instance, Intercom.State.Ready, ConfigManager.Intercom_Ready);
				}

				if (Intercom.AdminSpeaking != Intercom.LastState)
				{
					Intercom.LastState = Intercom.AdminSpeaking;
					__instance.RpcUpdateAdminStatus(Intercom.AdminSpeaking);
				}

				return false;
			}
			catch (Exception e)
			{
				Log.Add(nameof(Intercom.UpdateText), e);
				return true;
			}
		}
	}
}
