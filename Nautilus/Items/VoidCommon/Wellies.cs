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
        public static Wellies Wellies = new Wellies
        (
            "Wellies",
            [ItemTag.Utility],
            ItemTier.VoidTier1
        );
    }

    /// <summary>
    ///     // Ver.1
    ///     Wellies provide a utility alternative to ultra-crit builds, instead letting you invalidate annoying flying enemies
    ///     In exchange for the proc conditions and worse stacking, Weaken is a very powerful debuff (30 armor reduction goes a long way) and makes Death Mark far easier to activate
    /// </summary>
    public class Wellies : ItemBase
    {
        public override bool Enabled => Wellies_Enabled.Value;
        public override ItemDef ConversionItemDef => Addressables.LoadAssetAsync<ItemDef>("RoR2/DLC3/Items/CritAtLowerElevation/CritAtLowerElevation.asset").WaitForCompletion();
        public override GameObject itemPrefab => null;
        public override Sprite itemIcon => null;
        public BuffDef DebuffDef => Addressables.LoadAssetAsync<BuffDef>("RoR2/Base/Treebot/bdWeak.asset").WaitForCompletion();

        public Wellies(string _name, ItemTag[] _tags, ItemTier _tier, bool _canRemove = true, bool _isConsumed = false, bool _hidden = false) : 
        base(_name, _tags, _tier, _canRemove, _isConsumed, _hidden){}

        // Config
        public static ConfigItem<bool> Wellies_Enabled = new ConfigItem<bool>
        (
            "Void common: Waterlogged Wellies",
            "Item enabled",
            "Should this item appear in runs?",
            true
        );
        public static ConfigItem<float> Wellies_Force = new ConfigItem<float>
        (
            "Void common: Waterlogged Wellies",
            "Downward pull force",
            "How strong the on-hit pull down effect is.",
            12f,
            0f,
            24f,
            0.5f
        );
        public static ConfigItem<float> Wellies_ForceStack = new ConfigItem<float>
        (
            "Void common: Waterlogged Wellies",
            "Downward pull force (Per stack)",
            "How strong the on-hit pull down effect is per additional stack.",
            6f,
            0f,
            24f,
            0.5f
        );
        public static ConfigItem<float> Wellies_DebuffSeconds = new ConfigItem<float>
        (
            "Void common: Waterlogged Wellies",
            "Debuff length",
            "How long the on-hit debuff should last in seconds.",
            2f,
            0f,
            12f,
            0.5f
        );
        public static ConfigItem<float> Wellies_DebuffSecondsStack = new ConfigItem<float>
        (
            "Void common: Waterlogged Wellies",
            "Debuff length (Per stack)",
            "How long the on-hit debuff should last in seconds per additional stack.",
            2f,
            0f,
            12f,
            0.5f
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
                    Wellies_DebuffSeconds.Value,
                    Wellies_DebuffSecondsStack.Value
                )
            );
        }

        // Hooks
        public override void RegisterHooks()
        {
            // On-hit trigger
            On.RoR2.HealthComponent.TakeDamageProcess += (orig, self, damageInfo) =>
            {
                if (damageInfo.attacker)
                {
                    CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                    CharacterBody victimBody = self.body;
                    
                    if (attackerBody && victimBody)
                    {
                        int itemCount = GetItemCountEffective(attackerBody);
                        if (itemCount > 0)
                        {
                            float buffLength = Wellies_DebuffSeconds.Value + (Wellies_DebuffSecondsStack.Value * (itemCount - 1));
                            float pullDownForce = (Wellies_Force.Value + (Wellies_ForceStack.Value * (itemCount - 1))) * damageInfo.procCoefficient;

                            if (pullDownForce > 0f)
                            {
                                PhysForceInfo physForceInfo = new PhysForceInfo
                                {
                                    force = Vector3.down * pullDownForce
                                };
                                
                                if (victimBody.TryGetComponent(out CharacterMotor victimMotor) && !victimMotor.isGrounded)
                                {
                                    victimMotor.ApplyForceImpulse(physForceInfo);
                                    victimBody.AddTimedBuff(DebuffDef.buffIndex, buffLength);
                                    Util.PlaySound("Play_voidBarnacle_m1_chargeUp", victimBody.gameObject);
                                }
                                else if (victimBody.TryGetComponent(out RigidbodyMotor victimRigidMotor))
                                {
                                    victimRigidMotor.ApplyForceImpulse(physForceInfo);
                                    victimBody.AddTimedBuff(DebuffDef.buffIndex, buffLength);
                                    Util.PlaySound("Play_voidBarnacle_m1_chargeUp", victimBody.gameObject);
                                }
                            }
                        }
                    }
                }

                orig(self, damageInfo);   
            };
        }
    }
}