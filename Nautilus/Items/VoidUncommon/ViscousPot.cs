using RoR2;
using Nautilus.Configuration;
using System;
using R2API;
using UnityEngine;
using UnityEngine.AddressableAssets;
using HarmonyLib;

namespace Nautilus.Items
{
    public static partial class ItemInit
    {
        public static ViscousPot ViscousPot = new ViscousPot
        (
            "ViscousPot",
            [ItemTag.Healing],
            ItemTier.VoidTier2
        );
    }

    /// <summary>
    ///     // Ver.1
    ///     Defensive alternative to Luminous Shot. I wanted an option for reducing barrier decay, and it gives you somewhat of a reason to take it over luminous shot. Boosts void watch as well
    /// </summary>
    public class ViscousPot : ItemBase
    {
        public override bool Enabled => ViscousPot_Enabled.Value;
        public override ItemDef ConversionItemDef => Addressables.LoadAssetAsync<ItemDef>("RoR2/DLC2/Items/IncreasePrimaryDamage/IncreasePrimaryDamage.asset").WaitForCompletion();
        public override GameObject itemPrefab => OverwritePrefabMaterials();
        public Material material0 => Addressables.LoadAssetAsync<Material>("RoR2/DLC1/EliteVoid/matVoidInfestorMetal.mat").WaitForCompletion();
        public Material material1 => Addressables.LoadAssetAsync<Material>("RoR2/DLC1/RegeneratingScrap/matRegeneratingScrapGoo.mat").WaitForCompletion();
        public Material material2 => Addressables.LoadAssetAsync<Material>("RoR2/DLC1/voidstage/matVoidAsteroid.mat").WaitForCompletion();
        public override Sprite itemIcon => Main.Assets.LoadAsset<Sprite>("Assets/icons/viscousPot.png");

        public ViscousPot(string _name, ItemTag[] _tags, ItemTier _tier, bool _canRemove = true, bool _isConsumed = false, bool _hidden = false) : 
        base(_name, _tags, _tier, _canRemove, _isConsumed, _hidden){}

        // Config
        public static ConfigItem<bool> ViscousPot_Enabled = new ConfigItem<bool>
        (
            "Void uncommon: Viscous Pot",
            "Item enabled",
            "Should this item appear in runs?",
            true
        );
        public static ConfigItem<float> ViscousPot_DecayReduction = new ConfigItem<float>
        (
            "Void uncommon: Viscous Pot",
            "Barrier decay reduction",
            "Fraction for barrier decay reduction.",
            0.2f,
            0f,
            1f,
            0.05f
        );
        public static ConfigItem<float> ViscousPot_BarrierAdd = new ConfigItem<float>
        (
            "Void uncommon: Viscous Pot",
            "Barrier on secondary use",
            "Fraction of barrier added on secondary skill use.",
            0.075f,
            0f,
            1f,
            0.05f
        );
        public static ConfigItem<float> ViscousPot_BarrierAddStack = new ConfigItem<float>
        (
            "Void uncommon: Viscous Pot",
            "Barrier on secondary use (per stack)",
            "Fraction of barrier added on secondary skill use per additional stack.",
            0.075f,
            0f,
            1f,
            0.05f
        );

        public GameObject OverwritePrefabMaterials()
        {
            GameObject ret = Main.Assets.LoadAsset<GameObject>("Assets/prefabs/viscousPot.prefab");

            Material[] materials =
            {
                material0,
                material1,
                material2
            };
            ret.GetComponentInChildren<MeshRenderer>().SetMaterialArray(materials);

            return ret;
        }

        
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
                    ViscousPot_DecayReduction.Value * 100f,
                    ViscousPot_BarrierAdd.Value * 100f,
                    ViscousPot_BarrierAddStack.Value * 100f
                )
            );
        }

        // Hooks
        public override void RegisterHooks()
        {
            // Barrier decay reduction
            RecalculateStatsAPI.GetStatCoefficients += (orig, self) =>
            {
                int itemCount = GetItemCountEffective(orig);
                if (itemCount > 0)
                {
                    self.barrierDecayMult *= 1f - ViscousPot_DecayReduction.Value;
                }
            };

            // Barrier on skill
            On.RoR2.CharacterBody.OnSkillActivated += (orig, self, genericSkill) =>
            {
                if (GetItemCountEffective(self) <= 0 || !self.healthComponent)
                {
                    return;
                }

                int itemCount = GetItemCountEffective(self);
                float barrierFraction = ViscousPot_BarrierAdd.Value + (ViscousPot_BarrierAddStack.Value * (itemCount - 1));

                if (self.bodyIndex == BodyCatalog.SpecialCases.RailGunner())
                {
                    if ((object)self.skillLocator.primary == genericSkill && self.canAddIncrasePrimaryDamage)
                    {
                        self.healthComponent.AddBarrier(self.maxHealth * barrierFraction);
                    }
                }
                else if ((genericSkill.skillDef.autoHandleLuminousShot || self.canAddIncrasePrimaryDamage) && (object)self.skillLocator.secondary == genericSkill)
                {
                    self.healthComponent.AddBarrier(self.maxHealth * barrierFraction);
                }
            };
        }
    }
}