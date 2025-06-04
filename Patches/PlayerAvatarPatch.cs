using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace ReviveUtils.Patches
{
    [HarmonyPatch(typeof(PlayerAvatar))]
    public static class PlayerAvatarPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("ReviveRPC")]
        public static void ExtractionHealPatch(PlayerAvatar __instance, bool _revivedByTruck)
        {
            if (!ConfigManager.EnableExtractionHeal || _revivedByTruck)
            {
                return;
            }

            int healAmount = ConfigManager.ExtractionHealValue;

            if (ConfigManager.EnableExtractionHealPercentage)
            {
                healAmount = __instance.playerHealth.maxHealth * ConfigManager.ExtractionHealPercentage / 100;
            }

            __instance.playerHealth.Heal(healAmount - 1);
            ReviveUtils.Logger.LogInfo($"玩家 {__instance.playerName} 復活，恢復 {healAmount} 血量");
        }

        [HarmonyPrefix]
        [HarmonyPatch("FinalHealRPC")]
        public static bool TruckHealPatch(PlayerAvatar __instance)
        {
            if (!ConfigManager.EnableCustomTruckHeal || __instance.finalHeal || !__instance.isLocal)
            {
                return true;
            }

            int healAmount = ConfigManager.TruckHealValue;

            if (ConfigManager.EnableTruckHealPercentage)
            {
                healAmount = __instance.playerHealth.maxHealth * ConfigManager.TruckHealPercentage / 100;
            }
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                TruckScreenText.instance.MessageSendCustom(
                    "",
                    __instance.playerName + " {arrowright}{truck}{check}\n {point}{shades}{pointright}<b><color=#00FF00>+" + healAmount + "</color></b>{heart}",
                    0
                );
            }

            __instance.playerHealth.EyeMaterialOverride(PlayerHealth.EyeOverrideState.Green, 2f, 1);
            __instance.playerHealth.Heal(healAmount);
            TruckHealer.instance.Heal(__instance);
            __instance.truckReturn.Play(__instance.PlayerVisionTarget.VisionTransform.position);
            __instance.truckReturnGlobal.Play(__instance.PlayerVisionTarget.VisionTransform.position);
            __instance.playerAvatarVisuals.effectGetIntoTruck.gameObject.SetActive(true);
            __instance.finalHeal = true;
            ReviveUtils.Logger.LogInfo($"玩家 {__instance.playerName} 上車，恢復 {healAmount} 血量");
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch("PlayerDeathRPC")]
        public static void ShopRespawnPatch(PlayerAvatar __instance)
        {
            if (!ConfigManager.EnableShopRespawn || !SemiFunc.IsMultiplayer() || RunManager.instance.levelCurrent != RunManager.instance.levelShop)
            {
                return;
            }

            ReviveUtils.Logger.LogInfo($"玩家 {__instance.playerName} 死於商店中，準備復活...");

            GameObject gameObject = new GameObject("ShopRespawn");
            ShopRespawn shopRespawnComponent = gameObject.AddComponent<ShopRespawn>();

            shopRespawnComponent.PlayerAvatarInstance = __instance;
        }
    }

    internal class ShopRespawn : MonoBehaviour
    {
        public PlayerAvatar PlayerAvatarInstance {  get; set; }

        private void Start()
        {
            StartCoroutine(RespawnPlayer());
        }

        private IEnumerator RespawnPlayer()
        {
            yield return new WaitForSeconds(ConfigManager.ShopRespawnDelay);

            if (PlayerAvatarInstance.playerDeathHead != null)
            {
                PlayerAvatarInstance.Revive();
                ReviveUtils.Logger.LogInfo($"復活玩家 {PlayerAvatarInstance.playerName}");
            }
            else
            {
                ReviveUtils.Logger.LogInfo($"取消復活，玩家 {PlayerAvatarInstance.playerName} 已復活或未死亡");
            }

            Object.Destroy(base.gameObject);
        }
    }
}
