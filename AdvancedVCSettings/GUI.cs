using PulsarModLoader.CustomGUI;
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

        float NonPrioritySpeakerVolume = 0.2f;
        float CachedNonPrioritySpeakerVolume = 0.2f;

        float AssignedPlayerVolume = 1f;
        float CachedPlayerVolume = 1f;

        bool PrioritySpeaker = false;
        bool CachedPrioritySpeaker = false;

        bool Muted = false;
        bool CachedMuted = false;

        PhotonPlayer ManagedPPlayer = null;
        PlayerVCSettings ManagedPlayerData = null;

        GUILayoutOption[] NumberLabelSetting = new GUILayoutOption[] { MaxWidth(100f) };

        private void LoadPlayerSettings()
        {
            if (ManagedPPlayer != null && Global.PlayerData.TryGetValue(ManagedPPlayer, out PlayerVCSettings PVCS))
            {
                AssignedPlayerVolume = PVCS.PlayerVolume;
                CachedPlayerVolume = AssignedPlayerVolume;

                PrioritySpeaker = PVCS.IsPrioritySpeaker;
                CachedPrioritySpeaker = PrioritySpeaker;

                Muted = PVCS.Muted;
                CachedMuted = Muted;
            }
        }

        public override void OnOpen()
        {
            MasterVolume = PLXMLOptionsIO.Instance.CurrentOptions.GetFloatValue("VolumeVoice");
            CachedMasterVolume = MasterVolume;

            MasterVolumeMultiplier = Global.VCMainVolume.Value;
            CachedMasterVolumeMultiplier = MasterVolumeMultiplier;

            NonPrioritySpeakerVolume = Global.nonPrioritySpeakerVolume.Value;
            CachedNonPrioritySpeakerVolume = NonPrioritySpeakerVolume;

            VCEnabled = Global.GetVCState();
        }

        public override void Draw()
        {
            BeginHorizontal();
            {
                if (Button((VCEnabled ? "Enable" : "Disable") + " Voice Chat"))
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


            //Non-Priority Speaker Volume
            Label("Non-Priority Speaker Volume");

            BeginHorizontal();
            {
                NonPrioritySpeakerVolume = HorizontalSlider(NonPrioritySpeakerVolume, 0f, 1f);
                Label(NonPrioritySpeakerVolume.ToString("000%"), NumberLabelSetting);
            }
            EndHorizontal();

            if (CachedNonPrioritySpeakerVolume != NonPrioritySpeakerVolume)
            {
                CachedNonPrioritySpeakerVolume = NonPrioritySpeakerVolume;
                Global.nonPrioritySpeakerVolume.Value = NonPrioritySpeakerVolume;
            }



            //Manage selected player settings
            Label("Player Volume");
            BeginHorizontal();
            {
                if (Button("<"))
                {
                    PhotonPlayer LastPPlayer = null;
                    foreach (PhotonPlayer pplayer in Global.PlayerData.Keys)
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
                    foreach (PhotonPlayer pplayer in Global.PlayerData.Keys)
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

            //Clear PPLayer if null
            if (ManagedPPlayer != null && !Global.PlayerData.ContainsKey(ManagedPPlayer))
            {
                ManagedPPlayer = null;
                ManagedPlayerData = null;
            }

            //Load ManagedPlayerData
            if (ManagedPPlayer != null)
            {
                ManagedPlayerData = Global.PlayerData[ManagedPPlayer];
            }

            //Player settings block.
            if (ManagedPPlayer != null)
            {
                BeginVertical();
                {
                    Label("Editing volume of: " + ManagedPlayerData.Player.GetPlayerName());

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
                    CachedPlayerVolume = AssignedPlayerVolume;
                    ManagedPlayerData.PlayerVolume = AssignedPlayerVolume;
                }

                PrioritySpeaker = Toggle(PrioritySpeaker, "Priority Speaker");

                if (PrioritySpeaker != CachedPrioritySpeaker)
                {
                    CachedPrioritySpeaker = PrioritySpeaker;
                    ManagedPlayerData.IsPrioritySpeaker = PrioritySpeaker;
                }

                Muted = Toggle(Muted, "Mute player");

                if (Muted != CachedMuted)
                {
                    CachedMuted = Muted;
                    ManagedPlayerData.Muted = Muted;
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
