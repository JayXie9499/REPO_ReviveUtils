using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;
using ReviveUtils.Patches;

namespace ReviveUtils
{
    [BepInPlugin("jaydev.ReviveUtils", "ReviveUtils", "1.1.0")]
    public class ReviveUtils : BaseUnityPlugin
    {
        internal static ReviveUtils Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger => Instance._logger;
        private ManualLogSource _logger => base.Logger;
        internal Harmony? Harmony { get; set; }

        private void Awake()
        {
            Instance = this;
            this.gameObject.transform.parent = null;
            this.gameObject.hideFlags = HideFlags.HideAndDontSave;

            ConfigManager.Init();
            Patch();
            Logger.LogInfo("模組載入完成！");
        }

        internal void Patch()
        {
            Harmony ??= new Harmony(Info.Metadata.GUID);
            Harmony.PatchAll(typeof(PlayerAvatarPatch));
            Harmony.PatchAll(typeof(PlayerControllerPatch));
            Harmony.PatchAll(typeof(PlayerDeathHeadPatch));
            Harmony.PatchAll(typeof(ShopManagerPatch));
        }

        internal void Unpatch()
        {
            Harmony?.UnpatchSelf();
        }

        private void Update()
        {
        }
    }

    public static class ConfigManager
    {
        private readonly static ConfigFile configFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "ReviveUtils.cfg"), true);
        private static ConfigEntry<bool> _enableShopRespawn;
        private static ConfigEntry<int> _shopRespawnDelay;
        private static ConfigEntry<bool> _enableExtractionHeal;
        private static ConfigEntry<int> _extractionHealValue;
        private static ConfigEntry<bool> _enableExtractionHealPercentage;
        private static ConfigEntry<int> _extractionHealPercentage;
        private static ConfigEntry<bool> _enableCustomTruckHeal;
        private static ConfigEntry<int> _truckHealValue;
        private static ConfigEntry<bool> _enableTruckHealPercentage;
        private static ConfigEntry<int> _truckHealPercentage;
        private static ConfigEntry<bool> _enableSacrificialRevive;
        private static ConfigEntry<int> _sacrificialReviveHpCost;
        private static ConfigEntry<string> _sacrificialReviveKeybind;
        private static ConfigEntry<bool> _enablePointRevive;

        public static bool EnableShopRespawn
        {
            get { return _enableShopRespawn.Value; }
        }

        public static int ShopRespawnDelay
        {
            get { return _shopRespawnDelay.Value; }
        }

        public static bool EnableExtractionHeal
        {
            get { return _enableExtractionHeal.Value; }
        }

        public static int ExtractionHealValue
        {
            get { return _extractionHealValue.Value; }
        }

        public static bool EnableExtractionHealPercentage
        {
            get { return _enableExtractionHealPercentage.Value; }
        }

        public static int ExtractionHealPercentage
        {
            get { return _extractionHealPercentage.Value; }
        }

        public static bool EnableCustomTruckHeal
        {
            get { return _enableCustomTruckHeal.Value; }
        }

        public static int TruckHealValue
        {
            get { return _truckHealValue.Value; }
        }

        public static bool EnableTruckHealPercentage
        {
            get { return _enableTruckHealPercentage.Value; }
        }

        public static int TruckHealPercentage
        {
            get { return _truckHealPercentage.Value; }
        }

        public static bool EnableSacrificialRevive
        {
            get { return _enableSacrificialRevive.Value; }
        }

        public static int SacrificialReviveHpCost
        {
            get { return _sacrificialReviveHpCost.Value; }
        }

        public static string SacrificialReviveKeybind
        {
            get { return _sacrificialReviveKeybind.Value; }
        }

        public static bool EnablePointRevive
        {
            get { return _enablePointRevive.Value; }
        }

        public static void Init()
        {
            _enableShopRespawn = configFile.Bind(
                "Shop Respawn",
                "Enable",
                true,
                "Enable shop respawn"
            );
            _shopRespawnDelay = configFile.Bind(
                "Shop Respawn",
                "Respawn Delay",
                5,
                new ConfigDescription("Respawn delay in seconds", new AcceptableValueRange<int>(0, 30))
            );
            _enableExtractionHeal = configFile.Bind(
                "Extraction Heal",
                "Enable",
                true,
                "Enable extraction healing"
            );
            _extractionHealValue = configFile.Bind(
                "Extraction Heal",
                "Heal Value",
                50,
                "Extraction healing value"
            );
            _enableExtractionHealPercentage = configFile.Bind(
                "Extraction Heal",
                "Enable Percentage Healing",
                false,
                "Whether to heal player by the percentage of their max hp"
            );
            _extractionHealPercentage = configFile.Bind(
                "Extraction Heal",
                "Heal Percentage",
                50,
                new ConfigDescription("Extraction healing percentage", new AcceptableValueRange<int>(0, 100))
            );
            _enableCustomTruckHeal = configFile.Bind(
                "Truck Heal",
                "Enable",
                true,
                "Enable custom truck healing value"
            );
            _truckHealValue = configFile.Bind(
                "Truck Heal",
                "Heal Value",
                50,
                "Truck healing value"
            );
            _enableTruckHealPercentage = configFile.Bind(
                "Truck Heal",
                "Enable Percentage Healing",
                false,
                "Whether to heal player by the percentage of their max hp"
            );
            _truckHealPercentage = configFile.Bind(
                "Truck Heal",
                "Heal Percentage",
                50,
                new ConfigDescription("Truck healing percentage", new AcceptableValueRange<int>(0, 100))
            );
            _enableSacrificialRevive = configFile.Bind(
                "Sacrificial Revive",
                "Enable",
                true,
                "Enable sacrificial revive"
            );
            _sacrificialReviveHpCost = configFile.Bind(
                "Sacrificial Revive",
                "Health Cost",
                10,
                "Amount of health to spend on reviving"
            );
            _sacrificialReviveKeybind = configFile.Bind(
                "Sacrificial Revive",
                "Keybind",
                "R",
                "Keybind for sacrificial revive"
            );
            _enablePointRevive = configFile.Bind(
                "Extraction Point Revive",
                "Enable",
                false,
                "Enable instant revive on extraction point"
            );
        }
    }

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