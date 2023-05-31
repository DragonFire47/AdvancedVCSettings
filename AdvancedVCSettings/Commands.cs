using HarmonyLib;
using PulsarModLoader.Chat.Commands.CommandRouter;
using PulsarModLoader.Utilities;
using System.Collections.Generic;

namespace AdvancedVCSettings
{
    internal class Commands : ChatCommand
    {
        public override string[][] Arguments()
        {
            return new string[][] { new string[] { "master", "mastervolume", "mastervolumemultiplier", "mvm", "mastermultiplier", "mm", "playervolume", "pv" } };
        }

        public override string[] CommandAliases()
        {
            return new string[] { "advancedvoicechat", "avc" };
        }

        public override string Description()
        {
            return "Controls subcommands";
        }

        public override void Execute(string arguments)
        {
            string[] args = arguments.ToLower().Split(' ');
            switch (args[0])
            {
                case "master":
                case "mastervolume":
                case "mv":
                    if (args.Length > 1 && float.TryParse(args[1], out float masterVolume))
                    {
                        PLXMLOptionsIO.Instance.CurrentOptions.SetFloatValue("VolumeVoice", masterVolume);
                        Messaging.Notification("Main volume set to: " + Global.VCMainVolume.Value.ToString("000%"));
                    }
                    else
                    {
                        Messaging.Notification("Please input a number. Current Value: " + Global.VCMainVolume.Value.ToString("000%"));
                    }
                    break;
                case "mastervolumemultiplier":
                case "mvm":
                case "mastermultiplier":
                case "mm":
                    if (args.Length > 1 && float.TryParse(args[1], out float masterVolumeMultiplier))
                    {
                        Global.VCMainVolume.Value = masterVolumeMultiplier;
                        Messaging.Notification("Main volume set to: " + Global.VCMainVolume.Value.ToString("000%"));
                    }
                    else
                    {
                        Messaging.Notification("Please input a number. Current Value: " + Global.VCMainVolume.Value.ToString("000%"));
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
                    break;
                default:
                    break;
            }
        }
    }
}
