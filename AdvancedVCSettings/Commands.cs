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
            return new string[][] { new string[] { "main", "mainvolume" } };
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
                case "main":
                case "mainvolume":
                    if (args.Length > 1 && float.TryParse(args[1], out float mainVolume))
                    {
                        Global.VCMainVolume.Value = mainVolume;
                        Messaging.Notification("Main volume set to: " + Global.VCMainVolume.Value.ToString("000%"));
                    }
                    else
                    {
                        Messaging.Notification("Please input a number. Current Value: " + Global.VCMainVolume.Value.ToString("000%"));
                    }
                    break;
                case "dbg":
                    Dictionary<KeyValuePair<int, byte>, PhotonVoiceSpeaker> voices = (Dictionary<KeyValuePair<int, byte>, PhotonVoiceSpeaker>)AccessTools.Field(typeof(UnityVoiceFrontend), "voiceSpeakers").GetValue(PhotonVoiceNetwork.Client);
                    foreach(KeyValuePair<KeyValuePair<int, byte>, PhotonVoiceSpeaker> voice in voices)
                    {
                        Logger.Info($"AVCS Debug P1: Player: {voice.Key.Key}, VoiceID: {voice.Key.Value}, ViewID: {voice.Value.photonView.viewID}");
                    }
                    foreach(PLPlayer player in PLServer.Instance.AllPlayers)
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
