﻿using HarmonyLib;
using PulsarModLoader.Chat.Commands.CommandRouter;
using PulsarModLoader.Utilities;
using System.Collections.Generic;

namespace AdvancedVCSettings
{
    internal class Commands : ChatCommand
    {
        public override string[][] Arguments()
        {
            return new string[][] { new string[] { "vctoggle", "vct", "master", "mastervolume", "mastervolumemultiplier", "mvm", "mastermultiplier", "mm", "playervolume", "pv", "fix" } };
        }

        public override string[] CommandAliases()
        {
            return new string[] { "advancedvoicechat", "avc" };
        }

        public override string Description()
        {
            return "Controls subcommands for advanced voice chat. VCT - Toggle VC on/off, MV [value] - Set Main Volume, MVM [value] - Set Main Volume Multiplier, " +
                "PV [PlayerID] [Value] - Set Player Volume, FIX - Attempts to fix VC";
        }

        public override void Execute(string arguments)
        {
            string[] args = arguments.ToLower().Split(' ');
            switch (args[0])
            {
                case "vctoggle":
                case "vct":
                    Messaging.Notification("Voice Chat: " + !Global.GetVCState());
                    Global.SetVCState(!Global.GetVCState());
                    break;
                case "master":
                case "mastervolume":
                case "mv":
                    if (args.Length > 1 && float.TryParse(args[1], out float masterVolume))
                    {
                        PLXMLOptionsIO.Instance.CurrentOptions.SetFloatValue("VolumeVoice", masterVolume);
                        Messaging.Notification("Master volume set to: " + masterVolume.ToString("000%"));
                    }
                    else
                    {
                        Messaging.Notification("Please input a number. Current Value: " + PLXMLOptionsIO.Instance.CurrentOptions.GetFloatValue("VolumeVoice").ToString("000%"));
                    }
                    break;
                case "mastervolumemultiplier":
                case "mvm":
                case "mastermultiplier":
                case "mm":
                    if (args.Length > 1 && float.TryParse(args[1], out float masterVolumeMultiplier))
                    {
                        Global.VCMainVolume.Value = masterVolumeMultiplier;
                        Messaging.Notification("Master volume multiplier set to: " + Global.VCMainVolume.Value.ToString("0.#x"));
                    }
                    else
                    {
                        Messaging.Notification("Please input a number. Current Value: " + Global.VCMainVolume.Value.ToString("0.#x"));
                    }
                    break;
                case "playervolume":
                case "pv":
                    PLPlayer PVPlayer = null;
                    if (args.Length > 1)
                    {
                        PVPlayer = HelperMethods.GetPlayer(args[1]);
                    }
                    if (args.Length > 2 && PVPlayer && float.TryParse(args[2], out float playerVolume))
                    {
                        PhotonPlayer PPlayer = PVPlayer.GetPhotonPlayer();
                        if (Global.TrySetPlayerVolume(PPlayer, playerVolume))
                        {
                            Messaging.Notification($"volume of {PVPlayer.GetPlayerName()} set to {playerVolume.ToString("0.#x")}");
                        }
                        else
                        {
                            Messaging.Notification($"Cannot set volume for {PVPlayer.GetPlayerName()}");
                        }
                    }
                    else if (PVPlayer)
                    {
                        PhotonPlayer PPlayer = PVPlayer.GetPhotonPlayer();
                        if (Global.PlayerVolumes.TryGetValue(PPlayer, out float value))
                        {
                            Messaging.Notification($"Volume of {PVPlayer.GetPlayerName()} is currently set to {value.ToString("0.#x")}");
                        }
                        else
                        {
                            Messaging.Notification($"Voice failed to load for {PVPlayer.GetPlayerName()}");
                        }
                    }
                    else
                    {
                        Messaging.Notification("Requires a player name/ID and a number. /avc pv [player] [playervolume]");
                    }
                    break;
                case "fix":
                    Global.AttemptFixVCState();
                    break;
                case "dbg":
                    Dictionary<KeyValuePair<int, byte>, PhotonVoiceSpeaker> voices = (Dictionary<KeyValuePair<int, byte>, PhotonVoiceSpeaker>)AccessTools.Field(typeof(UnityVoiceFrontend), "voiceSpeakers").GetValue(PhotonVoiceNetwork.Client);
                    foreach (KeyValuePair<KeyValuePair<int, byte>, PhotonVoiceSpeaker> voice in voices)
                    {
                        Logger.Info($"AVCS Debug P1: Player: {voice.Key.Key}, VoiceID: {voice.Key.Value}, ViewID: {voice.Value.photonView.viewID}");
                    }
                    foreach (PLPlayer player in PLServer.Instance.AllPlayers)
                    {
                        if (player != null)
                        {
                            PhotonPlayer pPlayer = player.GetPhotonPlayer();
                            if (pPlayer != null)
                                Logger.Info($"AVCS Debug P2: {pPlayer.ID}");
                        }
                    }
                    Messaging.Notification("ClientState: " + PhotonVoiceNetwork.ClientState.ToString());
                    break;
                default:
                    Messaging.Notification("Please insert a subcommand, read '/help avc' for info on subcommands.");
                    break;
            }
        }
    }
}
