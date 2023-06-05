using PulsarModLoader;
using System.Collections.Generic;
using Steamworks;
using PulsarModLoader.Utilities;

namespace AdvancedVCSettings
{
    class Global
    {
        public static SaveValue<float> VCMainVolume = new SaveValue<float>("MainVolume", 1f);

        public static Dictionary<PhotonPlayer, float> PlayerVolumes = new Dictionary<PhotonPlayer, float>();

        public static SaveValue<Dictionary<ulong, float>> SteamPlayerVolumes = new SaveValue<Dictionary<ulong, float>>("SteamID-PlayerVolume", new Dictionary<ulong, float>());
        
        public static bool TrySetPlayerVolume(PhotonPlayer player, float volume)
        {
            if (PlayerVolumes.ContainsKey(player))
            {
                if (player.SteamID != CSteamID.Nil)
                {
                    SteamPlayerVolumes.Value[player.SteamID.m_SteamID] = volume;
                }

                PlayerVolumes[player] = volume;
                return true;
            }
            return false;
        }
        
        public static void SetVCState(bool Connect)
        {
            if (Connect)
            {
                PLXMLOptionsIO.Instance.CurrentOptions.SetStringValue("VoiceChatEnabled", "1");
                PhotonVoiceNetwork.Client.Reconnect();
            }
            else
            {
                PLXMLOptionsIO.Instance.CurrentOptions.SetStringValue("VoiceChatEnabled", "0");
                PhotonVoiceNetwork.Client.Disconnect();
            }
        }

        public static bool GetVCState()
        {
            return PLXMLOptionsIO.Instance.CurrentOptions.GetStringValue("VoiceChatEnabled") == "1";
        }

        public static bool CanFixVCState()
        {
            return PhotonVoiceNetwork.ClientState == ExitGames.Client.Photon.LoadBalancing.ClientState.Disconnecting || PhotonVoiceNetwork.ClientState == ExitGames.Client.Photon.LoadBalancing.ClientState.Disconnected;
        }

        public static void FixVCState()
        {
            if (PhotonVoiceNetwork.ClientState == ExitGames.Client.Photon.LoadBalancing.ClientState.Disconnecting)
            {
                PhotonVoiceNetwork.Client.OnStateChange(ExitGames.Client.Photon.LoadBalancing.ClientState.Disconnected);
            }
            else if (PhotonVoiceNetwork.ClientState == ExitGames.Client.Photon.LoadBalancing.ClientState.Disconnected)
            {
                PhotonVoiceNetwork.Client.AuthValues = null;
                PhotonVoiceNetwork.Client.Reconnect();
            }
        }

        public static void AttemptFixVCState()
        {
            if (!GetVCState())
            {
                Messaging.Notification("Enable VC before attempting to fix.");
            }
            else if (CanFixVCState())
            {
                Messaging.Notification("Attempting to fix VC");
                FixVCState();
            }
            else
            {
                Messaging.Notification("No fixes available.");
            }
        }
    }
}
