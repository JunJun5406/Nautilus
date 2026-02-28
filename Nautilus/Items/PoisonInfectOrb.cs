using UnityEngine;
using RoR2;
using RoR2.Orbs;

namespace Nautilus.Items
{
    public class PoisonInfectOrb : Orb
    {
        public override void Begin()
        {
            EffectData effectData = new EffectData
            {
                origin = origin,
                genericFloat = base.duration
            };
            effectData.SetHurtBoxReference(target);
            EffectManager.SpawnEffect(OrbStorageUtility.Get("Prefabs/Effects/OrbEffects/CrocoDiseaseOrbEffect"), effectData, transmit: true);
        }

        public static void CreateInfectOrb(Vector3 origin, HurtBox target)
        {
            PoisonInfectOrb orb = new PoisonInfectOrb();

            orb.duration = 0.5f;
            orb.origin = origin;
            orb.target = target;

            OrbManager.instance.AddOrb(orb);
        }
    }
}