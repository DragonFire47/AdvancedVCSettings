using PulsarModLoader;
using System.Collections.Generic;

namespace AdvancedVCSettings
{
    class Global
    {
        public static SaveValue<float> VCMainVolume = new SaveValue<float>("MainVolume", 1f);

        public static Dictionary<PhotonPlayer, float> PlayerVolumes = new Dictionary<PhotonPlayer, float>();
    }
}
