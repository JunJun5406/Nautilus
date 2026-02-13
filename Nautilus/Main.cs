using BepInEx;
using R2API.Utils;
using RiskOfOptions;
using Nautilus.Configuration;
using RoR2;
using RoR2.ExpansionManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using R2API;
using Nautilus.Items;
using Nautilus.Interactables;
using System.Collections.Generic;
using System.Reflection;
using RiskOfOptions;
using RiskOfOptions.Options;
using BepInEx.Configuration;
using System;
// using ShaderSwapper;

namespace Nautilus
{
    [BepInPlugin(NAUTILUS_GUID, NAUTILUS_NAME, NAUTILUS_VER)]
    [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.items", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.language", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.recalculatestats", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.prefab", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.director", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.HardDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string NAUTILUS_GUID = "com.Hex3.Nautilus";
        public const string NAUTILUS_NAME = "Nautilus";
        public const string NAUTILUS_VER = "1.2.1";
        public static Main Instance;
        public static ExpansionDef Expansion;
        public static AssetBundle Assets;
        public static ItemRelationshipProvider ItemRelationshipProvider = new();
        public static List<ItemDef.Pair> ItemConversionList = new();
        public static ConfigEntry<bool> Config_Enabled;

        public void Awake()
        {
            Log.Init(Logger);
            Log.Info($"Init {NAUTILUS_NAME} {NAUTILUS_VER}");

            Instance = this;

            Log.Info("Creating assets...");
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Nautilus.nautilusvfx"))
            {
                Assets = AssetBundle.LoadFromStream(stream);
            }
            // base.StartCoroutine(Assets.UpgradeStubbedShadersAsync());

            Log.Info($"Creating config...");
            Config_Enabled = Instance.Config.Bind(new ConfigDefinition("CONFIG - IMPORTANT", "Enable custom config"), false, new ConfigDescription("Set to 'true' to enable custom configuration for this mod. False by default to allow balance changes to take effect.", null, Array.Empty<object>()));
            if (Compat.RiskOfOptions)
            {
                Log.Info($"Detected RiskOfOptions");
                ModSettingsManager.SetModDescription("Adds new void counterparts for vanilla items.");
                ModSettingsManager.SetModIcon(Assets.LoadAsset<Sprite>("Assets/icons/expansion.png"));
                ModSettingsManager.AddOption
                (
                    new CheckBoxOption
                    (
                        Config_Enabled,
                        true
                    )
                );
            }

            Log.Info($"Creating expansion...");
            Expansion = ScriptableObject.CreateInstance<ExpansionDef>();
            Expansion.name = NAUTILUS_NAME;
            Expansion.nameToken = "NT_EXPANSION_NAME";
            Expansion.descriptionToken = "NT_EXPANSION_DESC";
            Expansion.iconSprite = Assets.LoadAsset<Sprite>("Assets/icons/expansion.png");
            Expansion.disabledIconSprite = Assets.LoadAsset<Sprite>("Assets/icons/expansion-inactive.png");
            Expansion.requiredEntitlement = null;
            ContentAddition.AddExpansionDef(Expansion);

            Log.Info($"Creating items...");
            ItemInit.Init();

            Log.Info($"Creating interactables...");
            InteractableInit.Init();

            Log.Info($"Creating void conversions...");
            ItemRelationshipProvider = ScriptableObject.CreateInstance<ItemRelationshipProvider>();
            ItemRelationshipProvider.name = "NT_ITEMRELATIONSHIPPROVIDER";
            ItemRelationshipProvider.relationshipType = Addressables.LoadAssetAsync<ItemRelationshipType>("RoR2/DLC1/Common/ContagiousItem.asset").WaitForCompletion();
            ItemRelationshipProvider.relationships = ItemConversionList.ToArray();
            ContentAddition.AddItemRelationshipProvider(ItemRelationshipProvider);

            On.RoR2.RoR2Application.OnMainMenuControllerInitialized += (orig, self) =>
            {
                ItemInit.FormatDescriptions();
                orig(self);
            };

            Log.Info($"Done");
        }
    }
}
