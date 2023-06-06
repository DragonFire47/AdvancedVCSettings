using PulsarModLoader;
using PulsarModLoader.Utilities;
using System.Collections.Generic;

namespace AdvancedVCSettings
{
    class Global
    {
        public static SaveValue<float> VCMainVolume = new SaveValue<float>("MainVolume", 1f);
        public static SaveValue<float> nonPrioritySpeakerVolume = new SaveValue<float>("nonPrioritySpeakerVolume", 0.2f);
        public static SaveValue<Dictionary<ulong, float>> SteamPlayerVolumes = new SaveValue<Dictionary<ulong, float>>("SteamID-PlayerVolume", new Dictionary<ulong, float>());
        public static SaveValue<List<ulong>> MutedSteamPlayers = new SaveValue<List<ulong>>("SteamID-Muted", new List<ulong>());

        public static Dictionary<PhotonPlayer, PlayerVCSettings> PlayerData = new Dictionary<PhotonPlayer, PlayerVCSettings>();
       
        
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
                //Not sure if anything is broken to the point of needing this.
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
