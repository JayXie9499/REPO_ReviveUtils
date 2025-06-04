using HarmonyLib;

namespace ReviveUtils.Patches
{
    [HarmonyPatch(typeof(PlayerDeathHead))]
    public static class PlayerDeathHeadPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        public static void InstantRevivePatch(PlayerDeathHead __instance)
        {
            if (!ConfigManager.EnablePointRevive || !SemiFunc.IsMasterClient())
            {
                return;
            }
            if (__instance.inExtractionPoint)
            {
                __instance.playerAvatar.Revive();
                ReviveUtils.Logger.LogInfo($"點上復活 {__instance.playerAvatar.playerName}");
            }
        }
    }
}
