using RoR2;
using Nautilus.Configuration;
using System;
using R2API;
using UnityEngine;
using UnityEngine.AddressableAssets;
using HarmonyLib;
using System.Threading;

namespace Nautilus.Items
{
    public static partial class ItemInit
    {
        public static VoidWatch VoidWatch = new VoidWatch
        (
            "VoidWatch",
            [ItemTag.Damage],
            ItemTier.VoidTier1
        );
    }

    /// <summary>
    ///     // Ver.1
    ///     Collector's Appraisal gives you a reason to stay at high health still, but avoids the 'all or nothing' nature of watches by making it unbreakable
    ///     Does not corrupt broken watches, too powerful
    ///     Adds synergy with barrier as it's rare to have a reason to stack barrier items
    /// </summary>
    public class VoidWatch : ItemBase
    {
        public override bool Enabled => VoidWatch_Enabled.Value;
        public override ItemDef ConversionItemDef => Addressables.LoadAssetAsync<ItemDef>("RoR2/DLC1/FragileDamageBonus/FragileDamageBonus.asset").WaitForCompletion();
        public override GameObject itemPrefab => OverwritePrefabMaterials();
        public Material material0 => Addressables.LoadAssetAsync<Material>("RoR2/Base/Treebot/matTreebotMetal.mat").WaitForCompletion();
        public Material material1 => Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/TrimSheets/matTrimSheetMetalGolden.mat").WaitForCompletion();
        public override Sprite itemIcon => Main.Assets.LoadAsset<Sprite>("Assets/icons/voidWatch.png");
        public BuffDef VoidWatchBuff;

        public VoidWatch(string _name, ItemTag[] _tags, ItemTier _tier, bool _canRemove = true, bool _isConsumed = false, bool _hidden = false) : 
        base(_name, _tags, _tier, _canRemove, _isConsumed, _hidden){}

        // Config
        public static ConfigItem<bool> VoidWatch_Enabled = new ConfigItem<bool>
        (
            "Void common: Collectors Appraisal",
            "Item enabled",
            "Should this item appear in runs?",
            true
        );
        public static ConfigItem<float> VoidWatch_Damage = new ConfigItem<float>
        (
            "Void common: Collectors Appraisal",
            "Damage boost",
            "Grants a damage boost with this multiplier.",
            0.10f,
            0f,
            1f,
            0.05f
        );
        public static ConfigItem<float> VoidWatch_DamageStack = new ConfigItem<float>
        (
            "Void common: Collectors Appraisal",
            "Damage boost (Per stack)",
            "Grants a damage boost with this multiplier per additional stack.",
            0.10f,
            0f,
            1f,
            0.05f
        );
        public static ConfigItem<float> VoidWatch_HealthThreshold = new ConfigItem<float>
        (
            "Void common: Collectors Appraisal",
            "Damage health threshold",
            "Health must be above this fraction for the damage boost to apply.",
            0.9f,
            0f,
            1f,
            0.05f
        );
        public static ConfigItem<bool> VoidWatch_BarrierMult = new ConfigItem<bool>
        (
            "Void common: Collectors Appraisal",
            "Damage scaling with barrier",
            "Adds up to 2x multiplier to the damage bonus scaling with barrier.",
            true
        );
        public static ConfigItem<bool> VoidWatch_HideBuff = new ConfigItem<bool>
        (
            "Void common: Collectors Appraisal",
            "Hide buff",
            "Do not display the buff for this item.",
            false
        );

        public GameObject OverwritePrefabMaterials()
        {
            GameObject ret = Main.Assets.LoadAsset<GameObject>("Assets/prefabs/voidWatch.prefab");

            Material[] materials =
            {
                material0,
                material1,
                material1,
            };
            ret.GetComponentInChildren<MeshRenderer>().SetMaterialArray(materials);

            return ret;
        }

        // Tokens
        public override void FormatDescriptionTokens()
        {
            string descriptionToken = ItemDef.descriptionToken;
            string extraBarrierDesc = VoidWatch_BarrierMult.Value == true ? " <style=cIsHealing>Additional temporary barrier multiplies this bonus up to 2x.</style>" : "";

            LanguageAPI.AddOverlay
            (
                descriptionToken,
                String.Format
                (
                    Language.currentLanguage.GetLocalizedStringByToken(descriptionToken),
                    VoidWatch_Damage.Value * 100f,
                    VoidWatch_DamageStack.Value * 100f,
                    VoidWatch_HealthThreshold.Value * 100f,
                    extraBarrierDesc
                )
            );
        }

        // Hooks
        public override void RegisterHooks()
        {
            CreateVoidWatchBuff();

            // Damage boost
            RecalculateStatsAPI.GetStatCoefficients += (orig, self) =>
            {
                int itemCount = GetItemCountEffective(orig);
                HealthComponent healthComponent = orig.healthComponent;

                if (healthComponent && itemCount > 0 && healthComponent.healthFraction >= VoidWatch_HealthThreshold.Value)
                {
                    if (!orig.HasBuff(VoidWatchBuff))
                    {
                        orig.AddBuff(VoidWatchBuff);
                    }

                    float barrierPercent = 0f;
                    if (healthComponent.barrier > 0f && orig.maxBarrier > 0f)
                    {
                        barrierPercent = healthComponent.barrier / orig.maxBarrier;
                    }

                    float baseDamageBonus = VoidWatch_Damage.Value + (VoidWatch_DamageStack.Value * (itemCount - 1));
                    float totalDamageBonus = baseDamageBonus + (VoidWatch_BarrierMult.Value == true ? baseDamageBonus * barrierPercent : 0f);
                    
                    self.damageMultAdd += totalDamageBonus;
                }
            };

            // Watch 'breaking' effect when falling below threshold
            On.RoR2.HealthComponent.TakeDamageProcess += (orig, self, damageInfo) =>
            {
                orig(self, damageInfo);

                if (self.body && self.body.HasBuff(VoidWatchBuff) && self.healthFraction < VoidWatch_HealthThreshold.Value)
                {
                    self.body.RemoveBuff(VoidWatchBuff);
                    Util.PlaySound("Play_item_proc_delicateWatch_break", self.gameObject);
                }
            };
        }

        public void CreateVoidWatchBuff()
        {
            BuffDef voidWatchBuff = ScriptableObject.CreateInstance<BuffDef>();
            voidWatchBuff.buffColor = new Color(0.76f, 0.3f, 0.92f);
            voidWatchBuff.canStack = false;
            voidWatchBuff.isDebuff = false;
            voidWatchBuff.ignoreGrowthNectar = true;
            voidWatchBuff.name = "Collector's Appraisal damage";
            voidWatchBuff.isHidden = VoidWatch_HideBuff.Value;
            voidWatchBuff.isCooldown = false;
            voidWatchBuff.iconSprite = Main.Assets.LoadAsset<Sprite>("Assets/icons/voidWatchBuff.png");
            ContentAddition.AddBuffDef(voidWatchBuff);

            VoidWatchBuff = voidWatchBuff;
        }
    }
}