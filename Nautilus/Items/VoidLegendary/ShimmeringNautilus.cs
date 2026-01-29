using RoR2;
using Nautilus.Configuration;
using System;
using R2API;
using UnityEngine;
using UnityEngine.AddressableAssets;
using HarmonyLib;
using RoR2.Orbs;

namespace Nautilus.Items
{
    public static partial class ItemInit
    {
        public static ShimmeringNautilus ShimmeringNautilus = new ShimmeringNautilus
        (
            "ShimmeringNautilus",
            [ItemTag.Damage, ItemTag.Utility, ItemTag.AIBlacklist],
            ItemTier.VoidTier3
        );
    }

    /// <summary>
    ///     // Ver.1
    ///     Shimmering Nautilus is a situationally strong tool, massively countering enemies that inflict DOT and giving you damage spikes against bosses.
    ///     Also provides a passive damage resistance that makes it worth picking up even if you're not in it for the damage.
    ///     Antler Shield got a glow-up
    /// </summary>
    public class ShimmeringNautilus : ItemBase
    {
        public override bool Enabled => ShimmeringNautilus_Enabled.Value;
        public override ItemDef ConversionItemDef => Addressables.LoadAssetAsync<ItemDef>("RoR2/Base/ArmorReductionOnHit/ArmorReductionOnHit.asset").WaitForCompletion();
        public override GameObject itemPrefab => null;
        public override Sprite itemIcon => null;
        public BuffDef NautilusBuff;

        public ShimmeringNautilus(string _name, ItemTag[] _tags, ItemTier _tier, bool _canRemove = true, bool _isConsumed = false, bool _hidden = false) : 
        base(_name, _tags, _tier, _canRemove, _isConsumed, _hidden){}

        // Config
        public static ConfigItem<bool> ShimmeringNautilus_Enabled = new ConfigItem<bool>
        (
            "Void legendary: Shimmering Nautilus",
            "Item enabled",
            "Should this item appear in runs?",
            true
        );
        public static ConfigItem<float> ShimmeringNautilus_DamageResist = new ConfigItem<float>
        (
            "Void legendary: Shimmering Nautilus",
            "Damage resistance",
            "Resist this fraction of all damage.",
            0.1f,
            0f,
            1f,
            0.05f
        );
        public static ConfigItem<int> ShimmeringNautilus_RetaliateHits = new ConfigItem<int>
        (
            "Void legendary: Shimmering Nautilus",
            "Retaliation hits",
            "Retaliation damage requires this many hits from the same enemy to trigger.",
            5,
            1f,
            20f,
            1f
        );
        public static ConfigItem<float> ShimmeringNautilus_RetaliateDamage = new ConfigItem<float>
        (
            "Void legendary: Shimmering Nautilus",
            "Retaliation damage",
            "Base damage percentage of retaliation hits.",
            1800f,
            100f,
            5000f,
            100f
        );
        public static ConfigItem<float> ShimmeringNautilus_RetaliateDamageStack = new ConfigItem<float>
        (
            "Void legendary: Shimmering Nautilus",
            "Retaliation damage (per stack)",
            "Base damage percentage of retaliation hits, per additional stack.",
            900f,
            100f,
            2500f,
            100f
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
                    ShimmeringNautilus_DamageResist.Value * 100f,
                    ShimmeringNautilus_RetaliateHits.Value,
                    ShimmeringNautilus_RetaliateDamage.Value,
                    ShimmeringNautilus_RetaliateDamageStack.Value
                )
            );
        }

        // Hooks
        public override void RegisterHooks()
        {
            // Damage resist, debuff application and retaliation
            On.RoR2.HealthComponent.TakeDamageProcess += (orig, self, damageInfo) =>
            {
                CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                CharacterBody victimBody = self.body;

                if (attackerBody && victimBody && attackerBody.teamComponent && victimBody.teamComponent)
                {
                    int itemCount = GetItemCountEffective(victimBody);

                    // Damage resist & debuff
                    if (itemCount > 0)
                    {
                        damageInfo.damage *= 1f - ShimmeringNautilus_DamageResist.Value;

                        if (attackerBody.teamComponent.teamIndex != victimBody.teamComponent.teamIndex)
                        {
                            attackerBody.AddBuff(NautilusBuff);
                        }

                        // Retaliation
                        if (attackerBody.GetBuffCount(NautilusBuff) >= ShimmeringNautilus_RetaliateHits.Value)
                        {
                            MissileVoidOrb missileVoidOrb = new MissileVoidOrb();
                            missileVoidOrb.origin = victimBody.aimOrigin;
                            missileVoidOrb.damageValue = victimBody.damage * ShimmeringNautilus_RetaliateDamage.Value + (ShimmeringNautilus_RetaliateDamageStack.Value * (itemCount - 1));
                            missileVoidOrb.teamIndex = victimBody.teamComponent.teamIndex;
                            missileVoidOrb.attacker = victimBody.gameObject;
                            missileVoidOrb.procCoefficient = 0f;
                            missileVoidOrb.damageColorIndex = DamageColorIndex.Void;
                            missileVoidOrb.scale = 3f;
                            HurtBox mainHurtBox = attackerBody.mainHurtBox;
                            if ((bool)mainHurtBox)
                            {
                                missileVoidOrb.target = mainHurtBox;
                                OrbManager.instance.AddOrb(missileVoidOrb);
                                attackerBody.ClearAllBuffs(NautilusBuff);
                            }
                        }
                    }
                }

                orig(self, damageInfo);
            };
        }

        public void CreateNautilusBuff()
        {
            BuffDef nautilusBuff = ScriptableObject.CreateInstance<BuffDef>();
            nautilusBuff.buffColor = new Color(1f, 0.35f, 0.8f);
            nautilusBuff.canStack = true;
            nautilusBuff.isDebuff = true;
            nautilusBuff.name = "Shimmering Nautilus stacks";
            nautilusBuff.isHidden = false;
            nautilusBuff.isCooldown = false;
            nautilusBuff.iconSprite = Main.Assets.LoadAsset<Sprite>("Assets/icons/nautilusBuff.png");
            ContentAddition.AddBuffDef(nautilusBuff);

            NautilusBuff = nautilusBuff;
        }
    }
}