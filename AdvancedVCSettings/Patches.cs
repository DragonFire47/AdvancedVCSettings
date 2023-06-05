using HarmonyLib;
using PulsarModLoader.Utilities;
using Steamworks;
using System.Collections.Generic;
using System.Linq;

namespace AdvancedVCSettings
{
    class Patches
    {
        //Loads settings from saved steam IDs when other players join the current session. Due to SteamIDs being sent by the PLPlayer onPhotonSerialization method, steamID settings cannot be loaded until steamIDs have been assigned.
        [HarmonyPatch(typeof(PLServer), "NotifyPlayerStart")]
        class SteamSettingOthersJoinPatch
        {
            static void Postfix(PLServer __instance, int inPlayerID)
            {
                PLPlayer player = PLServer.Instance.GetPlayerFromPlayerID(inPlayerID);
                if(player == null)
                {
                    Logger.Info("AVC NPSPatch: Player was null");
                    return;
                }

                PhotonPlayer PPlayer = player.GetPhotonPlayer();

                if (PPlayer != null && Global.SteamPlayerVolumes.Value.ContainsKey(PPlayer.SteamID.m_SteamID))
                {
                    if (Global.PlayerVolumes.ContainsKey(PPlayer))
                    {
                        Global.PlayerVolumes[PPlayer] = Global.SteamPlayerVolumes.Value[PPlayer.SteamID.m_SteamID];
                    }
                    else
                    {
                        Logger.Info("Couldn't load setting from steam volumes into PlayerVolumes");
                    }
                }
            }
        }

        //Loads settings from saved steam IDs when joining an existing session.
        [HarmonyPatch(typeof(PLGlobal), "EnterNewGame")]
        class SteamSettingOnJoinPatch
        {
            static void Postfix(PLGlobal __instance)
            {
                List<PhotonPlayer> PlayerList = Global.PlayerVolumes.Keys.ToList();
                foreach (PhotonPlayer PPlayer in PlayerList)
                {
                    if (PPlayer.SteamID != CSteamID.Nil && Global.SteamPlayerVolumes.Value.ContainsKey(PPlayer.SteamID.m_SteamID))
                    {
                        Global.PlayerVolumes[PPlayer] = Global.SteamPlayerVolumes.Value[PPlayer.SteamID.m_SteamID];
                    }
                }
            }
        }

        //Adds volume to list when photonVoice is instantiated.
        [HarmonyPatch(typeof(PLPhotonVoice), "Start")]
        class PLPVStartPatch
        {
            static void Postfix(PLPhotonVoice __instance)
            {
                PhotonPlayer PPLayer = __instance.photonView.owner;
                if (!PPLayer.IsLocal)
                {
                    Global.PlayerVolumes.Add(PPLayer, 1f);
                }
            }
        }

        //Modifies volume
        [HarmonyPatch(typeof(PhotonVoiceSpeaker), "OnAudioFrame")]
        internal class PVSAudioFramePatch
        {
            static void Prefix(PhotonVoiceSpeaker __instance, ref float[] frame)
            {
                PhotonPlayer player = __instance.photonView.owner;
                if (Global.PlayerVolumes.ContainsKey(player))
                {
                    float playerVolume = Global.PlayerVolumes[player];
                    for (int i = 0; i < frame.Length; i++)
                    {
                        frame[i] *= playerVolume;
                    }
                }
                float value = Global.VCMainVolume.Value;
                for (int i = 0; i < frame.Length; i++)
                {
                    frame[i] *= value;
                }
            }
        }

        //Remove players from list as they disconnect.
        [HarmonyPatch(typeof(PLNetworkManager), "OnPhotonPlayerDisconnected")]
        class PlayerDisconnectPatch
        {
            static void Postfix(PhotonPlayer photonPlayer)
            {
                Global.PlayerVolumes.Remove(photonPlayer);
            }
        }

        //clear playerVolumes on session end.
        [HarmonyPatch(typeof(PLNetworkManager), "OnLeaveGame")]
        class JoinSessionPatch
        {
            static void Postfix()
            {
                Global.PlayerVolumes.Clear();
            }
        }

        //Patch Vanilla VC menu to reconnect when enabled.
        [HarmonyPatch(typeof(PLUIAudioSettingsMenu), "Update")]
        class VanillaSettingsMenuPatch
        {
            static bool CachedIsOn = true;
            static void Postfix(PLUIAudioSettingsMenu __instance)
            {
                if(__instance.Visuals.activeSelf && __instance.VCEnabledToggle.isOn != CachedIsOn)
                {
                    CachedIsOn = __instance.VCEnabledToggle.isOn;
                    if (CachedIsOn)
                    {
                        PhotonVoiceNetwork.Client.Reconnect();
                    }
                }
            }
        }
    }
}
