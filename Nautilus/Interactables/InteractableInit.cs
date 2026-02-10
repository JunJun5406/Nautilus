using System.Collections.Generic;
using Nautilus.Configuration;
using RoR2;

namespace Nautilus.Interactables
{
    /// <summary>
    ///     Interactable setup
    /// </summary>
    public static partial class InteractableInit
    {
        public static List<InteractableBase> InteractableList = new List<InteractableBase>();

        public static void Init()
        {
            foreach(InteractableBase ib in InteractableList)
            {
                ib.Init();
            }
        }
    }
}