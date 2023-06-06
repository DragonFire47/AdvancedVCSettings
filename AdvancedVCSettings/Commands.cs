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
            return new string[][] { new string[] { "vctoggle", "vct", "master", "mastervolume", "mastervolumemultiplier", "mvm", "mastermultiplier", "mm", "playervolume", "pv", "priorityspeaker", "ps", "mute", "fix" } };
        }

        public override string[] CommandAliases()
        {
            return new string[] { "advancedvoicechat", "avc" };
        }

        public override string Description()
        {
            return "Controls subcommands for advanced voice chat. VCT - Toggle VC on/off, MV [Value] - Set Main Volume, MVM [Value] - Set Main Volume Multiplier, " +
                "PV [Player] [Value] - Set Player Volume, FIX - Attempts to fix VC, PS [Player] - Set player as priority speaker, MUTE [Player] - Mute or unmute player.";
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
                        if (Global.PlayerData.TryGetValue(PPlayer, out PlayerVCSettings value))
                        {
                            value.PlayerVolume = playerVolume;
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
                        if (Global.PlayerData.TryGetValue(PPlayer, out PlayerVCSettings value))
                        {
                            Messaging.Notification($"Volume of {PVPlayer.GetPlayerName()} is currently set to {value.PlayerVolume.ToString("0.#x")}");
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
                case "priorityspeaker":
                case "ps":
                    if(args.Length == 1)
                    {
                        string speakers = string.Empty;
                        foreach(PlayerVCSettings PVCS in Global.PlayerData.Values)
                        {
                            if (PVCS != null && PVCS.Player != null)
                            {
                                //Takes care of commas
                                if(speakers != string.Empty)
                                {
                                    speakers += ", ";
                                }

                                //add player to speaker list
                                speakers += PVCS.Player.GetPlayerName();
                            }
                        }
                        Messaging.Notification("Format: /avc ps [player]; Priority speakers: " + speakers);
                    }
                    if (args.Length > 1)
                    {
                        PLPlayer player = HelperMethods.GetPlayer(args[1]);
                        PhotonPlayer pplayer = player.GetPhotonPlayer();
                        if (player != null && pplayer != null)
                        {
                            bool PSSetting = Global.PlayerData[pplayer].IsPrioritySpeaker;
                            Global.PlayerData[pplayer].IsPrioritySpeaker = !PSSetting;

                            Messaging.Notification($"{player.GetPlayerName()} is {(PSSetting ? "now" : "no longer")} a priority speaker" );
                        }
                        else
                        {
                            Messaging.Notification("Player not detected. Format: /avc ps [player]");
                        }
                    }
                    break;
                case "mute":
                    if (args.Length > 1)
                    {
                        PLPlayer player = HelperMethods.GetPlayer(args[1]);
                        PhotonPlayer pplayer = player.GetPhotonPlayer();
                        if (player != null && pplayer != null)
                        {
                            Global.PlayerData[pplayer].Muted = player.TS_IsMuted;

                            Messaging.Notification($"{player.GetPlayerName()} is now {(player.TS_IsMuted ? "muted" : "unmuted")}");
                        }
                    }

                    Messaging.Notification("No player detected. Format: /avc mute [player]");
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
