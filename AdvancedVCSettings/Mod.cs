using PulsarModLoader;

namespace AdvancedVCSettings
{
    public class Mod : PulsarMod
    {
        public override string Version => "0.1.0";

        public override string Author => "Dragon";

        public override string Name => "AdvancedVCSettings";

        public override string HarmonyIdentifier()
        {
            return $"{Author}.{Name}";
        }
    }
}
