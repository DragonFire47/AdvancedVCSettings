using PulsarModLoader;
using System.Collections.Generic;
using Steamworks;

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
    }
}
