using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = PulsarModLoader.Utilities.Logger;

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
                    Logger.Info("AVC NPSPatch: PLPlayer was null");
                    return;
                }

                PhotonPlayer PPlayer = player.GetPhotonPlayer();

                if (PPlayer != null)
                {
                    if(PPlayer.IsLocal)
                    {
                        return;
                    }

                    Global.PlayerData.Add(PPlayer, new PlayerVCSettings(player));
                }
                else
                {
                    Logger.Info("AVC NPSPatch: PPlayer was null");
                }
            }
        }

        //Loads settings from saved steam IDs when joining an existing session.
        [HarmonyPatch(typeof(PLGlobal), "EnterNewGame")]
        class SteamSettingOnJoinPatch
        {
            static void Postfix(PLGlobal __instance)
            {
                foreach (PhotonPlayer PPlayer in PhotonNetwork.otherPlayers)
                {
                    PLPlayer player = PLServer.GetPlayerForPhotonPlayer(PPlayer);
                    if (player != null)
                    {
                        Global.PlayerData.Add(PPlayer, new PlayerVCSettings(player));
                    }
                    else
                    {
                        Logger.Info("AVC ENGPatch: PLPlayer was null");
                    }
                }
            }
        }

        //Modifies volume
        [HarmonyPatch(typeof(PhotonVoiceSpeaker), "OnAudioFrame")]
        internal class PVSAudioFramePatch
        {
            static float lastPrioritySpeakerTime = 0f;

            static void Prefix(PhotonVoiceSpeaker __instance, ref float[] frame)
            {
                PhotonPlayer player = __instance.photonView.owner;
                float time = Time.time;
                if (Global.PlayerData.ContainsKey(player))
                {
                    PlayerVCSettings playerSettings = Global.PlayerData[player];
                    if (playerSettings.IsPrioritySpeaker)
                    {
                        lastPrioritySpeakerTime = time;
                    }
                    //Volume multiplier = Custom Player Volume * Multiplier based on priority speaker. (if last PST > .5 seconds ago && player not a priority speaker. true = volume setting, false = 1x)
                    float playerVolume = playerSettings.PlayerVolume * ((time - lastPrioritySpeakerTime < 0.5f && !playerSettings.IsPrioritySpeaker) ? Global.nonPrioritySpeakerVolume.Value : 1f);
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
                Global.PlayerData.Remove(photonPlayer);
            }
        }

        //clear playerVolumes on session end.
        [HarmonyPatch(typeof(PLNetworkManager), "OnLeaveGame")]
        class JoinSessionPatch
        {
            static void Postfix()
            {
                Global.PlayerData.Clear();
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
                    else
                    {
                        PhotonVoiceNetwork.Client.Disconnect();
                    }
                }
            }
        }
    }
}
