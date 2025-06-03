using HarmonyLib;

namespace ReviveUtils.Patches
{
    [HarmonyPatch(typeof(PlayerController))]
    public static class PlayerControllerPatch
    {
        public static PlayerDeathHead? grabbedHead {  get; private set; }

        [HarmonyPostfix]
        [HarmonyPatch("FixedUpdate")]
        public static void FixedUpdatePostfix(PlayerController __instance)
        {
            if (!ConfigManager.EnableSacrificialRevive || !SemiFunc.IsMultiplayer() || __instance == null)
            {
                return;
            }


            if (__instance.physGrabObject != null && __instance.physGrabActive)
            {
                PlayerDeathHead head = __instance.physGrabObject.GetComponent<PlayerDeathHead>();

                if (head != null && head.playerAvatar != null)
                {
                    grabbedHead = head;
                }
            }
            else if (grabbedHead != null)
            {
                grabbedHead = null;
            }
        }
    }
}
