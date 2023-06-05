using PulsarModLoader;

namespace AdvancedVCSettings
{
    public class Mod : PulsarMod
    {
        public override string Version => "1.1.0";

        public override string Author => "Dragon";

        public override string Name => "AdvancedVCSettings";

        public override string LongDescription => "Features:\r\n- Allows VC to be enabled in-game.\r\n- Provides control over individual player volume in the existing voice chat system.\r\n- Provides extra control on overall voice chat volume.\r\n- Saves set player volume based on Steam IDs.";

        public override string HarmonyIdentifier()
        {
            return $"{Author}.{Name}";
        }
    }
}
