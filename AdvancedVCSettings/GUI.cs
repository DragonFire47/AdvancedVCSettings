using PulsarModLoader.CustomGUI;
using PulsarModLoader.Utilities;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace AdvancedVCSettings
{
    internal class GUI : ModSettingsMenu
    {
        bool VCEnabled;

        float MasterVolume = 1f;
        float CachedMasterVolume = 1f;

        float MasterVolumeMultiplier = 1f;
        float CachedMasterVolumeMultiplier = 1f;

        float AssignedPlayerVolume = 1f;
        float CachedPlayerVolume = 1f;
        PhotonPlayer ManagedPPlayer = null;
        PLPlayer ManagedPlayer = null;

        GUILayoutOption[] NumberLabelSetting = new GUILayoutOption[] { MaxWidth(100f) };

        private void LoadPlayerSettings()
        {
            if (ManagedPPlayer != null && Global.PlayerVolumes.ContainsKey(ManagedPPlayer))
            {
                AssignedPlayerVolume = Global.PlayerVolumes[ManagedPPlayer];
                CachedPlayerVolume = AssignedPlayerVolume;
            }
        }

        public override void OnOpen()
        {
            VCEnabled = Global.GetVCState();
        }

        public override void Draw()
        {
            BeginHorizontal();
            {
                if (Button("Voice Chat: " + (VCEnabled ? "Enabled" : "Disabled")))
                {
                    VCEnabled = !VCEnabled;
                    Global.SetVCState(VCEnabled);
                }

                if (Button("Fix VC", NumberLabelSetting))
                {
                    Global.AttemptFixVCState();
                }
            }
            EndHorizontal();
            Label("VC State: " + PhotonVoiceNetwork.ClientState.ToString());

            //MV Slider
            Label("Master Volume");

            BeginHorizontal();
            {
                MasterVolume = HorizontalSlider(MasterVolume, 0f, 1f);
                Label(MasterVolume.ToString("000%"), NumberLabelSetting);
            }
            EndHorizontal();

            if (CachedMasterVolume != MasterVolume)
            {
                CachedMasterVolume = MasterVolume;
                PLXMLOptionsIO.Instance.CurrentOptions.SetFloatValue("VolumeVoice", MasterVolume);
            }



            //MVM Slider
            Label("Master Volume Multiplier");

            BeginHorizontal();
            {
                MasterVolumeMultiplier = HorizontalSlider(MasterVolumeMultiplier, 0f, 10f);
                Label(MasterVolumeMultiplier.ToString("0.#x"), NumberLabelSetting);
            }
            EndHorizontal();

            if (CachedMasterVolumeMultiplier != MasterVolumeMultiplier)
            {
                CachedMasterVolumeMultiplier = MasterVolumeMultiplier;
                Global.VCMainVolume.Value = MasterVolumeMultiplier;
            }


            //Manage selected player settings
            Label("Player Volume");
            BeginHorizontal();
            {
                if (Button("<"))
                {
                    PhotonPlayer LastPPlayer = null;
                    foreach (PhotonPlayer pplayer in Global.PlayerVolumes.Keys)
                    {
                        if (pplayer.IsLocal) //Ignore Local Player
                        {
                            continue;
                        }

                        if (ManagedPPlayer == pplayer)
                        {
                            ManagedPPlayer = LastPPlayer;
                            LoadPlayerSettings();
                            break;
                        }

                        LastPPlayer = pplayer;
                    }
                }
                if (Button(">"))
                {
                    bool passedCurrentPPLayer = false;
                    foreach (PhotonPlayer pplayer in Global.PlayerVolumes.Keys)
                    {
                        if (pplayer.IsLocal) //Ignore Local Player
                        {
                            continue;
                        }

                        if (ManagedPPlayer == null || passedCurrentPPLayer)
                        {
                            ManagedPPlayer = pplayer;
                            LoadPlayerSettings();
                            break;
                        }
                        if (pplayer == ManagedPPlayer)
                        {
                            passedCurrentPPLayer = true;
                        }
                    }
                }
            }
            EndHorizontal();


            //Find PLPlayer
            bool foundManagedPPlayer = false;
            foreach (PhotonPlayer pPlayer in Global.PlayerVolumes.Keys)
            {
                if (pPlayer == ManagedPPlayer)
                {
                    foundManagedPPlayer = true;
                    PLPlayer player = PLServer.GetPlayerForPhotonPlayer(pPlayer);
                    if (player != null)
                    {
                        ManagedPlayer = player;
                    }
                }
            }
            if (!foundManagedPPlayer)
            {
                ManagedPPlayer = null;
                ManagedPlayer = null;
            }


            //Player settings block.
            if (ManagedPlayer != null)
            {
                BeginVertical();
                {
                    Label("Editing volume of: " + ManagedPlayer.GetPlayerName());

                    BeginHorizontal();
                    {
                        AssignedPlayerVolume = HorizontalSlider(AssignedPlayerVolume, 0f, 5f);
                        Label(AssignedPlayerVolume.ToString("0.#x"), NumberLabelSetting);
                    }
                    EndHorizontal();
                }
                EndVertical();

                if (AssignedPlayerVolume != CachedPlayerVolume)
                {
                    if (Global.TrySetPlayerVolume(ManagedPPlayer, AssignedPlayerVolume))
                    {
                        CachedPlayerVolume = AssignedPlayerVolume;
                    }
                    else
                    {
                        Messaging.Notification("Player volume does not exist! This player's volume cannot be modified.");
                        PulsarModLoader.Utilities.Logger.Info("Player volume does not exist!");
                        return;
                    }
                }
            }
            else
            {
                Label("No player selected");
            }
        }

        public override string Name()
        {
            return "Advanced VC Settings";
        }
    }
}
