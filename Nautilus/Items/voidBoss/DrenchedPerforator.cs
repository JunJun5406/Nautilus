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
        public static DrenchedPerforator DrenchedPerforator = new DrenchedPerforator
        (
            "DrenchedPerforator",
            [ItemTag.Damage],
            ItemTier.VoidBoss
        );
    }

    /// <summary>
    ///     // Ver.1
    /// </summary>
    public class DrenchedPerforator : ItemBase
    {
        public override bool Enabled => DrenchedPerforator_Enabled.Value;
        public override ItemDef ConversionItemDef => Addressables.LoadAssetAsync<ItemDef>("RoR2/Base/FireballsOnHit/FireballsOnHit.asset").WaitForCompletion();
        public override GameObject itemPrefab => OverwritePrefabMaterials();
        public override Sprite itemIcon => Main.Assets.LoadAsset<Sprite>("Assets/icons/drenchedPerforator.png");
        public ItemDef ConversionItemDefExtra => Addressables.LoadAssetAsync<ItemDef>("RoR2/Base/LightningStrikeOnHit/LightningStrikeOnHit.asset").WaitForCompletion();
        public Material material0 => Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/TrimSheets/matTrimSheetMetalLightSnow.mat").WaitForCompletion();
        public Material material1 => Addressables.LoadAssetAsync<Material>("RoR2/Base/Croco/matCrocoSpine.mat").WaitForCompletion();
        private ExplicitPickupDropTable _explicitPickupDropTable;
        public ExplicitPickupDropTable explicitPickupDropTable
        {
            get
            {
                if (_explicitPickupDropTable == null)
                {
                    _explicitPickupDropTable = ScriptableObject.CreateInstance<ExplicitPickupDropTable>();
                    _explicitPickupDropTable.pickupEntries = new ExplicitPickupDropTable.PickupDefEntry[]
                    {
                        new ExplicitPickupDropTable.PickupDefEntry
                        {
                            pickupDef = ItemDef,
                            pickupWeight = 1
                        }
                    };
                }

                _explicitPickupDropTable.Regenerate(Run.instance);
                return _explicitPickupDropTable;
            }
            set;
        }

        public DrenchedPerforator(string _name, ItemTag[] _tags, ItemTier _tier, bool _canRemove = true, bool _isConsumed = false, bool _hidden = false) : 
        base(_name, _tags, _tier, _canRemove, _isConsumed, _hidden){}

        // Config
        public static ConfigItem<bool> DrenchedPerforator_Enabled = new ConfigItem<bool>
        (
            "Void boss: Drenched Perforator",
            "Item enabled",
            "Should this item appear in runs?",
            true
        );
        public static ConfigItem<float> DrenchedPerforator_Threshold = new ConfigItem<float>
        (
            "Void boss: Drenched Perforator",
            "Damage threshold",
            "Repeating fractional damage threshold for additional stacks of collapse to be added.",
            4f,
            1f,
            12f,
            0.5f
        );
        public static ConfigItem<int> DrenchedPerforator_Stacks = new ConfigItem<int>
        (
            "Void boss: Drenched Perforator",
            "Collapse stacks",
            "Number of stacks of collapse to add on passing threshold.",
            1,
            1f,
            5f,
            1f
        );
        public static ConfigItem<int> DrenchedPerforator_StacksStack = new ConfigItem<int>
        (
            "Void boss: Drenched Perforator",
            "Collapse stacks (per stack)",
            "Number of stacks of collapse to add on passing threshold, per additional stack.",
            1,
            1f,
            5f,
            1f
        );

        public GameObject OverwritePrefabMaterials()
        {
            GameObject ret = Main.Assets.LoadAsset<GameObject>("Assets/prefabs/drenchedPerforator.prefab");

            Material[] materials =
            {
                material0,
                material1
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
                    DrenchedPerforator_Threshold.Value * 100f,
                    DrenchedPerforator_Stacks.Value,
                    DrenchedPerforator_StacksStack.Value
                )
            );
        }

        // Hooks
        public override void RegisterHooks()
        {
            // Additional void conversion
            ItemDef.Pair transformation = new()
            {
                itemDef1 = ConversionItemDefExtra,
                itemDef2 = ItemDef
            };
            Main.ItemConversionList.Add(transformation);

            Log.Info(String.Format("Added void conversion from {0} to {1}", ConversionItemDefExtra.name, ItemDef.name));

            // On-hit trigger
            On.RoR2.GlobalEventManager.OnHitEnemy += (orig, self, damageInfo, victimObject) =>
            {
                orig(self, damageInfo, victimObject);

                if (!damageInfo.procChainMask.HasProc(ProcType.FractureOnHit) && !damageInfo.rejected && damageInfo.procCoefficient > 0f && damageInfo.damage > 0f && damageInfo.attacker && damageInfo.attacker.TryGetComponent(out CharacterBody attackerBody) && attackerBody.master && victimObject.TryGetComponent(out CharacterBody victimBody) && victimBody.healthComponent)
                {
                    int itemCount = GetItemCountEffective(attackerBody);
                    
                    if (itemCount > 0 && attackerBody.teamComponent && victimBody.teamComponent)
                    {
                        float damage = damageInfo.crit ? damageInfo.damage * attackerBody.critMultiplier : damageInfo.damage;
                        float damageFraction = damage / attackerBody.damage;
                        if (damageFraction > 0f)
                        {
                            int hits = Convert.ToInt32(Math.Floor(damageFraction / DrenchedPerforator_Threshold.Value));
                            int stacksToInflict = (DrenchedPerforator_Stacks.Value + ((itemCount - 1) * DrenchedPerforator_StacksStack.Value)) * hits;

                            if (stacksToInflict > 0)
                            {
                                DotController.DotDef dotDef = DotController.GetDotDef(DotController.DotIndex.Fracture);
                                for (int i = 0; i < stacksToInflict; i++)
                                {
                                    DotController.InflictDot(victimObject, damageInfo.attacker, damageInfo.inflictedHurtbox, DotController.DotIndex.Fracture, dotDef.interval);
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}