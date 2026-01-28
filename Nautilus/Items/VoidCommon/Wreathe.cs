using RoR2;
using Nautilus.Configuration;
using System;
using R2API;
using UnityEngine;
using UnityEngine.AddressableAssets;
using HarmonyLib;
using System.Threading;
using UnityEngine.Networking;
/*
namespace Nautilus.Items
{
    public static partial class ItemInit
    {
        public static Wreathe Wreathe = new Wreathe
        (
            "VoidWatch",
            [ItemTag.Damage],
            ItemTier.VoidTier1
        );
    }

    /// <summary>
    ///     // Ver.1
    ///     Coral Wreathe gives you significant benefits for one common item, but doesn't stack well if you don't have many allies
    ///     Helps boost a large amount of stats for cheap, also provides a unique way to boost the crit of your drones and other allies
    /// </summary>
    public class Wreathe : ItemBase
    {
        public override bool Enabled => Wreathe_Enabled.Value;
        public override ItemDef ConversionItemDef => Addressables.LoadAssetAsync<ItemDef>("RoR2/DLC2/Items/AttackSpeedPerNearbyAllyOrEnemy/AttackSpeedPerNearbyAllyOrEnemy.asset").WaitForCompletion();
        public BuffDef WreatheBuff;

        public Wreathe(string _name, ItemTag[] _tags, ItemTier _tier, bool _canRemove = true, bool _isConsumed = false, bool _hidden = false) : 
        base(_name, _tags, _tier, _canRemove, _isConsumed, _hidden){}

        // Config
        public static ConfigItem<bool> Wreathe_Enabled = new ConfigItem<bool>
        (
            "Void common: Coral Wreathe",
            "Item enabled",
            "Should this item appear in runs?",
            true
        );
        public static ConfigItem<int> Wreathe_MaxLinks = new ConfigItem<int>
        (
            "Void common: Coral Wreathe",
            "Ally links",
            "Number of allies that can link and receive the buff.",
            1,
            1f,
            10f,
            1f
        );
        public static ConfigItem<int> Wreathe_MaxLinksStack = new ConfigItem<int>
        (
            "Void common: Coral Wreathe",
            "Ally links (Per stack)",
            "Number of allies that can link and receive the buff per additional stack.",
            1,
            1f,
            10f,
            1f
        );
        public static ConfigItem<float> Wreathe_BuffMovement = new ConfigItem<float>
        (
            "Void common: Coral Wreathe",
            "Speed boost",
            "Grants a speed boost with this multiplier.",
            0.14f,
            0f,
            0.5f,
            0.01f
        );
        public static ConfigItem<float> Wreathe_BuffCrit = new ConfigItem<float>
        (
            "Void common: Coral Wreathe",
            "Crit boost",
            "Grants a critical strike chance boost with this multiplier.",
            0.14f,
            0f,
            0.5f,
            0.01f
        );
        public static ConfigItem<float> Wreathe_BuffAttackSpeed = new ConfigItem<float>
        (
            "Void common: Coral Wreathe",
            "Attack speed boost",
            "Grants an attack speed boost with this multiplier.",
            0.14f,
            0f,
            0.5f,
            0.01f
        );

        public static ConfigItem<float> Wreathe_Radius = new ConfigItem<float>
        (
            "Void common: Coral Wreathe",
            "Link radius",
            "Link to allies within this radius in meters.",
            20f,
            1f,
            40f,
            1f
        );
        public static ConfigItem<float> Wreathe_RadiusStack = new ConfigItem<float>
        (
            "Void common: Coral Wreathe",
            "Link radius (Per stack)",
            "Link to allies within this radius in meters, per additional stack.",
            8f,
            0f,
            40f,
            1f
        );

        // Tokens
        public override void FormatDescriptionTokens()
        {
            string descriptionToken = ItemDef.descriptionToken;

            LanguageAPI.AddOverlay
            (
                descriptionToken,
                String.Format
                (
                    Language.currentLanguage.GetLocalizedStringByToken(descriptionToken),
                    Wreathe_MaxLinks.Value,
                    Wreathe_MaxLinksStack.Value,
                    Wreathe_BuffMovement.Value * 100f,
                    Wreathe_BuffCrit.Value * 100f,
                    Wreathe_BuffAttackSpeed.Value * 100f,
                    Wreathe_Radius,
                    Wreathe_RadiusStack
                )
            );
        }

        // Hooks
        public override void RegisterHooks()
        {
            CreateWreatheBuff();

            // Buff stats
            RecalculateStatsAPI.GetStatCoefficients += (orig, self) =>
            {
                HealthComponent healthComponent = orig.healthComponent;

                if (healthComponent && orig.HasBuff(WreatheBuff))
                {
                    self.moveSpeedMultAdd += Wreathe_BuffMovement.Value;
                    self.critAdd += Wreathe_BuffCrit.Value;
                    self.attackSpeedMultAdd += Wreathe_BuffAttackSpeed.Value;
                }
            };
        }

        public void CreateWreatheBuff()
        {
            BuffDef wreatheBuff = ScriptableObject.CreateInstance<BuffDef>();
            wreatheBuff.buffColor = new Color(1f, 1f, 1f);
            wreatheBuff.canStack = false;
            wreatheBuff.isDebuff = false;
            wreatheBuff.name = "Coral Wreathe buff";
            wreatheBuff.isHidden = false;
            wreatheBuff.isCooldown = false;
            wreatheBuff.iconSprite = null; // LoadBuffSprite();
            ContentAddition.AddBuffDef(wreatheBuff);

            WreatheBuff = wreatheBuff;
        }
    }

    public class WreatheBehavior : NetworkBehaviour
    {
        
    }
}
*/