using System.Collections.Generic;
using Nautilus.Configuration;
using RoR2;

namespace Nautilus.Items
{
    /// <summary>
    ///     Item setup
    /// </summary>
    public static partial class ItemInit
    {
        public static List<ItemBase> ItemList = new List<ItemBase>();

        public static void Init()
        {
            foreach(ItemBase ib in ItemList)
            {
                if (ib.RegisterItem())
                {
                    Log.Info("Added definition for item " + ib.Name);
                    ib.RegisterHooks();
                }
            }
        }

        public static void FormatDescriptions()
        {
            foreach(ItemBase ib in ItemList)
            {
                if (ib.Enabled)
                {
                    ib.FormatDescriptionTokens();
                }
            }
        }
    }
}