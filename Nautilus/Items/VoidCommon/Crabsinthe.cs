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
        public static Crabsinthe Crabsinthe = new Crabsinthe
        (
            "Crabsinthe",
            [ItemTag.Healing],
            ItemTier.VoidTier1
        );
    }

    /// <summary>
    ///     // Ver.1
    ///     Crabsinthe is another iteration of the 'simple regen item' from Many Other Mods, but hey I wanted to make one too
    ///     The catch is that it corrupts both active and consumed elixirs, making the empty bottles useful and letting you stack up a lot of bonus regen
    ///     Helps counteract Monsoon regen reduction and synergizes with items that aren't often interactable like slug & knurl.
    /// </summary>
    public class Crabsinthe : ItemBase
    {
        public override bool Enabled => Crabsinthe_Enabled.Value;
        public override ItemDef ConversionItemDef => Addressables.LoadAssetAsync<ItemDef>("RoR2/DLC1/HealingPotion/HealingPotion.asset").WaitForCompletion();
        public ItemDef ConversionItemDefConsumed => Addressables.LoadAssetAsync<ItemDef>("RoR2/DLC1/HealingPotion/HealingPotionConsumed.asset").WaitForCompletion();

        public Crabsinthe(string _name, ItemTag[] _tags, ItemTier _tier, bool _canRemove = true, bool _isConsumed = false, bool _hidden = false) : 
        base(_name, _tags, _tier, _canRemove, _isConsumed, _hidden){}

        // Config
        public static ConfigItem<bool> Crabsinthe_Enabled = new ConfigItem<bool>
        (
            "Void common: Crabsinthe",
            "Item enabled",
            "Should this item appear in runs?",
            true
        );
        public static ConfigItem<float> Crabsinthe_Regen = new ConfigItem<float>
        (
            "Void common: Crabsinthe",
            "Regen boost",
            "Grants a regen boost with this multiplier.",
            0.25f,
            0f,
            6f,
            0.1f
        );
        public static ConfigItem<float> Crabsinthe_RegenStack = new ConfigItem<float>
        (
            "Void common: Crabsinthe",
            "Regen boost (Per stack)",
            "Grants a regen boost with this multiplier per additional stack.",
            0.25f,
            0f,
            6f,
            0.1f
        );
        public static ConfigItem<bool> Crabsinthe_CorruptBottles = new ConfigItem<bool>
        (
            "Void common: Crabsinthe",
            "Corrupt empty bottles",
            "Should this item corrupt consumed elixirs?",
            true
        );

        // Tokens
        public override void FormatDescriptionTokens()
        {
            string descriptionToken = ItemDef.descriptionToken;
            string extraConversionDesc = Crabsinthe_CorruptBottles.Value == true? " and Empty Bottles" : "";

            LanguageAPI.AddOverlay
            (
                descriptionToken,
                String.Format
                (
                    Language.currentLanguage.GetLocalizedStringByToken(descriptionToken),
                    Crabsinthe_Regen.Value * 100f,
                    Crabsinthe_RegenStack.Value * 100f,
                    extraConversionDesc
                )
            );
        }

        // Hooks
        public override void RegisterHooks()
        {
            // Additional void conversion
            if (Crabsinthe_CorruptBottles.Value)
            {
                ItemDef.Pair transformation = new()
                {
                    itemDef1 = ConversionItemDefConsumed,
                    itemDef2 = ItemDef
                };
                Main.ItemConversionList.Add(transformation);

                Log.Info(String.Format("Added void conversion from {0} to {1}", ConversionItemDefConsumed.name, ItemDef.name));
            }

            // Regen boost
            RecalculateStatsAPI.GetStatCoefficients += (orig, self) =>
            {
                int itemCount = GetItemCountEffective(orig);
                if (itemCount > 0)
                {
                    self.regenMultAdd += Crabsinthe_Regen.Value + (Crabsinthe_RegenStack.Value * (itemCount - 1));
                }
            };
        }
    }
}