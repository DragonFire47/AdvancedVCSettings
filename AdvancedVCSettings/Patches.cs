using HarmonyLib;

namespace AdvancedVCSettings
{
    class Patches
    {
        //static PLPhotonVoice
        /*[HarmonyPatch(typeof(PLPhotonVoice), "Update")]
        internal class PVUpdatePatch
        {
            static float PatchMethod(float inVolume)
            {
                return inVolume * Global.VCMainVolume.Value;
            }
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> TargetSequence = new List<CodeInstruction>()
                {
                    new CodeInstruction(OpCodes.Ldstr, "VolumeVoice"),
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(PLOptionsData), "GetFloatValue"))
                };
                List<CodeInstruction> InjectedSequence = new List<CodeInstruction>()
                {
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PVUpdatePatch), "PatchMethod")),

                };
                return HarmonyHelpers.PatchBySequence(instructions, TargetSequence, InjectedSequence, HarmonyHelpers.PatchMode.AFTER, HarmonyHelpers.CheckMode.NONNULL, false);
            }
        }*/
        [HarmonyPatch(typeof(PLPhotonVoice), "Start")]
        class PLPVStartPatch
        {
            static void Postfix(PLPhotonVoice __instance)
            {
                Global.PlayerVolumes.Add(__instance.photonView.owner, 1f);
            }
        }
        [HarmonyPatch(typeof(PhotonVoiceSpeaker), "OnAudioFrame")]
        internal class PVSAudioFramePatch
        {
            static void Prefix(PhotonVoiceSpeaker __instance, ref float[] frame)
            {
                PhotonPlayer player = __instance.photonView.owner;
                if(Global.PlayerVolumes.ContainsKey(player))
                {
                    float playerVolume = Global.PlayerVolumes[player];
                    for (int i = 0; i < frame.Length; i++)
                    {
                        frame[i] *= playerVolume;
                    }
                }
                float value = Global.VCMainVolume.Value;
                for (int i = 0; i < frame.Length; i++)
                {
                    frame[i] *= value;
                }
            }
        }
        [HarmonyPatch(typeof(PLNetworkManager), "OnPhotonPlayerDisconnected")]
        class PlayerDisconnectPatch
        {
            static void Postfix(PhotonPlayer photonPlayer)
            {
                Global.PlayerVolumes.Remove(photonPlayer);
            }
        }
    }
}
