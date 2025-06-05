using UnityEngine;
using UnityEngine.InputSystem;
using ReviveUtils.Patches;

namespace ReviveUtils.Components
{
    public class SacrificialRevive : MonoBehaviour
    {
        private void Update()
        {
            PlayerDeathHead? head = PlayerControllerPatch.grabbedHead;
            PlayerAvatar playerAvatar = PlayerController.instance.playerAvatarScript;
            int hpCost = ConfigManager.SacrificialReviveHpCost;

            if (!Keyboard.current[ConfigManager.SacrificialReviveKeybind].IsPressed() || head == null || playerAvatar.playerHealth.health < hpCost)
            {
                return;
            }

            head.playerAvatar.Revive();
            playerAvatar.playerHealth.Hurt(hpCost, false, -1);
            ReviveUtils.Logger.LogInfo($"復活 {head.playerAvatar.playerName}");
        }
    }
}