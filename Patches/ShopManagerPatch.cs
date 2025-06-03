using HarmonyLib;

namespace ReviveUtils.Patches
{
    [HarmonyPatch(typeof(ShopManager))]
    public static class ShopManagerPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Awake")]
        public static void ChangeLevelPostfix(ShopManager __instance)
        {
            if (!ConfigManager.EnableSacrificialRevive || !SemiFunc.IsMultiplayer())
            {
                return;
            }
            if (__instance != null && __instance.gameObject.GetComponent<SacrificialRevive>() == null)
            {
                __instance.gameObject.AddComponent<SacrificialRevive>();
            }
        }
    }
}
