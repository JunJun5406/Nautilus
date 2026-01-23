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
using System.Collections.Generic;

namespace Nautilus
{
    [BepInPlugin(NAUTILUS_GUID, NAUTILUS_NAME, NAUTILUS_VER)]
    [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.items", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.language", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string NAUTILUS_GUID = "com.Hex3.Nautilus";
        public const string NAUTILUS_NAME = "Nautilus";
        public const string NAUTILUS_VER = "1.0.0";
        public static Main Instance;
        public static ExpansionDef Expansion;
        public static AssetBundle Assets;
        public static List<ItemBase> ItemList = new();
        public static List<ItemDef.Pair> ItemConversionList = new();

        public void Awake()
        {
            Log.Init(Logger);
            Log.Info($"Init {NAUTILUS_NAME} {NAUTILUS_VER}");

            Instance = this;

            Log.Info($"Creating config...");
            if (Compat.RiskOfOptions)
            {
                Log.Info($"Detected RiskOfOptions");
                ModSettingsManager.SetModDescription("Adds new void counterparts for vanilla items.");
                // ModSettingsManager.SetModIcon(MainAssets.LoadAsset<Sprite>("Assets/VFXPASS3/Icons/icon.png"));
            }

            Log.Info($"Creating expansion...");
            Expansion = ScriptableObject.CreateInstance<ExpansionDef>();
            Expansion.name = NAUTILUS_NAME;
            Expansion.nameToken = "NT_EXPANSION_NAME";
            Expansion.descriptionToken = "NT_EXPANSION_DESC";
            Expansion.iconSprite = null; // MainAssets.LoadAsset<Sprite>("Assets/VFXPASS3/Icons/expansion.png");
            Expansion.disabledIconSprite = null; // Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texUnlockIcon.png").WaitForCompletion();
            Expansion.requiredEntitlement = null;
            ContentAddition.AddExpansionDef(Expansion);

            Log.Info($"Creating items...");
            ItemInit.Init();

            ItemRelationshipProvider itemRelationshipProvider = ScriptableObject.CreateInstance<ItemRelationshipProvider>();
            itemRelationshipProvider.name = "NT_ITEMRELATIONSHIPPROVIDER";
            itemRelationshipProvider.relationshipType = Addressables.LoadAssetAsync<ItemRelationshipType>("RoR2/DLC1/Common/ContagiousItem.asset").WaitForCompletion();
            itemRelationshipProvider.relationships = ItemConversionList.ToArray();
            ContentAddition.AddItemRelationshipProvider(itemRelationshipProvider);

            On.RoR2.RoR2Application.OnMainMenuControllerInitialized += (orig, self) =>
            {
                ItemInit.FormatDescriptions();
                orig(self);
            };

            Log.Info($"Done");
        }
    }
}
