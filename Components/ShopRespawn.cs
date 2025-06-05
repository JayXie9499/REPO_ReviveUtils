using System.Collections;
using UnityEngine;

namespace ReviveUtils.Components
{
    public class ShopRespawn : MonoBehaviour
    {
        public PlayerAvatar PlayerAvatarInstance { get; set; }

        private void Start()
        {
            StartCoroutine(RespawnPlayer());
        }

        private IEnumerator RespawnPlayer()
        {
            yield return new WaitForSeconds(ConfigManager.ShopRespawnDelay);

            if (LevelGenerator.Instance == null)
            {
                ReviveUtils.Logger.LogWarning("LevelGenerator不存在");
            }
            else if (PlayerAvatarInstance.deadSet)
            {
                Vector3 position = Object.FindObjectOfType<SpawnPoint>().transform.position;
                Quaternion rotation = PlayerAvatarInstance.playerDeathHead.transform.rotation;

                PlayerAvatarInstance.Revive();
                PlayerAvatarInstance.Spawn(position, rotation);
                ReviveUtils.Logger.LogInfo($"復活玩家 {PlayerAvatarInstance.playerName}");
            }
            else
            {
                ReviveUtils.Logger.LogInfo($"取消復活，玩家 {PlayerAvatarInstance.playerName} 已復活或未死亡");
            }

            Object.Destroy(gameObject);
        }
    }
}